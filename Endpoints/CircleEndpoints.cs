using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;

namespace WithinAPI.Endpoints;

public static class CircleEndpoints
{
    public static IEndpointRouteBuilder MapCircleEndpoints(this IEndpointRouteBuilder app)
    {
        var circles = app.MapGroup("/api/circles");

        circles.MapGet("", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.TryUserId();
            var items = await db.Circles
                .Where(circle => circle.Visibility == CircleVisibility.Public && circle.Status == CircleStatus.Active)
                .OrderBy(circle => circle.Name)
                .ToArrayAsync();
            return Results.Ok(await ToCircleDtos(db, items, userId));
        });

        circles.MapGet("/my", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var items = await (
                    from circle in db.Circles
                    join member in db.CircleMembers on circle.Id equals member.CircleId
                    where member.UserId == userId && member.Status == CircleMemberStatus.Active
                    orderby circle.Name
                    select circle)
                .ToArrayAsync();
            return Results.Ok(await ToCircleDtos(db, items, userId));
        }).RequireAuthorization();

        circles.MapGet("/{circleId:guid}", async (Guid circleId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.TryUserId();
            var circle = await db.Circles.FindAsync(circleId);
            if (circle is null || circle.Status != CircleStatus.Active) return Results.NotFound();

            var guidelines = await db.CircleGuidelines
                .Where(item => item.CircleId == circleId && item.IsActive)
                .OrderBy(item => item.SortOrder)
                .Select(item => new CircleGuidelineDto(item.Id, item.Title, item.Body, item.SortOrder))
                .ToArrayAsync();

            var latestThreads = await db.CircleThreads
                .Where(item => item.CircleId == circleId && item.Status != CommunityContentStatus.Hidden)
                .OrderByDescending(item => item.CreatedAt)
                .Take(5)
                .ToArrayAsync();

            var events = await SharedEventsQuery(db, circleId, userId).Take(5).ToArrayAsync();
            return Results.Ok(new CircleDetailDto(
                await ToCircleDto(db, circle, userId),
                guidelines,
                await ToThreadDtos(db, latestThreads, userId),
                events));
        });

        circles.MapPost("/{circleId:guid}/join", async (Guid circleId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!await db.Circles.AnyAsync(item => item.Id == circleId && item.Visibility == CircleVisibility.Public && item.Status == CircleStatus.Active))
            {
                return Results.NotFound();
            }

            var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == userId);
            if (member is null)
            {
                db.CircleMembers.Add(new CircleMember
                {
                    Id = Guid.NewGuid(),
                    CircleId = circleId,
                    UserId = userId,
                    Status = CircleMemberStatus.Active,
                    JoinedAt = DateTimeOffset.UtcNow
                });
            }
            else
            {
                member.Status = CircleMemberStatus.Active;
                member.LeftAt = null;
                if (member.JoinedAt == default) member.JoinedAt = DateTimeOffset.UtcNow;
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapDelete("/{circleId:guid}/leave", async (Guid circleId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == userId);
            if (member is null) return Results.NoContent();

            member.Status = CircleMemberStatus.Left;
            member.LeftAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapGet("/{circleId:guid}/threads", async (Guid circleId, int? page, int? pageSize, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await db.Circles.AnyAsync(item => item.Id == circleId && item.Status == CircleStatus.Active)) return Results.NotFound();
            var currentPage = Math.Max(page ?? 1, 1);
            var size = Math.Clamp(pageSize ?? 20, 1, 50);
            var userId = principal.TryUserId();
            var threads = await db.CircleThreads
                .Where(item => item.CircleId == circleId && item.Status != CommunityContentStatus.Hidden)
                .OrderByDescending(item => item.CreatedAt)
                .Skip((currentPage - 1) * size)
                .Take(size)
                .ToArrayAsync();
            return Results.Ok(await ToThreadDtos(db, threads, userId));
        });

        circles.MapGet("/threads", async (Guid? eventId, int? pageSize, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.TryUserId();
            var size = Math.Clamp(pageSize ?? 20, 1, 50);
            var query = db.CircleThreads.Where(item => item.Status != CommunityContentStatus.Hidden);
            if (eventId is not null) query = query.Where(item => item.LinkedEventId == eventId);
            var threads = await query
                .OrderByDescending(item => item.CreatedAt)
                .Take(size)
                .ToArrayAsync();
            return Results.Ok(await ToThreadDtos(db, threads, userId));
        });

        circles.MapPost("/{circleId:guid}/threads", async (Guid circleId, CircleCreateThreadDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!await IsCircleParticipant(db, circleId, userId) && !principal.IsInRole(nameof(WithinRole.Admin)))
            {
                return Results.Forbid();
            }

            var validation = ValidateThread(request.Title, request.Body);
            if (validation is not null) return Results.BadRequest(new { message = validation });

            if (request.LinkedEventId is not null && !await db.Events.AnyAsync(item => item.Id == request.LinkedEventId && item.Status == EventStatus.Published))
            {
                return Results.BadRequest(new { message = "Linked event was not found." });
            }

            var now = DateTimeOffset.UtcNow;
            var thread = new CircleThread
            {
                Id = Guid.NewGuid(),
                CircleId = circleId,
                UserId = userId,
                ThreadType = request.ThreadType,
                Title = request.Title.Trim(),
                Body = request.Body.Trim(),
                LinkedEventId = request.LinkedEventId,
                Status = CommunityContentStatus.Active,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.CircleThreads.Add(thread);
            await db.SaveChangesAsync();
            return Results.Created($"/api/circles/threads/{thread.Id}", await ToThreadDto(db, thread, userId));
        }).RequireAuthorization();

        circles.MapGet("/threads/{threadId:guid}", async (Guid threadId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var thread = await db.CircleThreads.FindAsync(threadId);
            if (thread is null || thread.Status == CommunityContentStatus.Hidden) return Results.NotFound();
            var userId = principal.TryUserId();
            var comments = await db.CircleThreadComments
                .Where(item => item.ThreadId == threadId && item.Status != CommunityContentStatus.Hidden)
                .OrderBy(item => item.CreatedAt)
                .ToArrayAsync();
            return Results.Ok(new CircleThreadDetailDto(await ToThreadDto(db, thread, userId), await ToCommentDtos(db, comments, userId)));
        });

        circles.MapPut("/threads/{threadId:guid}", async (Guid threadId, CircleUpdateThreadDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var thread = await db.CircleThreads.FindAsync(threadId);
            if (thread is null) return Results.NotFound();
            if (!await CanModerateOrOwn(db, principal, thread.CircleId, thread.UserId)) return Results.Forbid();

            var validation = ValidateThread(request.Title, request.Body);
            if (validation is not null) return Results.BadRequest(new { message = validation });

            thread.ThreadType = request.ThreadType;
            thread.Title = request.Title.Trim();
            thread.Body = request.Body.Trim();
            thread.LinkedEventId = request.LinkedEventId;
            thread.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(await ToThreadDto(db, thread, principal.UserId()));
        }).RequireAuthorization();

        circles.MapDelete("/threads/{threadId:guid}", async (Guid threadId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var thread = await db.CircleThreads.FindAsync(threadId);
            if (thread is null) return Results.NotFound();
            if (!await CanModerateOrOwn(db, principal, thread.CircleId, thread.UserId)) return Results.Forbid();
            thread.Status = CommunityContentStatus.Removed;
            thread.DeletedAt = DateTimeOffset.UtcNow;
            thread.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapPost("/threads/{threadId:guid}/comments", async (Guid threadId, CircleCreateCommentDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var thread = await db.CircleThreads.FindAsync(threadId);
            if (thread is null || thread.Status == CommunityContentStatus.Hidden) return Results.NotFound();
            if (!await IsCircleParticipant(db, thread.CircleId, userId) && !principal.IsInRole(nameof(WithinRole.Admin))) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Trim().Length > 1200)
            {
                return Results.BadRequest(new { message = "Comment body is required and must be 1200 characters or less." });
            }

            var now = DateTimeOffset.UtcNow;
            var comment = new CircleThreadComment
            {
                Id = Guid.NewGuid(),
                ThreadId = threadId,
                UserId = userId,
                Body = request.Body.Trim(),
                Status = CommunityContentStatus.Active,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.CircleThreadComments.Add(comment);
            await db.SaveChangesAsync();
            return Results.Created($"/api/circles/comments/{comment.Id}", await ToCommentDto(db, comment, userId));
        }).RequireAuthorization();

        circles.MapPut("/comments/{commentId:guid}", async (Guid commentId, CircleCreateCommentDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var comment = await db.CircleThreadComments.FindAsync(commentId);
            if (comment is null) return Results.NotFound();
            var thread = await db.CircleThreads.FindAsync(comment.ThreadId);
            if (thread is null) return Results.NotFound();
            if (!await CanModerateOrOwn(db, principal, thread.CircleId, comment.UserId)) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Trim().Length > 1200) return Results.BadRequest(new { message = "Comment body is required and must be 1200 characters or less." });
            comment.Body = request.Body.Trim();
            comment.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(await ToCommentDto(db, comment, principal.UserId()));
        }).RequireAuthorization();

        circles.MapDelete("/comments/{commentId:guid}", async (Guid commentId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var comment = await db.CircleThreadComments.FindAsync(commentId);
            if (comment is null) return Results.NotFound();
            var thread = await db.CircleThreads.FindAsync(comment.ThreadId);
            if (thread is null) return Results.NotFound();
            if (!await CanModerateOrOwn(db, principal, thread.CircleId, comment.UserId)) return Results.Forbid();
            comment.Status = CommunityContentStatus.Removed;
            comment.DeletedAt = DateTimeOffset.UtcNow;
            comment.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapPost("/threads/{threadId:guid}/helpful", async (Guid threadId, WithinDbContext db, ClaimsPrincipal principal) =>
            await SetHelpful(db, principal.UserId(), threadId, null)).RequireAuthorization();

        circles.MapDelete("/threads/{threadId:guid}/helpful", async (Guid threadId, WithinDbContext db, ClaimsPrincipal principal) =>
            await RemoveHelpful(db, principal.UserId(), threadId, null)).RequireAuthorization();

        circles.MapPost("/comments/{commentId:guid}/helpful", async (Guid commentId, WithinDbContext db, ClaimsPrincipal principal) =>
            await SetHelpful(db, principal.UserId(), null, commentId)).RequireAuthorization();

        circles.MapDelete("/comments/{commentId:guid}/helpful", async (Guid commentId, WithinDbContext db, ClaimsPrincipal principal) =>
            await RemoveHelpful(db, principal.UserId(), null, commentId)).RequireAuthorization();

        circles.MapGet("/{circleId:guid}/events", async (Guid circleId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.TryUserId();
            if (!await db.Circles.AnyAsync(item => item.Id == circleId && item.Status == CircleStatus.Active)) return Results.NotFound();
            return Results.Ok(await SharedEventsQuery(db, circleId, userId).ToArrayAsync());
        });

        circles.MapPost("/{circleId:guid}/events", async (Guid circleId, CircleShareEventDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!await IsCircleParticipant(db, circleId, userId) && !principal.IsInRole(nameof(WithinRole.Admin))) return Results.Forbid();
            var evt = await db.Events.FindAsync(request.EventId);
            if (evt is null || evt.Status != EventStatus.Published) return Results.BadRequest(new { message = "Event was not found." });

            var existing = await db.CircleEvents.FirstOrDefaultAsync(item => item.CircleId == circleId && item.EventId == request.EventId);
            if (existing is null)
            {
                db.CircleEvents.Add(new CircleEvent
                {
                    Id = Guid.NewGuid(),
                    CircleId = circleId,
                    EventId = request.EventId,
                    SharedByUserId = userId,
                    OptionalNote = request.OptionalNote?.Trim(),
                    Status = CircleEventStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
            else
            {
                existing.Status = CircleEventStatus.Active;
                existing.OptionalNote = request.OptionalNote?.Trim();
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapDelete("/{circleId:guid}/events/{eventId:guid}", async (Guid circleId, Guid eventId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await CanModerateCircle(db, principal, circleId)) return Results.Forbid();
            var share = await db.CircleEvents.FirstOrDefaultAsync(item => item.CircleId == circleId && item.EventId == eventId);
            if (share is null) return Results.NoContent();
            share.Status = CircleEventStatus.Removed;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapPost("/reports", async (CircleReportRequestDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var targets = new[] { request.ThreadId, request.CommentId, request.CircleEventId }.Count(item => item is not null);
            if (targets != 1) return Results.BadRequest(new { message = "Report must target exactly one thread, comment, or event share." });

            Guid circleId;
            if (request.ThreadId is not null)
            {
                var thread = await db.CircleThreads.FindAsync(request.ThreadId.Value);
                if (thread is null) return Results.NotFound();
                circleId = thread.CircleId;
            }
            else if (request.CommentId is not null)
            {
                var comment = await db.CircleThreadComments.FindAsync(request.CommentId.Value);
                if (comment is null) return Results.NotFound();
                var thread = await db.CircleThreads.FindAsync(comment.ThreadId);
                if (thread is null) return Results.NotFound();
                circleId = thread.CircleId;
            }
            else
            {
                var share = await db.CircleEvents.FindAsync(request.CircleEventId!.Value);
                if (share is null) return Results.NotFound();
                circleId = share.CircleId;
            }

            var report = new CircleReport
            {
                Id = Guid.NewGuid(),
                ReporterUserId = principal.UserId(),
                CircleId = circleId,
                ThreadId = request.ThreadId,
                CommentId = request.CommentId,
                CircleEventId = request.CircleEventId,
                Reason = request.Reason,
                Description = request.Description?.Trim(),
                Status = CommunityReportStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.CircleReports.Add(report);
            await db.SaveChangesAsync();
            return Results.Created($"/api/admin/circles/reports/{report.Id}", await ToReportDto(db, report, principal.UserId()));
        }).RequireAuthorization();

        var admin = app.MapGroup("/api/admin/circles").RequireAuthorization("AdminOnly");

        admin.MapGet("/reports", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var reports = await db.CircleReports.OrderByDescending(item => item.CreatedAt).Take(100).ToArrayAsync();
            var userId = principal.UserId();
            var response = new List<CircleReportDto>(reports.Length);
            foreach (var report in reports)
            {
                response.Add(await ToReportDto(db, report, userId));
            }
            return Results.Ok(response.ToArray());
        });

        admin.MapPost("/reports/{reportId:guid}/review", async (Guid reportId, CircleReviewReportDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (request.Status == CommunityReportStatus.Pending) return Results.BadRequest(new { message = "Choose a completed review status." });
            var report = await db.CircleReports.FindAsync(reportId);
            if (report is null) return Results.NotFound();
            report.Status = request.Status;
            report.ReviewedByUserId = principal.UserId();
            report.ReviewedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(await ToReportDto(db, report, principal.UserId()));
        });

        admin.MapPost("/threads/{threadId:guid}/remove", async (Guid threadId, WithinDbContext db) =>
        {
            var thread = await db.CircleThreads.FindAsync(threadId);
            if (thread is null) return Results.NotFound();
            thread.Status = CommunityContentStatus.Removed;
            thread.DeletedAt = DateTimeOffset.UtcNow;
            thread.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        admin.MapPost("/comments/{commentId:guid}/remove", async (Guid commentId, WithinDbContext db) =>
        {
            var comment = await db.CircleThreadComments.FindAsync(commentId);
            if (comment is null) return Results.NotFound();
            comment.Status = CommunityContentStatus.Removed;
            comment.DeletedAt = DateTimeOffset.UtcNow;
            comment.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        admin.MapPost("/events/{circleEventId:guid}/remove", async (Guid circleEventId, WithinDbContext db) =>
        {
            var share = await db.CircleEvents.FindAsync(circleEventId);
            if (share is null) return Results.NotFound();
            share.Status = CircleEventStatus.Removed;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }

    private static string? ValidateThread(string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 140) return "Title is required and must be 140 characters or less.";
        if (string.IsNullOrWhiteSpace(body) || body.Trim().Length > 4000) return "Body is required and must be 4000 characters or less.";
        return null;
    }

    private static async Task<bool> IsCircleParticipant(WithinDbContext db, Guid circleId, Guid userId) =>
        await db.CircleMembers.AnyAsync(item => item.CircleId == circleId && item.UserId == userId && item.Status == CircleMemberStatus.Active);

    private static async Task<bool> CanModerateCircle(WithinDbContext db, ClaimsPrincipal principal, Guid circleId)
    {
        if (principal.IsInRole(nameof(WithinRole.Admin))) return true;
        var userId = principal.UserId();
        return await db.CircleRoles.AnyAsync(item => item.CircleId == circleId && item.UserId == userId);
    }

    private static async Task<bool> CanModerateOrOwn(WithinDbContext db, ClaimsPrincipal principal, Guid circleId, Guid ownerUserId) =>
        principal.UserId() == ownerUserId || await CanModerateCircle(db, principal, circleId);

    private static async Task<IResult> SetHelpful(WithinDbContext db, Guid userId, Guid? threadId, Guid? commentId)
    {
        if (threadId is not null && !await db.CircleThreads.AnyAsync(item => item.Id == threadId)) return Results.NotFound();
        if (commentId is not null && !await db.CircleThreadComments.AnyAsync(item => item.Id == commentId)) return Results.NotFound();
        var exists = await db.CircleHelpfulReactions.AnyAsync(item => item.UserId == userId && item.ThreadId == threadId && item.CommentId == commentId);
        if (!exists)
        {
            db.CircleHelpfulReactions.Add(new CircleHelpfulReaction { Id = Guid.NewGuid(), UserId = userId, ThreadId = threadId, CommentId = commentId, CreatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();
        }
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveHelpful(WithinDbContext db, Guid userId, Guid? threadId, Guid? commentId)
    {
        await db.CircleHelpfulReactions
            .Where(item => item.UserId == userId && item.ThreadId == threadId && item.CommentId == commentId)
            .ExecuteDeleteAsync();
        return Results.NoContent();
    }

    private static IQueryable<EventDto> SharedEventsQuery(WithinDbContext db, Guid circleId, Guid? userId) =>
        ApiMapping.ProjectEvents(
            from evt in db.Events
            join share in db.CircleEvents on evt.Id equals share.EventId
            where share.CircleId == circleId && share.Status == CircleEventStatus.Active && evt.Status == EventStatus.Published
            orderby evt.StartUtc
            select evt,
            db,
            userId);

    private static async Task<CircleDto[]> ToCircleDtos(WithinDbContext db, Circle[] circles, Guid? currentUserId)
    {
        var response = new List<CircleDto>(circles.Length);
        foreach (var circle in circles)
        {
            response.Add(await ToCircleDto(db, circle, currentUserId));
        }
        return response.ToArray();
    }

    private static async Task<CircleDto> ToCircleDto(WithinDbContext db, Circle circle, Guid? currentUserId) => new(
        circle.Id,
        circle.Name,
        circle.Slug,
        circle.Description,
        circle.Type,
        circle.Visibility,
        circle.Status,
        circle.Lens,
        await db.CircleMembers.CountAsync(item => item.CircleId == circle.Id && item.Status == CircleMemberStatus.Active),
        await db.CircleThreads.CountAsync(item => item.CircleId == circle.Id && item.Status == CommunityContentStatus.Active),
        await db.CircleEvents.CountAsync(item => item.CircleId == circle.Id && item.Status == CircleEventStatus.Active),
        currentUserId is not null && await db.CircleMembers.AnyAsync(item => item.CircleId == circle.Id && item.UserId == currentUserId && item.Status == CircleMemberStatus.Active));

    private static async Task<CircleThreadDto[]> ToThreadDtos(WithinDbContext db, CircleThread[] threads, Guid? currentUserId)
    {
        var response = new List<CircleThreadDto>(threads.Length);
        foreach (var thread in threads)
        {
            response.Add(await ToThreadDto(db, thread, currentUserId));
        }
        return response.ToArray();
    }

    private static async Task<CircleThreadDto> ToThreadDto(WithinDbContext db, CircleThread thread, Guid? currentUserId)
    {
        var circleName = await db.Circles.Where(item => item.Id == thread.CircleId).Select(item => item.Name).FirstOrDefaultAsync() ?? "Circle";
        var author = await ToAuthorDto(db, thread.UserId);
        var body = thread.Status == CommunityContentStatus.Removed ? "This thread has been removed." : thread.Body;
        var title = thread.Status == CommunityContentStatus.Removed ? "Removed thread" : thread.Title;
        return new CircleThreadDto(
            thread.Id,
            thread.CircleId,
            circleName,
            thread.ThreadType,
            title,
            body,
            thread.Status,
            author,
            await ToEventSummary(db, thread.LinkedEventId),
            await db.CircleHelpfulReactions.CountAsync(item => item.ThreadId == thread.Id),
            await db.CircleThreadComments.CountAsync(item => item.ThreadId == thread.Id && item.Status == CommunityContentStatus.Active),
            currentUserId is not null && await db.CircleHelpfulReactions.AnyAsync(item => item.ThreadId == thread.Id && item.UserId == currentUserId),
            thread.CreatedAt,
            thread.UpdatedAt);
    }

    private static async Task<CircleThreadCommentDto[]> ToCommentDtos(WithinDbContext db, CircleThreadComment[] comments, Guid? currentUserId)
    {
        var response = new List<CircleThreadCommentDto>(comments.Length);
        foreach (var comment in comments)
        {
            response.Add(await ToCommentDto(db, comment, currentUserId));
        }
        return response.ToArray();
    }

    private static async Task<CircleThreadCommentDto> ToCommentDto(WithinDbContext db, CircleThreadComment comment, Guid? currentUserId)
    {
        var body = comment.Status == CommunityContentStatus.Removed ? "This comment has been removed." : comment.Body;
        return new CircleThreadCommentDto(
            comment.Id,
            comment.ThreadId,
            body,
            comment.Status,
            await ToAuthorDto(db, comment.UserId),
            await db.CircleHelpfulReactions.CountAsync(item => item.CommentId == comment.Id),
            currentUserId is not null && await db.CircleHelpfulReactions.AnyAsync(item => item.CommentId == comment.Id && item.UserId == currentUserId),
            comment.CreatedAt,
            comment.UpdatedAt);
    }

    private static async Task<CircleReportDto> ToReportDto(WithinDbContext db, CircleReport report, Guid? currentUserId)
    {
        var circle = await db.Circles.FindAsync(report.CircleId);
        var thread = report.ThreadId is null ? null : await db.CircleThreads.FindAsync(report.ThreadId.Value);
        CircleThreadComment? comment = null;
        if (report.CommentId is not null) comment = await db.CircleThreadComments.FindAsync(report.CommentId.Value);

        EventDto? sharedEvent = null;
        if (report.CircleEventId is not null)
        {
            var eventId = await db.CircleEvents.Where(item => item.Id == report.CircleEventId.Value).Select(item => (Guid?)item.EventId).FirstOrDefaultAsync();
            if (eventId is not null)
            {
                sharedEvent = await ApiMapping.ProjectEvents(db.Events.Where(item => item.Id == eventId.Value), db, currentUserId).FirstOrDefaultAsync();
            }
        }

        return new CircleReportDto(
            report.Id,
            report.CircleId,
            report.CircleEventId,
            circle?.Name ?? "Circle",
            report.Reason,
            report.Description,
            report.Status,
            thread is null ? null : await ToThreadDto(db, thread, currentUserId),
            comment is null ? null : await ToCommentDto(db, comment, currentUserId),
            sharedEvent,
            await ToAuthorDto(db, report.ReporterUserId),
            report.ReviewedByUserId is null ? null : await ToAuthorDto(db, report.ReviewedByUserId.Value),
            report.CreatedAt,
            report.ReviewedAt);
    }

    private static async Task<CommunityAuthorDto> ToAuthorDto(WithinDbContext db, Guid userId)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null) return new CommunityAuthorDto(userId, "Unknown user", WithinRole.User, false);
        var verified = user.Role == WithinRole.Provider && await db.Providers.AnyAsync(item => item.OwnerUserId == userId && item.IsVerified);
        return new CommunityAuthorDto(user.Id, user.DisplayName, user.Role, verified);
    }

    private static async Task<CommunityEventSummaryDto?> ToEventSummary(WithinDbContext db, Guid? eventId)
    {
        if (eventId is null) return null;
        return await (
                from evt in db.Events
                join provider in db.Providers on evt.ProviderId equals provider.Id
                where evt.Id == eventId
                select new CommunityEventSummaryDto(evt.Id, evt.Title, provider.Name, evt.StartUtc, evt.LocationName))
            .FirstOrDefaultAsync();
    }
}
