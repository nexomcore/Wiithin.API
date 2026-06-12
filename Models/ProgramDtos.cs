using WithinAPI.Domain;

namespace WithinAPI.Models;

public sealed record ProgramTemplateTaskDto(
    Guid Id,
    ProgramTaskType TaskType,
    string Title,
    string? Description,
    string? Instructions,
    int? DurationMinutes,
    int? Sets,
    string? Reps,
    decimal? Weight,
    decimal? Distance,
    int? Calories,
    int? Protein,
    int? Carbs,
    int? Fat,
    string? AttachmentUrl,
    int SortOrder);

public sealed record ProgramTemplateDayDto(Guid Id, int DayNumber, string Title, string? Description, ProgramTemplateTaskDto[] Tasks);

public sealed record ProgramTemplateWeekDto(Guid Id, int WeekNumber, string Title, string? Description, ProgramTemplateDayDto[] Days);

public sealed record ProgramTemplateDto(
    Guid Id,
    Guid ProviderId,
    string Title,
    string Description,
    ProgramCategory Category,
    int DurationWeeks,
    string DifficultyLevel,
    string Goal,
    bool IsPublicTemplate,
    ProgramTemplateWeekDto[] Weeks,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertProgramTemplateDto(
    string Title,
    string Description,
    ProgramCategory Category,
    int DurationWeeks,
    string DifficultyLevel,
    string Goal,
    bool IsPublicTemplate,
    ProgramTemplateWeekInputDto[] Weeks);

public sealed record ProgramTemplateWeekInputDto(int WeekNumber, string Title, string? Description, ProgramTemplateDayInputDto[] Days);

public sealed record ProgramTemplateDayInputDto(int DayNumber, string Title, string? Description, ProgramTaskInputDto[] Tasks);

public sealed record ProgramTaskInputDto(
    ProgramTaskType TaskType,
    string Title,
    string? Description,
    string? Instructions,
    int? DurationMinutes,
    int? Sets,
    string? Reps,
    decimal? Weight,
    decimal? Distance,
    int? Calories,
    int? Protein,
    int? Carbs,
    int? Fat,
    string? AttachmentUrl,
    int SortOrder);

public sealed record AssignProgramDto(Guid ProgramTemplateId, Guid ClientUserId, DateOnly StartDate, string? Title, string? ProviderNotes);

public sealed record AssignedProgramTaskDto(
    Guid Id,
    Guid AssignedProgramId,
    int WeekNumber,
    int DayNumber,
    ProgramTaskType TaskType,
    string Title,
    string? Description,
    string? Instructions,
    int? DurationMinutes,
    int? Sets,
    string? Reps,
    decimal? Weight,
    decimal? Distance,
    int? Calories,
    int? Protein,
    int? Carbs,
    int? Fat,
    string? AttachmentUrl,
    DateOnly ScheduledDate,
    AssignedProgramTaskStatus Status,
    string? ClientNotes,
    string? ProviderFeedback,
    DateTimeOffset? CompletedAt,
    int SortOrder);

public sealed record AssignedProgramDto(
    Guid Id,
    Guid ProgramTemplateId,
    Guid ProviderId,
    string ProviderName,
    Guid ClientUserId,
    string ClientName,
    string Title,
    string Description,
    ProgramCategory Category,
    DateOnly StartDate,
    DateOnly EndDate,
    AssignedProgramStatus Status,
    string? ProviderNotes,
    int ProgressPercent,
    int CompletedTasks,
    int SkippedTasks,
    int PendingTasks,
    int CurrentStreakDays,
    AssignedProgramTaskDto[] Tasks,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpdateAssignedProgramDto(string Title, string Description, ProgramCategory Category, DateOnly StartDate, DateOnly EndDate, AssignedProgramStatus Status, string? ProviderNotes);

public sealed record UpsertAssignedProgramTaskDto(
    int WeekNumber,
    int DayNumber,
    ProgramTaskType TaskType,
    string Title,
    string? Description,
    string? Instructions,
    int? DurationMinutes,
    int? Sets,
    string? Reps,
    decimal? Weight,
    decimal? Distance,
    int? Calories,
    int? Protein,
    int? Carbs,
    int? Fat,
    string? AttachmentUrl,
    DateOnly ScheduledDate,
    int SortOrder);

public sealed record UpdateAssignedProgramTaskStatusDto(AssignedProgramTaskStatus Status, string? ClientNotes);

public sealed record ProviderTaskFeedbackDto(string? ProviderFeedback);

public sealed record ClientCheckInDto(
    Guid Id,
    Guid AssignedProgramId,
    Guid ClientUserId,
    Guid ProviderId,
    DateOnly CheckInDate,
    decimal? Weight,
    int? EnergyLevel,
    int? StressLevel,
    int? SleepQuality,
    string? Mood,
    int? ComplianceScore,
    string? ClientNotes,
    string? ProviderFeedback,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertClientCheckInDto(
    DateOnly CheckInDate,
    decimal? Weight,
    int? EnergyLevel,
    int? StressLevel,
    int? SleepQuality,
    string? Mood,
    int? ComplianceScore,
    string? ClientNotes);

public sealed record ProviderCheckInFeedbackDto(string? ProviderFeedback);

public sealed record ProviderProgramClientDto(Guid UserId, string DisplayName, int ActiveProgramCount, DateTimeOffset? LastActiveUtc);
