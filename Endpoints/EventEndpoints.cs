using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class EventEndpoints
{
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var events = app.MapGroup("/api/events");

        events.MapGet("", async (
            WithinDbContext db,
            ClaimsPrincipal principal,
            WithinLens? lens,
            bool? free,
            bool? online,
            bool? weekend,
            string? search,
            string? tag,
            Guid? providerId,
            string? type) =>
        {
            var userId = principal.TryUserId();
            var query = db.Events.Where(item => item.Status == EventStatus.Published);
            if (lens is not null) query = query.Where(item => item.Lens == lens);
            if (free is true) query = query.Where(item => item.PriceAmount == 0);
            if (online is not null) query = query.Where(item => item.IsOnline == online);
            if (weekend is true) query = query.Where(item => item.StartUtc.DayOfWeek == DayOfWeek.Saturday || item.StartUtc.DayOfWeek == DayOfWeek.Sunday);
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(item => item.Title.ToLower().Contains(search.Trim().ToLower()));
            if (!string.IsNullOrWhiteSpace(tag)) query = query.Where(item => item.Tags.Contains(tag.Trim().ToLower()));
            if (providerId is not null) query = query.Where(item => item.ProviderId == providerId);
            if (!string.IsNullOrWhiteSpace(type))
            {
                var normalizedType = type.Trim().ToLower();
                query = query.Where(item => item.EventType == normalizedType);
            }
            return Results.Ok(await ApiMapping.ProjectEvents(query.OrderBy(item => item.StartUtc), db, userId).ToArrayAsync());
        });

        events.MapGet("/{id:guid}", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.TryUserId();
            var item = await ApiMapping.ProjectEvents(db.Events.Where(evt => evt.Id == id), db, userId).FirstOrDefaultAsync();
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        events.MapPost("", async (UpsertEventDto request, WithinDbContext db, NotificationService notifications, ClaimsPrincipal principal) =>
        {
            if (!request.TryValidate(out var validationMessage)) return Results.BadRequest(new { message = validationMessage });

            var provider = await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == principal.UserId());
            if (provider is null) return Results.Forbid();
            if (!await ProviderServiceBelongsToProvider(db, request.ProviderServiceId, provider.Id))
            {
                return Results.BadRequest(new { message = "Provider service was not found for this provider." });
            }

            var evt = request.ToEntity(provider.Id);
            db.Events.Add(evt);
            await db.SaveChangesAsync();
            await notifications.NotifyProviderFollowersNewEvent(provider.Id, evt.Id, evt.Title, evt.EventType);
            return Results.Created($"/api/events/{evt.Id}", await ApiMapping.ProjectEvents(db.Events.Where(item => item.Id == evt.Id), db, principal.UserId()).FirstAsync());
        }).RequireAuthorization();

        events.MapPut("/{id:guid}", async (Guid id, UpsertEventDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!request.TryValidate(out var validationMessage)) return Results.BadRequest(new { message = validationMessage });

            var evt = await db.Events.FindAsync(id);
            var provider = evt is null ? null : await db.Providers.FindAsync(evt.ProviderId);
            if (evt is null || provider is null) return Results.NotFound();
            if (provider.OwnerUserId != principal.UserId()) return Results.Forbid();
            if (!await ProviderServiceBelongsToProvider(db, request.ProviderServiceId, provider.Id))
            {
                return Results.BadRequest(new { message = "Provider service was not found for this provider." });
            }

            request.ApplyTo(evt);
            await db.SaveChangesAsync();
            return Results.Ok(await ApiMapping.ProjectEvents(db.Events.Where(item => item.Id == evt.Id), db, principal.UserId()).FirstAsync());
        }).RequireAuthorization();

        events.MapPost("/{id:guid}/join", async (Guid id, JoinEventDto request, WithinDbContext db, PrivacyService privacy, NotificationService notifications, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var evt = await db.Events.FindAsync(id);
            if (evt is null) return Results.NotFound();

            var registration = await db.EventRegistrations.FirstOrDefaultAsync(item => item.EventId == id && item.UserId == userId);
            if (registration is null)
            {
                registration = new EventRegistration { Id = Guid.NewGuid(), EventId = id, UserId = userId, CreatedUtc = DateTimeOffset.UtcNow };
                db.EventRegistrations.Add(registration);
            }

            registration.State = request.State;
            if (registration.Visibility == default)
            {
                registration.Visibility = await DefaultRsvpVisibility(db, privacy, userId, id);
            }
            registration.UpdatedUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            await notifications.ScheduleEventReminders(userId, evt, request.State);
            await notifications.NotifyPublicFriendRsvp(userId, id);
            return Results.Ok(await ApiMapping.ProjectEvents(db.Events.Where(item => item.Id == id), db, userId).FirstAsync());
        }).RequireAuthorization();

        events.MapPut("/{id:guid}/rsvp", async (Guid id, EventRsvpDto request, WithinDbContext db, PrivacyService privacy, NotificationService notifications, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var evt = await db.Events.FindAsync(id);
            if (evt is null) return Results.NotFound();

            var registration = await db.EventRegistrations.FirstOrDefaultAsync(item => item.EventId == id && item.UserId == userId);
            if (registration is null)
            {
                registration = new EventRegistration { Id = Guid.NewGuid(), EventId = id, UserId = userId, CreatedUtc = DateTimeOffset.UtcNow };
                db.EventRegistrations.Add(registration);
            }

            registration.State = request.State;
            registration.Visibility = request.Visibility ?? await DefaultRsvpVisibility(db, privacy, userId, id);
            registration.UpdatedUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            await notifications.ScheduleEventReminders(userId, evt, request.State);
            await notifications.NotifyPublicFriendRsvp(userId, id);
            return Results.Ok(await ApiMapping.ProjectEvents(db.Events.Where(item => item.Id == id), db, userId).FirstAsync());
        }).RequireAuthorization();

        events.MapPut("/{id:guid}/rsvp/visibility", async (Guid id, RsvpVisibilityDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var registration = await db.EventRegistrations.FirstOrDefaultAsync(item => item.EventId == id && item.UserId == userId);
            if (registration is null) return Results.NotFound();
            registration.Visibility = request.Visibility;
            registration.UpdatedUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        events.MapPost("/{id:guid}/save", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!await db.Events.AnyAsync(item => item.Id == id)) return Results.NotFound();
            if (!await db.SavedEvents.AnyAsync(item => item.EventId == id && item.UserId == userId))
            {
                db.SavedEvents.Add(new SavedEvent { Id = Guid.NewGuid(), EventId = id, UserId = userId, CreatedUtc = DateTimeOffset.UtcNow });
                await db.SaveChangesAsync();
            }
            return Results.NoContent();
        }).RequireAuthorization();

        events.MapDelete("/{id:guid}/save", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            await db.SavedEvents.Where(item => item.EventId == id && item.UserId == userId).ExecuteDeleteAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        events.MapGet("/{id:guid}/attendees", async (Guid id, WithinDbContext db, PrivacyService privacy, ClaimsPrincipal principal) =>
        {
            var viewerUserId = principal.UserId();
            var evt = await db.Events.FindAsync(id);
            if (evt is null) return Results.NotFound();
            var registrations = await db.EventRegistrations
                .Where(item => item.EventId == id && item.State != EventJoinState.Declined)
                .OrderByDescending(item => item.UpdatedUtc)
                .ToArrayAsync();

            var attendees = new List<EventAttendeeDto>();
            foreach (var registration in registrations)
            {
                if (!await privacy.CanViewEventRsvp(viewerUserId, evt, registration)) continue;
                attendees.Add(await ToAttendeeDto(db, registration));
            }
            return Results.Ok(attendees.ToArray());
        }).RequireAuthorization();

        events.MapGet("/{id:guid}/friends-going", async (Guid id, WithinDbContext db, PrivacyService privacy, ClaimsPrincipal principal) =>
        {
            var viewerUserId = principal.UserId();
            var evt = await db.Events.FindAsync(id);
            if (evt is null) return Results.NotFound();
            var registrations = await db.EventRegistrations
                .Where(item => item.EventId == id && item.State == EventJoinState.Going && item.UserId != viewerUserId)
                .OrderByDescending(item => item.UpdatedUtc)
                .ToArrayAsync();

            var friends = new List<EventAttendeeDto>();
            foreach (var registration in registrations)
            {
                if (!await privacy.AreConnected(viewerUserId, registration.UserId)) continue;
                if (!await privacy.CanViewEventRsvp(viewerUserId, evt, registration)) continue;
                friends.Add(await ToAttendeeDto(db, registration));
            }
            return Results.Ok(new FriendsGoingDto(friends.Count, friends.ToArray()));
        }).RequireAuthorization();

        events.MapPost("/{id:guid}/invites", async (Guid id, CreateEventInvitesDto request, WithinDbContext db, PrivacyService privacy, NotificationService notifications, ClaimsPrincipal principal) =>
        {
            var inviterUserId = principal.UserId();
            if (!await db.Events.AnyAsync(item => item.Id == id)) return Results.NotFound();
            var now = DateTimeOffset.UtcNow;
            var created = new List<EventInvite>();
            foreach (var invitedUserId in request.InvitedUserIds.Distinct())
            {
                if (invitedUserId == inviterUserId) continue;
                if (!await privacy.AreConnected(inviterUserId, invitedUserId)) continue;
                if (await privacy.IsBlocked(inviterUserId, invitedUserId)) continue;
                var invitedSettings = await privacy.GetOrCreateSettings(invitedUserId);
                if (!invitedSettings.AllowEventInviteFromFriends) continue;
                if (await db.EventInvites.AnyAsync(item => item.EventId == id && item.InvitedUserId == invitedUserId && item.Status == EventInviteStatus.Pending)) continue;

                var invite = new EventInvite
                {
                    Id = Guid.NewGuid(),
                    EventId = id,
                    InvitedByUserId = inviterUserId,
                    InvitedUserId = invitedUserId,
                    Status = EventInviteStatus.Pending,
                    Message = request.Message?.Trim(),
                    CreatedAt = now,
                    UpdatedAt = now
                };
                db.EventInvites.Add(invite);
                created.Add(invite);
            }

            await db.SaveChangesAsync();
            foreach (var invite in created)
            {
                await notifications.NotifyEventInvite(invite.InvitedUserId, invite.InvitedByUserId, invite.EventId, invite.Id);
            }
            return Results.Ok(await ToInviteDtos(db, created.ToArray()));
        }).RequireAuthorization();

        events.MapGet("/invites", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var invites = await db.EventInvites
                .Where(item => item.InvitedUserId == userId || item.InvitedByUserId == userId)
                .OrderByDescending(item => item.CreatedAt)
                .Take(100)
                .ToArrayAsync();
            return Results.Ok(await ToInviteDtos(db, invites));
        }).RequireAuthorization();

        events.MapPost("/invites/{inviteId:guid}/accept", async (Guid inviteId, WithinDbContext db, ClaimsPrincipal principal) =>
            await UpdateInvite(db, principal.UserId(), inviteId, EventInviteStatus.Accepted)).RequireAuthorization();

        events.MapPost("/invites/{inviteId:guid}/decline", async (Guid inviteId, WithinDbContext db, ClaimsPrincipal principal) =>
            await UpdateInvite(db, principal.UserId(), inviteId, EventInviteStatus.Declined)).RequireAuthorization();

        events.MapGet("/{id:guid}/comments", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
            Results.Ok(await ApiMapping.ProjectComments(db.Comments.Where(item => item.EventId == id && !item.IsHidden), db, principal.TryUserId()).ToArrayAsync()));

        events.MapPost("/{id:guid}/comments", async (Guid id, UpsertCommentDto request, WithinDbContext db, NotificationService notifications, ClaimsPrincipal principal) =>
        {
            if (!await db.Events.AnyAsync(item => item.Id == id)) return Results.NotFound();

            var body = request.Body.Trim();
            if (string.IsNullOrWhiteSpace(body)) return Results.BadRequest(new { message = "Comment body is required." });

            if (request.ParentCommentId is not null)
            {
                var parent = await db.Comments.FirstOrDefaultAsync(item => item.Id == request.ParentCommentId && item.EventId == id && !item.IsHidden);
                if (parent is null) return Results.BadRequest(new { message = "Parent comment was not found for this event." });
                if (parent.ParentCommentId is not null) return Results.BadRequest(new { message = "Replies can only be added to top-level comments." });
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                EventId = id,
                ParentCommentId = request.ParentCommentId,
                AuthorUserId = principal.UserId(),
                Body = body,
                CreatedUtc = DateTimeOffset.UtcNow
            };
            db.Comments.Add(comment);
            await db.SaveChangesAsync();
            if (request.ParentCommentId is not null)
            {
                await notifications.NotifyCommentReply(request.ParentCommentId.Value, comment.Id, comment.AuthorUserId, id);
            }
            await notifications.NotifyMentions(comment.AuthorUserId, body, MentionSourceType.EventComment, comment.Id, null, id);
            var dto = await ApiMapping.ProjectComments(db.Comments.Where(item => item.Id == comment.Id), db, comment.AuthorUserId).FirstAsync();
            return Results.Created($"/api/events/{id}/comments/{comment.Id}", dto);
        }).RequireAuthorization();

        events.MapPost("/{id:guid}/comments/{commentId:guid}/like", async (Guid id, Guid commentId, WithinDbContext db, ClaimsPrincipal principal) =>
            await SetCommentLike(db, principal.UserId(), id, commentId)).RequireAuthorization();

        events.MapDelete("/{id:guid}/comments/{commentId:guid}/like", async (Guid id, Guid commentId, WithinDbContext db, ClaimsPrincipal principal) =>
            await RemoveCommentLike(db, principal.UserId(), id, commentId)).RequireAuthorization();

        events.MapPost("/{id:guid}/reviews", async (Guid id, UpsertReviewDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (request.Rating is < 1 or > 5) return Results.BadRequest(new { message = "Rating must be 1-5." });
            var userId = principal.UserId();
            var review = await db.Reviews.FirstOrDefaultAsync(item => item.EventId == id && item.UserId == userId);
            if (review is null)
            {
                review = new Review { Id = Guid.NewGuid(), EventId = id, UserId = userId, CreatedUtc = DateTimeOffset.UtcNow };
                db.Reviews.Add(review);
            }
            review.Rating = request.Rating;
            review.Body = request.Body.Trim();
            await db.SaveChangesAsync();
            return Results.Ok(review);
        }).RequireAuthorization();

        return app;
    }

    private static async Task<RsvpVisibility> DefaultRsvpVisibility(WithinDbContext db, PrivacyService privacy, Guid userId, Guid eventId)
    {
        if (await db.CircleEvents.AnyAsync(item => item.EventId == eventId && item.Status == CircleEventStatus.Active && db.Circles.Any(circle => circle.Id == item.CircleId && circle.PrivacyType == CirclePrivacyType.Sensitive)))
        {
            return RsvpVisibility.Private;
        }

        return (await privacy.GetOrCreateSettings(userId)).DefaultRsvpVisibility;
    }

    private static async Task<IResult> SetCommentLike(WithinDbContext db, Guid userId, Guid eventId, Guid commentId)
    {
        if (!await db.Comments.AnyAsync(item => item.Id == commentId && item.EventId == eventId && !item.IsHidden))
        {
            return Results.NotFound();
        }

        var exists = await db.Reactions.AnyAsync(item => item.UserId == userId && item.CommentId == commentId && item.Kind == "like");
        if (!exists)
        {
            db.Reactions.Add(new Reaction { Id = Guid.NewGuid(), UserId = userId, CommentId = commentId, Kind = "like", CreatedUtc = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();
        }

        return Results.NoContent();
    }

    private static async Task<IResult> RemoveCommentLike(WithinDbContext db, Guid userId, Guid eventId, Guid commentId)
    {
        if (!await db.Comments.AnyAsync(item => item.Id == commentId && item.EventId == eventId && !item.IsHidden))
        {
            return Results.NotFound();
        }

        await db.Reactions
            .Where(item => item.UserId == userId && item.CommentId == commentId && item.Kind == "like")
            .ExecuteDeleteAsync();
        return Results.NoContent();
    }

    private static async Task<bool> ProviderServiceBelongsToProvider(WithinDbContext db, Guid? serviceId, Guid providerId) =>
        serviceId is null || await db.ProviderServices.AnyAsync(item => item.Id == serviceId && item.ProviderId == providerId && item.IsActive);

    private static async Task<EventAttendeeDto> ToAttendeeDto(WithinDbContext db, EventRegistration registration)
    {
        var user = await db.Users.FindAsync(registration.UserId);
        return new EventAttendeeDto(
            registration.UserId,
            user?.DisplayName ?? "Within user",
            registration.State,
            registration.Visibility,
            registration.Visibility == RsvpVisibility.Private,
            registration.UpdatedUtc);
    }

    private static async Task<IResult> UpdateInvite(WithinDbContext db, Guid userId, Guid inviteId, EventInviteStatus status)
    {
        var invite = await db.EventInvites.FindAsync(inviteId);
        if (invite is null) return Results.NotFound();
        if (invite.InvitedUserId != userId || invite.Status != EventInviteStatus.Pending) return Results.Forbid();
        invite.Status = status;
        invite.RespondedAt = DateTimeOffset.UtcNow;
        invite.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<EventInviteDto[]> ToInviteDtos(WithinDbContext db, EventInvite[] invites)
    {
        var response = new List<EventInviteDto>(invites.Length);
        foreach (var invite in invites)
        {
            var evt = await db.Events.FindAsync(invite.EventId);
            response.Add(new EventInviteDto(
                invite.Id,
                invite.EventId,
                evt?.Title ?? "Event",
                await ToAuthorDto(db, invite.InvitedByUserId),
                await ToAuthorDto(db, invite.InvitedUserId),
                invite.Status,
                invite.Message,
                invite.CreatedAt,
                invite.UpdatedAt));
        }
        return response.ToArray();
    }

    private static async Task<CommunityAuthorDto> ToAuthorDto(WithinDbContext db, Guid userId)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null) return new CommunityAuthorDto(userId, "Within user", WithinRole.User, false);
        var verified = user.RoleEnum == WithinRole.Provider && await db.Providers.AnyAsync(item => item.OwnerUserId == userId && item.IsVerified);
        return new CommunityAuthorDto(user.Id, user.DisplayName, user.RoleEnum, verified);
    }
}
