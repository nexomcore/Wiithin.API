using WithinAPI.Domain;

namespace WithinAPI.Services;

/// <summary>
/// Pure privacy/permission decisions for the Move pillar. Centralises the rules the
/// endpoints enforce: a trainer reads a client's private data only with an Active
/// relationship AND the matching share flag, and challenge participants are masked per
/// their chosen display mode. No EF/DI so the safeguards are directly unit-tested.
/// </summary>
public static class MoveAccessRules
{
    public enum MoveShareScope
    {
        Profile,
        BodyMetrics,
        WorkoutLogs,
        DietLogs,
        ChallengeProgress
    }

    /// <summary>Does an Active relationship grant the trainer the given share scope right now?</summary>
    public static bool TrainerCanView(TrainerClient? link, MoveShareScope scope)
    {
        if (link is null || link.Status != TrainerClientStatus.Active) return false;
        return scope switch
        {
            MoveShareScope.Profile => link.ShareMoveProfileWithTrainer,
            MoveShareScope.BodyMetrics => link.ShareBodyMetricsWithTrainer,
            MoveShareScope.WorkoutLogs => link.ShareWorkoutLogsWithTrainer,
            MoveShareScope.DietLogs => link.ShareDietLogsWithTrainer,
            MoveShareScope.ChallengeProgress => link.ShareChallengeProgressWithTrainer,
            _ => false
        };
    }

    /// <summary>Plans may be assigned only over an Active trainer-client relationship.</summary>
    public static bool RelationshipAllowsAssignment(TrainerClient? link) =>
        link is not null && link.Status == TrainerClientStatus.Active;

    public sealed record ParticipantDisplay(string DisplayName, bool ShowAvatar, bool PubliclyVisible);

    /// <summary>
    /// How a participant appears in public lists/leaderboards. Anonymous hides the real
    /// name and avatar; Private is excluded from public surfaces entirely (the user still
    /// sees their own row — pass isSelf: true).
    /// </summary>
    public static ParticipantDisplay ResolveParticipantDisplay(string realName, ChallengeDisplayMode mode, bool isSelf) => mode switch
    {
        ChallengeDisplayMode.Anonymous => new ParticipantDisplay("Anonymous", false, true),
        // No friend graph in Move yet: FriendsOnly degrades to private for non-self viewers.
        ChallengeDisplayMode.FriendsOnly => new ParticipantDisplay(isSelf ? realName : "Member", false, isSelf),
        ChallengeDisplayMode.Private => new ParticipantDisplay(isSelf ? realName : "Member", isSelf, isSelf),
        _ => new ParticipantDisplay(realName, true, true)
    };

    /// <summary>Whether a participant row should be exposed on a public leaderboard to a non-owner viewer.</summary>
    public static bool IsPubliclyVisible(ChallengeDisplayMode mode) =>
        mode is ChallengeDisplayMode.PublicName or ChallengeDisplayMode.Anonymous;
}
