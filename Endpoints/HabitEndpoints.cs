using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class HabitEndpoints
{
    public static IEndpointRouteBuilder MapHabitEndpoints(this IEndpointRouteBuilder app)
    {
        var habits = app.MapGroup("/api/habits").RequireAuthorization();

        // Starter wellbeing habit templates (public catalogue, not user-specific).
        habits.MapGet("/templates", async (WithinDbContext db) =>
        {
            var templates = await db.HabitTemplates
                .Where(item => item.IsActive)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name)
                .ToArrayAsync();
            return Results.Ok(templates.Select(item => item.ToDto()).ToArray());
        });

        // The current user's habits (active and inactive) with today's completion flag.
        habits.MapGet("/mine", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var mine = await db.UserHabits
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.IsActive)
                .ThenBy(item => item.CreatedAtUtc)
                .ToArrayAsync();
            var completedHabitIds = await CompletedHabitIds(db, userId, today);
            return Results.Ok(mine.Select(item => item.ToDto(completedHabitIds.Contains(item.Id))).ToArray());
        });

        // Today's active habits plus gentle progress for the Home dashboard.
        habits.MapGet("/today", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var active = await db.UserHabits
                .Where(item => item.UserId == userId && item.IsActive)
                .OrderBy(item => item.CreatedAtUtc)
                .ToArrayAsync();
            var completedHabitIds = await CompletedHabitIds(db, userId, today);
            var habitDtos = active.Select(item => item.ToDto(completedHabitIds.Contains(item.Id))).ToArray();

            var weekStart = today.AddDays(-6);
            var daysShownUp = await db.HabitCompletions
                .Where(item => item.UserId == userId && item.CompletionDate >= weekStart && item.CompletionDate <= today)
                .Select(item => item.CompletionDate)
                .Distinct()
                .CountAsync();

            var completedToday = habitDtos.Count(item => item.CompletedToday);
            var progress = new HabitProgressDto(
                completedToday,
                habitDtos.Length,
                HabitProgressRules.DailyProgressLabel(completedToday, habitDtos.Length),
                daysShownUp,
                HabitProgressRules.WeeklyShowUpLabel(daysShownUp));

            return Results.Ok(new HabitTodayDto(habitDtos, progress));
        });

        // Add a starter template or create a simple custom habit.
        habits.MapPost("/mine", async (AddUserHabitDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var now = DateTimeOffset.UtcNow;

            UserHabit habit;
            if (!string.IsNullOrWhiteSpace(request.HabitTemplateId))
            {
                if (!Guid.TryParse(request.HabitTemplateId, out var templateId))
                {
                    return Results.BadRequest(new { message = "That habit template is not recognised." });
                }

                var template = await db.HabitTemplates.FirstOrDefaultAsync(item => item.Id == templateId && item.IsActive);
                if (template is null)
                {
                    return Results.NotFound(new { message = "Habit template not found." });
                }

                var existing = await db.UserHabits.FirstOrDefaultAsync(item => item.UserId == userId && item.HabitTemplateId == templateId);
                if (existing is not null)
                {
                    // Re-activate rather than duplicate.
                    existing.IsActive = true;
                    existing.UpdatedAtUtc = now;
                    await db.SaveChangesAsync();
                    return Results.Ok(existing.ToDto(false));
                }

                habit = new UserHabit
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    HabitTemplateId = template.Id,
                    Name = template.Name,
                    Category = template.Category,
                    IsCustom = false,
                    IsActive = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };
            }
            else
            {
                var name = request.Name?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Results.BadRequest(new { message = "Give your habit a short name." });
                }

                if (name.Length > 60)
                {
                    return Results.BadRequest(new { message = "Habit name must be 60 characters or fewer." });
                }

                HabitCategory? category = null;
                if (!string.IsNullOrWhiteSpace(request.Category))
                {
                    if (!Enum.TryParse<HabitCategory>(request.Category, ignoreCase: true, out var parsedCategory))
                    {
                        return Results.BadRequest(new { message = "That habit category is not recognised." });
                    }
                    category = parsedCategory;
                }

                habit = new UserHabit
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    HabitTemplateId = null,
                    Name = name,
                    Category = category,
                    IsCustom = true,
                    IsActive = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };
            }

            db.UserHabits.Add(habit);
            await db.SaveChangesAsync();
            return Results.Created($"/api/habits/mine/{habit.Id}", habit.ToDto(false));
        });

        // Activate / deactivate a habit (soft state, never a hard delete).
        habits.MapPatch("/mine/{id:guid}", async (Guid id, UpdateUserHabitDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var habit = await db.UserHabits.FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);
            if (habit is null)
            {
                return Results.NotFound();
            }

            habit.IsActive = request.IsActive;
            habit.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(habit.ToDto(false));
        });

        // Mark a habit done for a date (idempotent — one completion per habit per date).
        habits.MapPost("/mine/{id:guid}/complete", async (Guid id, WithinDbContext db, ClaimsPrincipal principal, string? date) =>
        {
            var userId = principal.UserId();
            var completionDate = ResolveDate(date);
            var habit = await db.UserHabits.FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);
            if (habit is null)
            {
                return Results.NotFound();
            }

            var already = await db.HabitCompletions.AnyAsync(item => item.UserHabitId == id && item.CompletionDate == completionDate);
            if (!already)
            {
                db.HabitCompletions.Add(new HabitCompletion
                {
                    Id = Guid.NewGuid(),
                    UserHabitId = id,
                    UserId = userId,
                    CompletionDate = completionDate,
                    CompletedAtUtc = DateTimeOffset.UtcNow
                });
                await db.SaveChangesAsync();
            }

            return Results.Ok(habit.ToDto(true));
        });

        // Undo a habit completion for a date.
        habits.MapDelete("/mine/{id:guid}/complete", async (Guid id, WithinDbContext db, ClaimsPrincipal principal, string? date) =>
        {
            var userId = principal.UserId();
            var completionDate = ResolveDate(date);
            var completion = await db.HabitCompletions
                .FirstOrDefaultAsync(item => item.UserHabitId == id && item.UserId == userId && item.CompletionDate == completionDate);
            if (completion is not null)
            {
                db.HabitCompletions.Remove(completion);
                await db.SaveChangesAsync();
            }

            return Results.NoContent();
        });

        MapAdminHabitTemplateEndpoints(app);
        return app;
    }

    // Master data: habit templates are managed from the admin portal (no seed scripts).
    private static void MapAdminHabitTemplateEndpoints(IEndpointRouteBuilder app)
    {
        var templates = app.MapGroup("/api/admin/habits/templates").RequireAuthorization("AdminOnly");

        templates.MapGet("", async (WithinDbContext db) =>
            Results.Ok(await db.HabitTemplates
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name)
                .Select(item => new AdminHabitTemplateDto(
                    item.Id.ToString(), item.Name, item.Category.ToString(),
                    item.Description, item.IconKey, item.SortOrder, item.IsActive))
                .ToArrayAsync()));

        templates.MapPost("", async (CreateHabitTemplateRequest request, WithinDbContext db) =>
        {
            var validation = ValidateTemplate(request.Name, request.Category, request.Description, request.IconKey,
                out var name, out var category, out var description, out var iconKey);
            if (validation is not null) return Results.BadRequest(new { message = validation });
            if (await db.HabitTemplates.AnyAsync(item => item.Name == name))
            {
                return Results.Conflict(new { message = "A habit template with that name already exists." });
            }

            var template = new HabitTemplate
            {
                Id = Guid.NewGuid(),
                Name = name,
                Category = category,
                Description = description,
                IconKey = iconKey,
                SortOrder = request.SortOrder ?? 0,
                IsActive = true
            };
            db.HabitTemplates.Add(template);
            await db.SaveChangesAsync();
            return Results.Created($"/api/admin/habits/templates/{template.Id}", ToAdminTemplateDto(template));
        });

        templates.MapPut("/{templateId:guid}", async (Guid templateId, UpdateHabitTemplateRequest request, WithinDbContext db) =>
        {
            var template = await db.HabitTemplates.FindAsync(templateId);
            if (template is null) return Results.NotFound();

            var validation = ValidateTemplate(request.Name, request.Category, request.Description, request.IconKey,
                out var name, out var category, out var description, out var iconKey);
            if (validation is not null) return Results.BadRequest(new { message = validation });
            if (await db.HabitTemplates.AnyAsync(item => item.Name == name && item.Id != templateId))
            {
                return Results.Conflict(new { message = "A habit template with that name already exists." });
            }

            template.Name = name;
            template.Category = category;
            template.Description = description;
            template.IconKey = iconKey;
            template.SortOrder = request.SortOrder;
            template.IsActive = request.IsActive;
            await db.SaveChangesAsync();
            return Results.Ok(ToAdminTemplateDto(template));
        });

        templates.MapDelete("/{templateId:guid}", async (Guid templateId, WithinDbContext db) =>
        {
            var template = await db.HabitTemplates.FindAsync(templateId);
            if (template is null) return Results.NotFound();
            template.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static AdminHabitTemplateDto ToAdminTemplateDto(HabitTemplate template) => new(
        template.Id.ToString(),
        template.Name,
        template.Category.ToString(),
        template.Description,
        template.IconKey,
        template.SortOrder,
        template.IsActive);

    private static string? ValidateTemplate(string? rawName, string? rawCategory, string? rawDescription, string? rawIconKey,
        out string name, out HabitCategory category, out string? description, out string? iconKey)
    {
        name = rawName?.Trim() ?? "";
        category = default;
        description = null;
        iconKey = null;

        if (string.IsNullOrWhiteSpace(name) || name.Length > 60)
            return "Habit name is required and must be 60 characters or less.";
        if (!Enum.TryParse(rawCategory, ignoreCase: true, out category))
            return "Choose a valid category (Mind, Body, Lifestyle, Social, or Nature).";

        var trimmedDescription = rawDescription?.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedDescription))
        {
            if (trimmedDescription.Length > 240) return "Description must be 240 characters or less.";
            description = trimmedDescription;
        }

        var trimmedIcon = rawIconKey?.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedIcon))
        {
            if (trimmedIcon.Length > 48) return "Icon key must be 48 characters or less.";
            iconKey = trimmedIcon;
        }

        return null;
    }

    private static async Task<HashSet<Guid>> CompletedHabitIds(WithinDbContext db, Guid userId, DateOnly date)
    {
        var ids = await db.HabitCompletions
            .Where(item => item.UserId == userId && item.CompletionDate == date)
            .Select(item => item.UserHabitId)
            .ToArrayAsync();
        return ids.ToHashSet();
    }

    private static DateOnly ResolveDate(string? date) =>
        DateOnly.TryParse(date, out var parsed) ? parsed : DateOnly.FromDateTime(DateTime.UtcNow);
}
