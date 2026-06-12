namespace WithinAPI.Domain;

public enum ProgramCategory
{
    Fitness,
    Nutrition,
    Yoga,
    Meditation,
    Mindfulness,
    SpiritualGrowth,
    GeneralWellbeing
}

public enum ProgramTaskType
{
    Exercise,
    Meal,
    YogaPose,
    Meditation,
    Reflection,
    Reading,
    Habit,
    Custom
}

public enum AssignedProgramStatus
{
    Draft,
    Active,
    Paused,
    Completed,
    Cancelled
}

public enum AssignedProgramTaskStatus
{
    Pending,
    Completed,
    Skipped
}

public sealed class ProgramTemplate
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public ProgramCategory Category { get; set; } = ProgramCategory.GeneralWellbeing;
    public int DurationWeeks { get; set; }
    public string DifficultyLevel { get; set; } = "";
    public string Goal { get; set; } = "";
    public bool IsPublicTemplate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ProgramTemplateWeek
{
    public Guid Id { get; set; }
    public Guid ProgramTemplateId { get; set; }
    public int WeekNumber { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
}

public sealed class ProgramTemplateDay
{
    public Guid Id { get; set; }
    public Guid ProgramTemplateWeekId { get; set; }
    public int DayNumber { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
}

public sealed class ProgramTemplateTask
{
    public Guid Id { get; set; }
    public Guid ProgramTemplateDayId { get; set; }
    public ProgramTaskType TaskType { get; set; } = ProgramTaskType.Custom;
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public int? DurationMinutes { get; set; }
    public int? Sets { get; set; }
    public string? Reps { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Distance { get; set; }
    public int? Calories { get; set; }
    public int? Protein { get; set; }
    public int? Carbs { get; set; }
    public int? Fat { get; set; }
    public string? AttachmentUrl { get; set; }
    public int SortOrder { get; set; }
}

public sealed class AssignedProgram
{
    public Guid Id { get; set; }
    public Guid ProgramTemplateId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid ClientUserId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public ProgramCategory Category { get; set; } = ProgramCategory.GeneralWellbeing;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public AssignedProgramStatus Status { get; set; } = AssignedProgramStatus.Draft;
    public string? ProviderNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssignedProgramTask
{
    public Guid Id { get; set; }
    public Guid AssignedProgramId { get; set; }
    public int WeekNumber { get; set; }
    public int DayNumber { get; set; }
    public ProgramTaskType TaskType { get; set; } = ProgramTaskType.Custom;
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public int? DurationMinutes { get; set; }
    public int? Sets { get; set; }
    public string? Reps { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Distance { get; set; }
    public int? Calories { get; set; }
    public int? Protein { get; set; }
    public int? Carbs { get; set; }
    public int? Fat { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public AssignedProgramTaskStatus Status { get; set; } = AssignedProgramTaskStatus.Pending;
    public string? ClientNotes { get; set; }
    public string? ProviderFeedback { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int SortOrder { get; set; }
}

public sealed class ClientCheckIn
{
    public Guid Id { get; set; }
    public Guid AssignedProgramId { get; set; }
    public Guid ClientUserId { get; set; }
    public Guid ProviderId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public decimal? Weight { get; set; }
    public int? EnergyLevel { get; set; }
    public int? StressLevel { get; set; }
    public int? SleepQuality { get; set; }
    public string? Mood { get; set; }
    public int? ComplianceScore { get; set; }
    public string? ClientNotes { get; set; }
    public string? ProviderFeedback { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
