namespace WithinAPI.Models;

// Wire DTOs for the Move pillar. Field names and the lower-token enum strings mirror the
// app's shipping contract (WithinApp/src/move/service.ts) so the existing tabs light up
// without churn. ASP.NET's web JSON defaults serialise these as camelCase.

// ── Profile ──
public sealed record MoveProfileDto(
    string? Id,
    decimal? HeightCm,
    decimal? WeightKg,
    string? DateOfBirth,
    int? Age,
    string? Gender,
    string? ActivityLevel,
    string? FitnessGoal,
    string? ExperienceLevel,
    string? PreferredWorkoutType,
    int? AvailableDaysPerWeek,
    int? SessionDurationMinutes,
    string[] Equipment,
    string? Injuries,
    string? DietPreference,
    string? FoodAllergies,
    string? UpdatedUtc);

public sealed record MoveProfileInputDto(
    decimal? HeightCm,
    decimal? WeightKg,
    string? DateOfBirth,
    string? Gender,
    string? ActivityLevel,
    string? FitnessGoal,
    string? ExperienceLevel,
    string? PreferredWorkoutType,
    int? AvailableDaysPerWeek,
    int? SessionDurationMinutes,
    string[]? Equipment,
    string? Injuries,
    string? DietPreference,
    string? FoodAllergies);

// ── Body metrics ──
public sealed record BodyMetricDto(
    string Id,
    decimal WeightKg,
    decimal? BodyFatPercentage,
    decimal? WaistCm,
    decimal? Bmi,
    string LoggedAt);

public sealed record BodyMetricInputDto(
    decimal WeightKg,
    decimal? BodyFatPercentage,
    decimal? WaistCm,
    decimal? HipCm,
    decimal? ChestCm,
    decimal? ArmCm,
    decimal? ThighCm);

// ── Calculators ──
public sealed record CalculatorSaveDto(string CalculatorType, object Input, object Result);
public sealed record CalculatorHistoryDto(string Id, string CalculatorType, string InputJson, string ResultJson, string CreatedUtc);

// ── Workouts ──
public sealed record WorkoutExerciseDto(string Name, int? Sets, string? Reps, decimal? Weight, int? RestSeconds, string? Notes);
public sealed record WorkoutDayDto(string Id, string Label, string? Focus, WorkoutExerciseDto[] Exercises);
public sealed record WorkoutPlanDto(
    string Id,
    string Title,
    string? Type,
    string Source,
    int DaysPerWeek,
    bool IsActive,
    WorkoutDayDto[] Days,
    string CreatedUtc);

public sealed record WorkoutLogDto(string Id, string Date, string Title, int? DurationMinutes, string? Notes);
public sealed record LogWorkoutInputDto(string? Title, int? DurationMinutes, string? Notes, string? WorkoutDayId, string? Status);
public sealed record GeneratePlanRequestDto(
    string? Goal,
    string? WorkoutType,
    string? ExperienceLevel,
    int? DaysPerWeek,
    int? SessionDurationMinutes,
    int? CalorieTarget,
    string? DietPreference,
    bool Activate = true);

// ── Diet ──
public sealed record DietFoodItemDto(string Name, string? Quantity, string? SubstitutionNote);
public sealed record DietMealDto(string Id, string Name, int? Calories, DietFoodItemDto[] Items, string? Status, string? Note);
public sealed record DietPlanDto(
    string Id,
    string Title,
    string Source,
    bool IsActive,
    int Calories,
    int ProteinG,
    int CarbsG,
    int FatG,
    DietMealDto[] Meals,
    decimal? AdherencePct,
    string CreatedUtc);

public sealed record LogMealInputDto(string MealId, string Status, string? Note);

// ── Templates (trainer assignment / discovery) ──
public sealed record WorkoutTemplateDto(string Id, string Title, string? Goal, string? WorkoutType, string? ExperienceLevel, int DaysPerWeek, int DurationWeeks);
public sealed record DietTemplateDto(string Id, string Title, string? Goal, string? DietPreference, int Calories, int DurationWeeks);

// ── Challenges ──
public sealed record ChallengeDto(
    string Id,
    string Title,
    string Description,
    int DurationDays,
    int ParticipantCount,
    string Source,
    string? CircleName,
    bool IsJoined,
    int? ProgressDays,
    string? Privacy);

public sealed record JoinChallengeInputDto(string? Privacy);
public sealed record ChallengeProgressInputDto(string? Note, string? TaskId, decimal? ValueCompleted, string? Status);
public sealed record ChallengeParticipantDto(string Id, string DisplayName, bool ShowAvatar, int CompletedTasks, string Status);

// ── Trainer ──
public sealed record TrainerPermissionsDto(
    bool ShareProfile,
    bool ShareBodyMetrics,
    bool ShareWorkoutLogs,
    bool ShareDietLogs,
    bool ShareChallengeProgress);

public sealed record TrainerSummaryDto(
    bool HasTrainer,
    string? TrainerName,
    string? TrainerTitle,
    string? TrainerClientId,
    string? PendingTrainerName,
    string? AssignedWorkoutPlanId,
    string? AssignedDietPlanId,
    string? LatestNote,
    bool IsTrainer,
    int PendingIncomingRequests,
    TrainerPermissionsDto Permissions);

public sealed record TrainerClientDto(string Id, string ClientUserId, string DisplayName, string? Goal, string Status, string? LastActiveUtc);
public sealed record TrainerConsoleDto(TrainerClientDto[] Clients, int PendingRequestCount, int AssignedPlanCount);
public sealed record TrainerNoteDto(string Id, string Note, string Visibility, string CreatedUtc);
public sealed record CreateTrainerNoteDto(string Note, string? Visibility);
public sealed record AssignPlanDto(string TemplateId, bool Activate = true);
public sealed record TrainerRequestDto(string? TrainerUserId, string? ProviderId);

// ── Dashboard (Overview) ──
public sealed record DashboardWorkoutDto(string Id, string Title, int DaysPerWeek);
public sealed record DashboardDietDto(string Id, string Title, int Calories);
public sealed record DashboardChallengeDto(string Id, string Title, int ProgressDays, int DurationDays);
public sealed record DashboardTrainerDto(bool HasTrainer, string? TrainerName);
public sealed record MoveDashboardDto(
    bool HasProfile,
    MoveProfileDto? Profile,
    BodyMetricDto? LatestBodyMetric,
    DashboardWorkoutDto? ActiveWorkout,
    DashboardDietDto? ActiveDiet,
    DashboardChallengeDto? ActiveChallenge,
    DashboardTrainerDto? Trainer,
    int WeeklyWorkoutCompletion,
    int WeeklyDietAdherence);
