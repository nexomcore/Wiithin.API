namespace WithinAPI.Domain;

// ── Move pillar (physical wellbeing) ─────────────────────────────────────────────
//
// Privacy posture: body metrics, diet logs, workout logs and trainer notes are private
// by default. A trainer may only read a client's data when an Active TrainerClient
// relationship exists AND the matching share-permission flag is set (see MoveAccessRules).
//
// Enums are stored as text via HasConversion<string>() (see WithinDbContext) so the Move
// migration stays a simple set of CreateTable calls — no Postgres native-enum churn.
// The HTTP wire contract uses the lower-token vocabulary the app already ships
// (see MoveDtos / MoveVocabulary); these C# enums are the internal source of truth.

public enum MovePlanStatus
{
    Draft,
    Active,
    Completed,
    Paused,
    Archived
}

public enum MovePlanSourceType
{
    SelfGenerated,
    TrainerAssigned,
    SystemTemplate,
    Challenge
}

public enum WorkoutLogStatus
{
    Planned,
    Completed,
    Skipped,
    PartiallyCompleted
}

public enum DietLogStatus
{
    Followed,
    PartiallyFollowed,
    Missed
}

public enum CalculatorType
{
    BMI,
    BodyFatPercentage,
    BMR,
    TDEE,
    Macro,
    OneRepMax,
    WaterIntake,
    TargetHeartRate
}

public enum TrainerClientStatus
{
    Pending,
    Active,
    Paused,
    Ended,
    Rejected
}

public enum TrainerNoteVisibility
{
    TrainerOnly,
    SharedWithClient
}

public enum ChallengeType
{
    PushUp,
    Steps,
    WeightLoss,
    Strength,
    Running,
    Yoga,
    Mobility,
    Habit,
    Custom
}

public enum ChallengeVisibility
{
    Public,
    FriendsOnly,
    CircleOnly,
    Private
}

public enum ChallengeDisplayMode
{
    PublicName,
    FriendsOnly,
    Anonymous,
    Private
}

public enum ChallengeParticipantStatus
{
    Joined,
    Active,
    Completed,
    Dropped
}

public enum ChallengeProgressStatus
{
    Pending,
    Completed,
    Skipped,
    Failed
}

// One Move profile per user. Option fields (gender, activity level, goal, …) hold the
// app's lower-token vocabulary as validated strings — mirrors the Event option-field
// convention rather than introducing a parallel set of Postgres enums.
public sealed class MoveProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? GenderForCalculation { get; set; }
    public string? ActivityLevel { get; set; }
    public string? FitnessGoal { get; set; }
    public string? ExperienceLevel { get; set; }
    public string? PreferredWorkoutType { get; set; }
    public int? AvailableDaysPerWeek { get; set; }
    public int? SessionDurationMinutes { get; set; }
    public string[] EquipmentAvailable { get; set; } = [];
    public string? InjuriesOrLimitations { get; set; }
    public string? DietPreference { get; set; }
    public string? FoodAllergies { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

// Private by default. Drives the Progress tab body-metric trend.
public sealed class BodyMetricLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal WeightKg { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public decimal? WaistCm { get; set; }
    public decimal? HipCm { get; set; }
    public decimal? ChestCm { get; set; }
    public decimal? ArmCm { get; set; }
    public decimal? ThighCm { get; set; }
    public decimal? Bmi { get; set; }
    public decimal? Bmr { get; set; }
    public decimal? Tdee { get; set; }
    public DateTimeOffset LoggedAt { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}

// Saved calculator runs. Input/output kept as JSON so new calculators need no schema change.
public sealed class CalculatorResult
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public CalculatorType CalculatorType { get; set; }
    public string InputJson { get; set; } = "{}";
    public string ResultJson { get; set; } = "{}";
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class WorkoutPlan
{
    public Guid Id { get; set; }
    public Guid CreatedByUserId { get; set; }
    // Null only for reusable system templates; otherwise the owning client.
    public Guid? AssignedToUserId { get; set; }
    public Guid? TrainerId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Goal { get; set; }
    public string? WorkoutType { get; set; }
    public string? ExperienceLevel { get; set; }
    public int DurationWeeks { get; set; }
    public int DaysPerWeek { get; set; }
    public bool IsTemplate { get; set; }
    public MovePlanSourceType SourceType { get; set; } = MovePlanSourceType.SelfGenerated;
    public MovePlanStatus Status { get; set; } = MovePlanStatus.Draft;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

public sealed class WorkoutDay
{
    public Guid Id { get; set; }
    public Guid WorkoutPlanId { get; set; }
    public int DayNumber { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

public sealed class WorkoutExercise
{
    public Guid Id { get; set; }
    public Guid WorkoutDayId { get; set; }
    public string ExerciseName { get; set; } = "";
    public int? Sets { get; set; }
    public string? Reps { get; set; }
    public decimal? Weight { get; set; }
    public int? RestSeconds { get; set; }
    public string? Tempo { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}

public sealed class WorkoutLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    // Nullable so the app's quick "log today's session" flow works even without an active plan.
    public Guid? WorkoutPlanId { get; set; }
    public Guid? WorkoutDayId { get; set; }
    public string? Title { get; set; }
    public int? DurationMinutes { get; set; }
    public WorkoutLogStatus Status { get; set; } = WorkoutLogStatus.Completed;
    public DateOnly LogDate { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class WorkoutExerciseLog
{
    public Guid Id { get; set; }
    public Guid WorkoutLogId { get; set; }
    public Guid WorkoutExerciseId { get; set; }
    public int? ActualSets { get; set; }
    public string? ActualReps { get; set; }
    public decimal? ActualWeight { get; set; }
    public string? Notes { get; set; }
}

public sealed class DietPlan
{
    public Guid Id { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? TrainerId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Goal { get; set; }
    public int Calories { get; set; }
    public int ProteinGrams { get; set; }
    public int CarbsGrams { get; set; }
    public int FatGrams { get; set; }
    public string? DietPreference { get; set; }
    public int DurationWeeks { get; set; }
    public bool IsTemplate { get; set; }
    public MovePlanSourceType SourceType { get; set; } = MovePlanSourceType.SelfGenerated;
    public MovePlanStatus Status { get; set; } = MovePlanStatus.Draft;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

public sealed class DietMeal
{
    public Guid Id { get; set; }
    public Guid DietPlanId { get; set; }
    public string MealName { get; set; } = "";
    public string? MealTime { get; set; }
    public int Calories { get; set; }
    public int ProteinGrams { get; set; }
    public int CarbsGrams { get; set; }
    public int FatGrams { get; set; }
    public int SortOrder { get; set; }
}

public sealed class DietMealItem
{
    public Guid Id { get; set; }
    public Guid DietMealId { get; set; }
    public string FoodName { get; set; } = "";
    public string Quantity { get; set; } = "";
    public string? Unit { get; set; }
    public int? Calories { get; set; }
    public int? ProteinGrams { get; set; }
    public int? CarbsGrams { get; set; }
    public int? FatGrams { get; set; }
    public string? SubstitutionNotes { get; set; }
}

public sealed class DietLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid DietPlanId { get; set; }
    public Guid DietMealId { get; set; }
    public DietLogStatus Status { get; set; } = DietLogStatus.Followed;
    public DateOnly LogDate { get; set; }
    public DateTimeOffset LoggedAt { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}

// Trainer ↔ client link. TrainerUserId is a normal user holding the Provider role
// (no second identity system). Sharing flags live here per spec's "fields on TrainerClient".
public sealed class TrainerClient
{
    public Guid Id { get; set; }
    public Guid TrainerUserId { get; set; }
    public Guid ClientUserId { get; set; }
    public TrainerClientStatus Status { get; set; } = TrainerClientStatus.Pending;
    public bool ShareMoveProfileWithTrainer { get; set; } = true;
    public bool ShareBodyMetricsWithTrainer { get; set; }
    public bool ShareWorkoutLogsWithTrainer { get; set; } = true;
    public bool ShareDietLogsWithTrainer { get; set; } = true;
    public bool ShareChallengeProgressWithTrainer { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

public sealed class TrainerNote
{
    public Guid Id { get; set; }
    public Guid TrainerUserId { get; set; }
    public Guid ClientUserId { get; set; }
    public string Note { get; set; } = "";
    public TrainerNoteVisibility Visibility { get; set; } = TrainerNoteVisibility.TrainerOnly;
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class Challenge
{
    public Guid Id { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? TrainerId { get; set; }
    public Guid? CircleId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public ChallengeType ChallengeType { get; set; } = ChallengeType.Custom;
    public int DurationDays { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public ChallengeVisibility Visibility { get; set; } = ChallengeVisibility.Public;
    public bool IsPublic { get; set; } = true;
    public bool IsTemplate { get; set; }
    public bool AllowAnonymousParticipation { get; set; } = true;
    public bool LeaderboardEnabled { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

public sealed class ChallengeTask
{
    public Guid Id { get; set; }
    public Guid ChallengeId { get; set; }
    public int DayNumber { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public decimal? TargetValue { get; set; }
    public string? TargetUnit { get; set; }
    public int SortOrder { get; set; }
}

public sealed class ChallengeParticipant
{
    public Guid Id { get; set; }
    public Guid ChallengeId { get; set; }
    public Guid UserId { get; set; }
    public ChallengeDisplayMode DisplayMode { get; set; } = ChallengeDisplayMode.PublicName;
    public ChallengeParticipantStatus Status { get; set; } = ChallengeParticipantStatus.Joined;
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class ChallengeProgress
{
    public Guid Id { get; set; }
    public Guid ChallengeId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ChallengeTaskId { get; set; }
    public ChallengeProgressStatus Status { get; set; } = ChallengeProgressStatus.Completed;
    public decimal? ValueCompleted { get; set; }
    public string? ProofImageUrl { get; set; }
    public string? Notes { get; set; }
    public DateOnly LogDate { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}
