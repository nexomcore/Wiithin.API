using Microsoft.EntityFrameworkCore;
using WithinAPI.Data;
using WithinAPI.Domain;

namespace WithinAPI.Services;

public sealed record DisplayIdentity(string DisplayName, bool ProfileLinkAllowed, CircleIdentityMode IdentityMode);

public sealed class PrivacyService(WithinDbContext db)
{
    public async Task<UserPrivacySettings> GetOrCreateSettings(Guid userId)
    {
        var settings = await db.UserPrivacySettings.FindAsync(userId);
        if (settings is not null) return settings;

        var now = DateTimeOffset.UtcNow;
        settings = new UserPrivacySettings
        {
            UserId = userId,
            ProfileVisibility = ProfileVisibility.FriendsOnly,
            DefaultRsvpVisibility = RsvpVisibility.FriendsOnly,
            TaggingPermission = TaggingPermission.FriendsOnly,
            FriendRequestPermission = FriendRequestPermission.SameCircleOrEvent,
            ShowActivityToFriends = true,
            AllowEventInviteFromFriends = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.UserPrivacySettings.Add(settings);
        await db.SaveChangesAsync();
        return settings;
    }

    public async Task<bool> AreConnected(Guid firstUserId, Guid secondUserId) =>
        await db.Connections.AnyAsync(item =>
            item.Status == ConnectionStatus.Accepted &&
            ((item.RequesterUserId == firstUserId && item.ReceiverUserId == secondUserId) ||
             (item.RequesterUserId == secondUserId && item.ReceiverUserId == firstUserId)));

    public async Task<bool> IsBlocked(Guid firstUserId, Guid secondUserId) =>
        await db.Connections.AnyAsync(item =>
            item.Status == ConnectionStatus.Blocked &&
            ((item.RequesterUserId == firstUserId && item.ReceiverUserId == secondUserId) ||
             (item.RequesterUserId == secondUserId && item.ReceiverUserId == firstUserId)));

    public async Task<bool> ShareCircle(Guid firstUserId, Guid secondUserId) =>
        await (
            from first in db.CircleMembers
            join second in db.CircleMembers on first.CircleId equals second.CircleId
            where first.UserId == firstUserId &&
                  second.UserId == secondUserId &&
                  first.Status == CircleMemberStatus.Active &&
                  second.Status == CircleMemberStatus.Active
            select first.CircleId).AnyAsync();

    public async Task<bool> ShareVisibleEvent(Guid firstUserId, Guid secondUserId) =>
        await (
            from first in db.EventRegistrations
            join second in db.EventRegistrations on first.EventId equals second.EventId
            where first.UserId == firstUserId &&
                  second.UserId == secondUserId &&
                  first.State != EventJoinState.Declined &&
                  second.State != EventJoinState.Declined
            select first.EventId).AnyAsync();

    public async Task<bool> CanSendConnectionRequest(Guid requesterUserId, Guid receiverUserId)
    {
        if (requesterUserId == receiverUserId) return false;
        if (await IsBlocked(requesterUserId, receiverUserId)) return false;
        var receiverSettings = await GetOrCreateSettings(receiverUserId);
        return receiverSettings.FriendRequestPermission switch
        {
            FriendRequestPermission.Everyone => true,
            FriendRequestPermission.FriendsOfFriends => false,
            FriendRequestPermission.SameCircleOrEvent => await ShareCircle(requesterUserId, receiverUserId) || await ShareVisibleEvent(requesterUserId, receiverUserId),
            FriendRequestPermission.NoOne => false,
            _ => false
        };
    }

    public async Task<bool> CanViewEventRsvp(Guid viewerUserId, Event evt, EventRegistration rsvp)
    {
        if (viewerUserId == rsvp.UserId) return true;
        var provider = await db.Providers.FindAsync(evt.ProviderId);
        var viewer = await db.Users.FindAsync(viewerUserId);
        if (viewer?.Role == WithinRole.Admin || provider?.OwnerUserId == viewerUserId) return true;

        return rsvp.Visibility switch
        {
            RsvpVisibility.Public => true,
            RsvpVisibility.FriendsOnly => await AreConnected(viewerUserId, rsvp.UserId),
            RsvpVisibility.CircleMembersOnly => await ShareEventCircle(viewerUserId, rsvp.UserId, evt.Id),
            RsvpVisibility.Private => false,
            _ => false
        };
    }

    public async Task<bool> ShareEventCircle(Guid viewerUserId, Guid attendeeUserId, Guid eventId) =>
        await (
            from share in db.CircleEvents
            join viewer in db.CircleMembers on share.CircleId equals viewer.CircleId
            join attendee in db.CircleMembers on share.CircleId equals attendee.CircleId
            where share.EventId == eventId &&
                  share.Status == CircleEventStatus.Active &&
                  viewer.UserId == viewerUserId &&
                  attendee.UserId == attendeeUserId &&
                  viewer.Status == CircleMemberStatus.Active &&
                  attendee.Status == CircleMemberStatus.Active
            select share.Id).AnyAsync();

    public async Task<bool> CanMentionUser(Guid actorUserId, Guid targetUserId, Guid? circleId = null)
    {
        if (actorUserId == targetUserId) return true;
        if (await IsBlocked(actorUserId, targetUserId)) return false;
        var settings = await GetOrCreateSettings(targetUserId);
        return settings.TaggingPermission switch
        {
            TaggingPermission.Everyone => true,
            TaggingPermission.FriendsOnly => await AreConnected(actorUserId, targetUserId),
            TaggingPermission.CircleMembersOnly => circleId is null
                ? await ShareCircle(actorUserId, targetUserId)
                : await IsCircleMember(circleId.Value, actorUserId) && await IsCircleMember(circleId.Value, targetUserId),
            TaggingPermission.NoOne => false,
            _ => false
        };
    }

    public async Task<bool> IsCircleMember(Guid circleId, Guid userId) =>
        await db.CircleMembers.AnyAsync(item => item.CircleId == circleId && item.UserId == userId && item.Status == CircleMemberStatus.Active);

    public async Task<bool> CanViewCircleMember(Guid viewerUserId, Guid circleId, Guid targetUserId)
    {
        if (viewerUserId == targetUserId) return true;
        var circle = await db.Circles.FindAsync(circleId);
        if (circle is null) return false;
        var isMember = await IsCircleMember(circleId, viewerUserId);
        var isModerator = await db.CircleRoles.AnyAsync(item => item.CircleId == circleId && item.UserId == viewerUserId);
        var viewer = await db.Users.FindAsync(viewerUserId);
        if (viewer?.Role == WithinRole.Admin || isModerator) return true;

        return circle.MemberListVisibility switch
        {
            MemberListVisibility.Public => true,
            MemberListVisibility.MembersOnly => isMember,
            MemberListVisibility.AdminsOnly => false,
            MemberListVisibility.Hidden => false,
            _ => false
        };
    }

    public async Task<DisplayIdentity> GetDisplayIdentityForCircle(Guid? viewerUserId, Guid circleId, Guid targetUserId)
    {
        var user = await db.Users.FindAsync(targetUserId);
        var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == targetUserId);
        if (user is null) return new DisplayIdentity("Circle Member", false, CircleIdentityMode.HiddenProfile);
        if (member is null || member.IdentityMode == CircleIdentityMode.RealProfile)
        {
            return new DisplayIdentity(user.DisplayName, true, CircleIdentityMode.RealProfile);
        }

        if (viewerUserId == targetUserId)
        {
            return new DisplayIdentity(member.DisplayNameOverride ?? user.DisplayName, true, member.IdentityMode);
        }

        return member.IdentityMode switch
        {
            CircleIdentityMode.Pseudonym => new DisplayIdentity(member.DisplayNameOverride ?? "Circle Member", false, CircleIdentityMode.Pseudonym),
            CircleIdentityMode.HiddenProfile => new DisplayIdentity("Circle Member", false, CircleIdentityMode.HiddenProfile),
            _ => new DisplayIdentity(user.DisplayName, true, CircleIdentityMode.RealProfile)
        };
    }
}
