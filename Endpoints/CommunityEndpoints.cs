using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class CommunityEndpoints
{
    private const int MaxPageSize = 50;

    public static IEndpointRouteBuilder MapCommunityEndpoints(this IEndpointRouteBuilder app)
    {
        var community = app.MapGroup("/api/community");

        community.MapGet("/topics", async (WithinDbContext db) =>
            Results.Ok(await db.CommunityTopics
                .Where(item => item.IsActive)
                .OrderBy(item => item.Name)
                .Select(item => new CommunityTopicDto(item.Id, item.Name, item.Slug, item.Description, item.IsActive))
                .ToArrayAsync()));

        community.MapGet("/posts", async (
            WithinDbContext db,
            ClaimsPrincipal principal,
            int? page,
            int? pageSize,
            CommunityPostType? postType,
            Guid? topicId,
            Guid? eventId,
            Guid? userId,
            string? sort) =>
        {
            var currentUserId = principal.TryUserId();
            var query = db.CommunityPosts.AsQueryable();
            query = query.Where(item => !item.IsDeleted && item.Status != CommunityContentStatus.Hidden);
            if (postType is not null) query = query.Where(item => item.PostType == postType);
            if (eventId is not null) query = query.Where(item => item.LinkedEventId == eventId);
            if (userId is not null) query = query.Where(item => item.UserId == userId);
            if (topicId is not null) query = query.Where(item => db.CommunityPostTopics.Any(topic => topic.PostId == item.Id && topic.TopicId == topicId));

            query = string.Equals(sort, "helpful", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(item => db.CommunityHelpfulReactions.Count(reaction => reaction.PostId == item.Id)).ThenByDescending(item => item.CreatedAt)
                : query.OrderByDescending(item => item.CreatedAt);

            var safePage = Math.Max(page ?? 1, 1);
            var safePageSize = Math.Clamp(pageSize ?? 20, 1, MaxPageSize);
            var posts = await query.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArrayAsync();
            return Results.Ok(await ToPostDtos(db, posts, currentUserId));
        });

        community.MapGet("/posts/{postId:guid}", async (Guid postId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var currentUserId = principal.TryUserId();
            var post = await db.CommunityPosts.FirstOrDefaultAsync(item => item.Id == postId && item.Status != CommunityContentStatus.Hidden);
            if (post is null) return Results.NotFound();
            var comments = await db.CommunityComments
                .Where(item => item.PostId == postId && !item.IsDeleted && item.Status != CommunityContentStatus.Hidden)
                .OrderBy(item => item.CreatedAt)
                .ToArrayAsync();
            return Results.Ok(new CommunityPostDetailDto(
                await ToPostDto(db, post, currentUserId),
                await ToCommentDtos(db, comments, currentUserId)));
        });

        community.MapPost("/posts", async (CommunityCreatePostDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var validation = await ValidatePostRequest(db, request.Title, request.Body, request.TopicIds, request.LinkedEventId);
            if (validation is not null) return validation;

            var now = DateTimeOffset.UtcNow;
            var post = new CommunityPost
            {
                Id = Guid.NewGuid(),
                UserId = principal.UserId(),
                PostType = request.PostType,
                Title = request.Title.Trim(),
                Body = request.Body.Trim(),
                LinkedEventId = request.LinkedEventId,
                Status = CommunityContentStatus.Active,
                CreatedAt = now,
                UpdatedAt = now
            };

            db.CommunityPosts.Add(post);
            foreach (var topicId in request.TopicIds.Distinct())
            {
                db.CommunityPostTopics.Add(new CommunityPostTopic { PostId = post.Id, TopicId = topicId });
            }
            await db.SaveChangesAsync();
            return Results.Created($"/api/community/posts/{post.Id}", await ToPostDto(db, post, principal.UserId()));
        }).RequireAuthorization();

        community.MapPut("/posts/{postId:guid}", async (Guid postId, CommunityUpdatePostDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var post = await db.CommunityPosts.FindAsync(postId);
            if (post is null) return Results.NotFound();
            if (!CanManage(principal, post.UserId)) return Results.Forbid();
            if (post.Status == CommunityContentStatus.Removed && !principal.IsInRole(nameof(WithinRole.Admin)))
            {
                return Results.BadRequest(new { message = "Removed posts cannot be updated." });
            }

            var validation = await ValidatePostRequest(db, request.Title, request.Body, request.TopicIds, request.LinkedEventId);
            if (validation is not null) return validation;

            post.PostType = request.PostType;
            post.Title = request.Title.Trim();
            post.Body = request.Body.Trim();
            post.LinkedEventId = request.LinkedEventId;
            post.UpdatedAt = DateTimeOffset.UtcNow;
            await db.CommunityPostTopics.Where(item => item.PostId == postId).ExecuteDeleteAsync();
            foreach (var topicId in request.TopicIds.Distinct())
            {
                db.CommunityPostTopics.Add(new CommunityPostTopic { PostId = post.Id, TopicId = topicId });
            }
            await db.SaveChangesAsync();
            return Results.Ok(await ToPostDto(db, post, principal.UserId()));
        }).RequireAuthorization();

        community.MapDelete("/posts/{postId:guid}", async (Guid postId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var post = await db.CommunityPosts.FindAsync(postId);
            if (post is null) return Results.NotFound();
            if (!CanManage(principal, post.UserId)) return Results.Forbid();
            post.IsDeleted = true;
            post.Status = CommunityContentStatus.Removed;
            post.DeletedAt = DateTimeOffset.UtcNow;
            post.UpdatedAt = post.DeletedAt.Value;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        community.MapGet("/posts/{postId:guid}/comments", async (Guid postId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await db.CommunityPosts.AnyAsync(item => item.Id == postId)) return Results.NotFound();
            var comments = await db.CommunityComments
                .Where(item => item.PostId == postId && !item.IsDeleted && item.Status != CommunityContentStatus.Hidden)
                .OrderBy(item => item.CreatedAt)
                .ToArrayAsync();
            return Results.Ok(await ToCommentDtos(db, comments, principal.TryUserId()));
        });

        community.MapPost("/posts/{postId:guid}/comments", async (Guid postId, CommunityCreateCommentDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var post = await db.CommunityPosts.FirstOrDefaultAsync(item => item.Id == postId && item.Status == CommunityContentStatus.Active && !item.IsDeleted);
            if (post is null) return Results.NotFound();
            var body = request.Body.Trim();
            if (string.IsNullOrWhiteSpace(body)) return Results.BadRequest(new { message = "Comment body is required." });
            if (body.Length > 1000) return Results.BadRequest(new { message = "Comment body must be 1000 characters or fewer." });
            var now = DateTimeOffset.UtcNow;
            var comment = new CommunityComment
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = principal.UserId(),
                Body = body,
                Status = CommunityContentStatus.Active,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.CommunityComments.Add(comment);
            await db.SaveChangesAsync();
            return Results.Created($"/api/community/comments/{comment.Id}", await ToCommentDto(db, comment, principal.UserId()));
        }).RequireAuthorization();

        community.MapPut("/comments/{commentId:guid}", async (Guid commentId, CommunityCreateCommentDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var comment = await db.CommunityComments.FindAsync(commentId);
            if (comment is null) return Results.NotFound();
            if (!CanManage(principal, comment.UserId)) return Results.Forbid();
            if (comment.Status == CommunityContentStatus.Removed && !principal.IsInRole(nameof(WithinRole.Admin)))
            {
                return Results.BadRequest(new { message = "Removed comments cannot be updated." });
            }
            var body = request.Body.Trim();
            if (string.IsNullOrWhiteSpace(body)) return Results.BadRequest(new { message = "Comment body is required." });
            if (body.Length > 1000) return Results.BadRequest(new { message = "Comment body must be 1000 characters or fewer." });
            comment.Body = body;
            comment.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(await ToCommentDto(db, comment, principal.UserId()));
        }).RequireAuthorization();

        community.MapDelete("/comments/{commentId:guid}", async (Guid commentId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var comment = await db.CommunityComments.FindAsync(commentId);
            if (comment is null) return Results.NotFound();
            if (!CanManage(principal, comment.UserId)) return Results.Forbid();
            comment.IsDeleted = true;
            comment.Status = CommunityContentStatus.Removed;
            comment.DeletedAt = DateTimeOffset.UtcNow;
            comment.UpdatedAt = comment.DeletedAt.Value;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        community.MapPost("/posts/{postId:guid}/helpful", async (Guid postId, WithinDbContext db, ClaimsPrincipal principal) =>
            await SetHelpful(db, principal.UserId(), postId, null)).RequireAuthorization();

        community.MapDelete("/posts/{postId:guid}/helpful", async (Guid postId, WithinDbContext db, ClaimsPrincipal principal) =>
            await RemoveHelpful(db, principal.UserId(), postId, null)).RequireAuthorization();

        community.MapPost("/comments/{commentId:guid}/helpful", async (Guid commentId, WithinDbContext db, ClaimsPrincipal principal) =>
            await SetHelpful(db, principal.UserId(), null, commentId)).RequireAuthorization();

        community.MapDelete("/comments/{commentId:guid}/helpful", async (Guid commentId, WithinDbContext db, ClaimsPrincipal principal) =>
            await RemoveHelpful(db, principal.UserId(), null, commentId)).RequireAuthorization();

        community.MapPost("/posts/{postId:guid}/save", async (Guid postId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!await db.CommunityPosts.AnyAsync(item => item.Id == postId)) return Results.NotFound();
            if (!await db.SavedCommunityPosts.AnyAsync(item => item.PostId == postId && item.UserId == userId))
            {
                db.SavedCommunityPosts.Add(new SavedCommunityPost { Id = Guid.NewGuid(), PostId = postId, UserId = userId, CreatedAt = DateTimeOffset.UtcNow });
                await db.SaveChangesAsync();
            }
            return Results.NoContent();
        }).RequireAuthorization();

        community.MapDelete("/posts/{postId:guid}/save", async (Guid postId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            await db.SavedCommunityPosts.Where(item => item.PostId == postId && item.UserId == principal.UserId()).ExecuteDeleteAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        community.MapGet("/saved-posts", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var posts = await db.SavedCommunityPosts
                .Where(item => item.UserId == userId)
                .Join(db.CommunityPosts, saved => saved.PostId, post => post.Id, (_, post) => post)
                .Where(post => !post.IsDeleted && post.Status != CommunityContentStatus.Hidden)
                .OrderByDescending(post => post.CreatedAt)
                .ToArrayAsync();
            return Results.Ok(await ToPostDtos(db, posts, userId));
        }).RequireAuthorization();

        community.MapPost("/reports", async (CommunityReportRequestDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var hasPost = request.PostId is not null;
            var hasComment = request.CommentId is not null;
            if (hasPost == hasComment) return Results.BadRequest(new { message = "Report exactly one post or comment." });
            if (hasPost && !await db.CommunityPosts.AnyAsync(item => item.Id == request.PostId)) return Results.NotFound();
            if (hasComment && !await db.CommunityComments.AnyAsync(item => item.Id == request.CommentId)) return Results.NotFound();
            db.CommunityReports.Add(new CommunityReport
            {
                Id = Guid.NewGuid(),
                ReportedByUserId = principal.UserId(),
                PostId = request.PostId,
                CommentId = request.CommentId,
                Reason = request.Reason,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Status = CommunityReportStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        MapAdminCommunityEndpoints(app);
        return app;
    }

    private static void MapAdminCommunityEndpoints(IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/admin/community").RequireAuthorization("AdminOnly");

        admin.MapGet("/reports", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var reports = await db.CommunityReports.OrderByDescending(item => item.CreatedAt).ToArrayAsync();
            var currentUserId = principal.UserId();
            var dtos = new List<CommunityReportDto>();
            foreach (var report in reports)
            {
                dtos.Add(await ToReportDto(db, report, currentUserId));
            }
            return Results.Ok(dtos);
        });

        admin.MapPost("/reports/{reportId:guid}/review", async (Guid reportId, CommunityReviewReportDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var report = await db.CommunityReports.FindAsync(reportId);
            if (report is null) return Results.NotFound();
            if (request.Status is CommunityReportStatus.Pending)
            {
                return Results.BadRequest(new { message = "Use Reviewed, ActionTaken, or Dismissed." });
            }
            report.Status = request.Status;
            report.ReviewedByUserId = principal.UserId();
            report.ReviewedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(await ToReportDto(db, report, principal.UserId()));
        });

        admin.MapPost("/posts/{postId:guid}/remove", async (Guid postId, WithinDbContext db) =>
        {
            var post = await db.CommunityPosts.FindAsync(postId);
            if (post is null) return Results.NotFound();
            post.Status = CommunityContentStatus.Removed;
            post.IsDeleted = true;
            post.DeletedAt = DateTimeOffset.UtcNow;
            post.UpdatedAt = post.DeletedAt.Value;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        admin.MapPost("/comments/{commentId:guid}/remove", async (Guid commentId, WithinDbContext db) =>
        {
            var comment = await db.CommunityComments.FindAsync(commentId);
            if (comment is null) return Results.NotFound();
            comment.Status = CommunityContentStatus.Removed;
            comment.IsDeleted = true;
            comment.DeletedAt = DateTimeOffset.UtcNow;
            comment.UpdatedAt = comment.DeletedAt.Value;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // Master data: community topics are managed from the admin portal (no seed scripts).
        admin.MapGet("/topics", async (WithinDbContext db) =>
            Results.Ok(await db.CommunityTopics
                .OrderBy(item => item.Name)
                .Select(item => new CommunityTopicDto(item.Id, item.Name, item.Slug, item.Description, item.IsActive))
                .ToArrayAsync()));

        admin.MapPost("/topics", async (CreateCommunityTopicRequest request, WithinDbContext db) =>
        {
            var name = request.Name?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(name) || name.Length > 80)
            {
                return Results.BadRequest(new { message = "Topic name is required and must be 80 characters or less." });
            }

            var topic = new CommunityTopic
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = await UniqueTopicSlug(db, name),
                Description = NormalizeTopicDescription(request.Description),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.CommunityTopics.Add(topic);
            await db.SaveChangesAsync();
            return Results.Created($"/api/admin/community/topics/{topic.Id}",
                new CommunityTopicDto(topic.Id, topic.Name, topic.Slug, topic.Description, topic.IsActive));
        });

        admin.MapPut("/topics/{topicId:guid}", async (Guid topicId, UpdateCommunityTopicRequest request, WithinDbContext db) =>
        {
            var topic = await db.CommunityTopics.FindAsync(topicId);
            if (topic is null) return Results.NotFound();

            var name = request.Name?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(name) || name.Length > 80)
            {
                return Results.BadRequest(new { message = "Topic name is required and must be 80 characters or less." });
            }

            // Slug stays immutable after creation so existing links keep working.
            topic.Name = name;
            topic.Description = NormalizeTopicDescription(request.Description);
            topic.IsActive = request.IsActive;
            await db.SaveChangesAsync();
            return Results.Ok(new CommunityTopicDto(topic.Id, topic.Name, topic.Slug, topic.Description, topic.IsActive));
        });

        admin.MapDelete("/topics/{topicId:guid}", async (Guid topicId, WithinDbContext db) =>
        {
            var topic = await db.CommunityTopics.FindAsync(topicId);
            if (topic is null) return Results.NotFound();
            topic.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static string? NormalizeTopicDescription(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return null;
        return trimmed.Length > 240 ? trimmed[..240] : trimmed;
    }

    private static async Task<string> UniqueTopicSlug(WithinDbContext db, string name)
    {
        var baseSlug = Slugs.From(name);
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = $"topic-{Guid.NewGuid():N}"[..16];
        if (baseSlug.Length > 78) baseSlug = baseSlug[..78];

        var slug = baseSlug;
        var suffix = 2;
        while (await db.CommunityTopics.AnyAsync(item => item.Slug == slug))
        {
            slug = $"{baseSlug}-{suffix++}";
        }
        return slug;
    }

    private static async Task<IResult> SetHelpful(WithinDbContext db, Guid userId, Guid? postId, Guid? commentId)
    {
        if (postId is not null && !await db.CommunityPosts.AnyAsync(item => item.Id == postId)) return Results.NotFound();
        if (commentId is not null && !await db.CommunityComments.AnyAsync(item => item.Id == commentId)) return Results.NotFound();
        var exists = await db.CommunityHelpfulReactions.AnyAsync(item => item.UserId == userId && item.PostId == postId && item.CommentId == commentId);
        if (!exists)
        {
            db.CommunityHelpfulReactions.Add(new CommunityHelpfulReaction { Id = Guid.NewGuid(), UserId = userId, PostId = postId, CommentId = commentId, CreatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();
        }
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveHelpful(WithinDbContext db, Guid userId, Guid? postId, Guid? commentId)
    {
        await db.CommunityHelpfulReactions.Where(item => item.UserId == userId && item.PostId == postId && item.CommentId == commentId).ExecuteDeleteAsync();
        return Results.NoContent();
    }

    private static async Task<IResult?> ValidatePostRequest(WithinDbContext db, string title, string body, Guid[] topicIds, Guid? linkedEventId)
    {
        if (string.IsNullOrWhiteSpace(title)) return Results.BadRequest(new { message = "Title is required." });
        if (title.Trim().Length > 120) return Results.BadRequest(new { message = "Title must be 120 characters or fewer." });
        if (string.IsNullOrWhiteSpace(body)) return Results.BadRequest(new { message = "Body is required." });
        if (body.Trim().Length > 3000) return Results.BadRequest(new { message = "Body must be 3000 characters or fewer." });
        var distinctTopicIds = topicIds.Distinct().ToArray();
        if (distinctTopicIds.Length > 3) return Results.BadRequest(new { message = "Choose up to 3 topics." });
        if (distinctTopicIds.Length > 0)
        {
            var count = await db.CommunityTopics.CountAsync(item => distinctTopicIds.Contains(item.Id) && item.IsActive);
            if (count != distinctTopicIds.Length) return Results.BadRequest(new { message = "One or more topics were not found." });
        }
        if (linkedEventId is not null && !await db.Events.AnyAsync(item => item.Id == linkedEventId))
        {
            return Results.BadRequest(new { message = "Linked event was not found." });
        }
        return null;
    }

    private static bool CanManage(ClaimsPrincipal principal, Guid ownerUserId) =>
        principal.IsInRole(nameof(WithinRole.Admin)) || principal.UserId() == ownerUserId;

    private static async Task<CommunityPostDto[]> ToPostDtos(WithinDbContext db, CommunityPost[] posts, Guid? currentUserId)
    {
        var result = new List<CommunityPostDto>();
        foreach (var post in posts)
        {
            result.Add(await ToPostDto(db, post, currentUserId));
        }
        return result.ToArray();
    }

    private static async Task<CommunityPostDto> ToPostDto(WithinDbContext db, CommunityPost post, Guid? currentUserId)
    {
        var author = await ToAuthorDto(db, post.UserId);
        var topicIds = await db.CommunityPostTopics.Where(item => item.PostId == post.Id).Select(item => item.TopicId).ToArrayAsync();
        var topics = await db.CommunityTopics
            .Where(item => topicIds.Contains(item.Id))
            .OrderBy(item => item.Name)
            .Select(item => new CommunityTopicDto(item.Id, item.Name, item.Slug, item.Description, item.IsActive))
            .ToArrayAsync();
        var linkedEvent = post.LinkedEventId is null ? null : await ToEventSummaryDto(db, post.LinkedEventId.Value);
        var body = post.Status == CommunityContentStatus.Removed ? "This post has been removed." : post.Body;
        return new CommunityPostDto(
            post.Id,
            post.PostType,
            post.Title,
            body,
            post.Status,
            author,
            topics,
            linkedEvent,
            await db.CommunityHelpfulReactions.CountAsync(item => item.PostId == post.Id),
            await db.CommunityComments.CountAsync(item => item.PostId == post.Id && !item.IsDeleted && item.Status != CommunityContentStatus.Hidden),
            await db.SavedCommunityPosts.CountAsync(item => item.PostId == post.Id),
            currentUserId is not null && await db.CommunityHelpfulReactions.AnyAsync(item => item.PostId == post.Id && item.UserId == currentUserId),
            currentUserId is not null && await db.SavedCommunityPosts.AnyAsync(item => item.PostId == post.Id && item.UserId == currentUserId),
            post.CreatedAt,
            post.UpdatedAt);
    }

    private static async Task<CommunityCommentDto[]> ToCommentDtos(WithinDbContext db, CommunityComment[] comments, Guid? currentUserId)
    {
        var result = new List<CommunityCommentDto>();
        foreach (var comment in comments)
        {
            result.Add(await ToCommentDto(db, comment, currentUserId));
        }
        return result.ToArray();
    }

    private static async Task<CommunityCommentDto> ToCommentDto(WithinDbContext db, CommunityComment comment, Guid? currentUserId)
    {
        var body = comment.Status == CommunityContentStatus.Removed ? "This comment has been removed." : comment.Body;
        return new CommunityCommentDto(
            comment.Id,
            comment.PostId,
            body,
            comment.Status,
            await ToAuthorDto(db, comment.UserId),
            await db.CommunityHelpfulReactions.CountAsync(item => item.CommentId == comment.Id),
            currentUserId is not null && await db.CommunityHelpfulReactions.AnyAsync(item => item.CommentId == comment.Id && item.UserId == currentUserId),
            comment.CreatedAt,
            comment.UpdatedAt);
    }

    private static async Task<CommunityReportDto> ToReportDto(WithinDbContext db, CommunityReport report, Guid? currentUserId)
    {
        var post = report.PostId is null ? null : await db.CommunityPosts.FindAsync(report.PostId);
        var comment = report.CommentId is null ? null : await db.CommunityComments.FindAsync(report.CommentId);
        return new CommunityReportDto(
            report.Id,
            report.Reason,
            report.Description,
            report.Status,
            post is null ? null : await ToPostDto(db, post, currentUserId),
            comment is null ? null : await ToCommentDto(db, comment, currentUserId),
            await ToAuthorDto(db, report.ReportedByUserId),
            report.ReviewedByUserId is null ? null : await ToAuthorDto(db, report.ReviewedByUserId.Value),
            report.CreatedAt,
            report.ReviewedAt);
    }

    private static async Task<CommunityAuthorDto> ToAuthorDto(WithinDbContext db, Guid userId)
    {
        var user = await db.Users.FindAsync(userId) ?? throw new InvalidOperationException("Community author missing.");
        var isVerifiedProvider = user.Role == WithinRole.Provider && await db.Providers.AnyAsync(item => item.OwnerUserId == userId && item.IsVerified);
        return new CommunityAuthorDto(user.Id, user.DisplayName, user.Role, isVerifiedProvider);
    }

    private static async Task<CommunityEventSummaryDto?> ToEventSummaryDto(WithinDbContext db, Guid eventId)
    {
        var result = await (
            from evt in db.Events
            join provider in db.Providers on evt.ProviderId equals provider.Id
            where evt.Id == eventId
            select new CommunityEventSummaryDto(evt.Id, evt.Title, provider.Name, evt.StartUtc, evt.LocationName))
            .FirstOrDefaultAsync();
        return result;
    }
}
