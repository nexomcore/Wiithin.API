using Microsoft.EntityFrameworkCore;
using WithinAPI.Data;
using WithinAPI.Domain;

namespace WithinAPI.Services;

public enum AccountDeletionStatus
{
    Deleted,
    NotFound,
    Blocked
}

public sealed record AccountDeletionResult(AccountDeletionStatus Status, string? Message = null);

/// <summary>
/// Deletes a user account in line with APP 11.2: destroys PII and sensitive personal data,
/// and de-identifies retained audit/history by keeping a scrubbed "tombstone" user row.
/// </summary>
public sealed class AccountDeletionService(WithinDbContext db)
{
    public async Task<AccountDeletionResult> DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null || user.IsDeleted)
        {
            return new AccountDeletionResult(AccountDeletionStatus.NotFound);
        }

        // ---- Guards: block while the user still owns business/community structures ----
        if (await db.Providers.AnyAsync(item => item.OwnerUserId == userId && item.IsActive, cancellationToken))
        {
            return new AccountDeletionResult(AccountDeletionStatus.Blocked,
                "Transfer or deactivate your provider profile before deleting your account.");
        }

        var soleAdminCircle = await db.CircleMembers
            .Where(member => member.UserId == userId
                && member.Status == CircleMemberStatus.Active
                && member.Role == CircleMemberRole.Admin)
            .Select(member => member.CircleId)
            .Where(circleId => db.CircleMembers.Count(other =>
                other.CircleId == circleId
                && other.Status == CircleMemberStatus.Active
                && other.Role == CircleMemberRole.Admin) == 1)
            .AnyAsync(cancellationToken);
        if (soleAdminCircle)
        {
            return new AccountDeletionResult(AccountDeletionStatus.Blocked,
                "Assign another admin to your circle(s) before deleting your account.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // ---- Hard-delete: PII, sensitive personal data, and personal operational data ----
        // Auth / session
        await db.RefreshTokens.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.PushTokens.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.DeviceTokens.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        // Health / wellbeing (sensitive)
        await db.UserWellbeingProfiles.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserWellbeingInterests.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserWellbeingGoals.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.DailyCheckIns.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.MonthlyProfiles.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserHabits.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.HabitCompletions.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        // Preferences / notifications
        await db.NotificationPreferences.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.NotificationMutes.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.NotificationSchedules.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.Notifications.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserPrivacySettings.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        // Lightweight personal operational data (no audit value)
        await db.SavedEvents.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.Reactions.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleReactions.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleHelpfulReactions.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CirclePollVotes.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleWeeklyCheckInResponses.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CommunityMembers.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        // Relationships and inbound invites (no longer meaningful once the account is gone)
        await db.Connections
            .Where(item => item.RequesterUserId == userId || item.ReceiverUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        await db.EventInvites.Where(item => item.InvitedUserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleInvites.Where(item => item.InvitedUserId == userId).ExecuteDeleteAsync(cancellationToken);

        // ---- Retain, de-identified: leave the user's circle membership as history, inactive ----
        await db.CircleMembers
            .Where(item => item.UserId == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(member => member.Status, CircleMemberStatus.Left), cancellationToken);

        // Retained content/audit (circle threads & comments, reviews, event registrations,
        // reports, role/join history, mentions, shared events, announcements) keep their
        // UserId references; they render via the scrubbed tombstone user below.

        // ---- Scrub the user row (tombstone) ----
        user.Email = $"deleted-{user.Id}@deleted.invalid";
        user.DisplayName = "Deleted user";
        user.PasswordHash = "";
        user.IsDeleted = true;
        user.DeletedUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return new AccountDeletionResult(AccountDeletionStatus.Deleted);
    }
}
