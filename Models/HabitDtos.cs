namespace WithinAPI.Models;

public sealed record HabitTemplateDto(
    string Id,
    string Name,
    string Category,
    string? Description,
    string? IconKey,
    int SortOrder);

public sealed record UserHabitDto(
    string Id,
    string? HabitTemplateId,
    string Name,
    string? Category,
    bool IsCustom,
    bool IsActive,
    bool CompletedToday);

public sealed record AddUserHabitDto(
    string? HabitTemplateId = null,
    string? Name = null,
    string? Category = null);

public sealed record UpdateUserHabitDto(bool IsActive);

public sealed record HabitProgressDto(
    int CompletedToday,
    int TotalActive,
    string DailyLabel,
    int DaysShownUpThisWeek,
    string WeeklyLabel);

public sealed record HabitTodayDto(
    UserHabitDto[] Habits,
    HabitProgressDto Progress);
