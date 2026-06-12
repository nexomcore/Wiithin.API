using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class ProgramEndpoints
{
    public static IEndpointRouteBuilder MapProgramEndpoints(this IEndpointRouteBuilder app)
    {
        var provider = app.MapGroup("/api/providers/me/programs").RequireAuthorization("ProviderOnly");

        provider.MapGet("/clients", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var providerId = providerRecord.Id;
            var clients = await (
                from program in db.AssignedPrograms
                join user in db.Users on program.ClientUserId equals user.Id
                where program.ProviderId == providerId
                group program by new { user.Id, user.DisplayName } into groupItems
                orderby groupItems.Key.DisplayName
                select new ProviderProgramClientDto(
                    groupItems.Key.Id,
                    groupItems.Key.DisplayName,
                    groupItems.Count(item => item.Status == AssignedProgramStatus.Active),
                    groupItems.Max(item => (DateTimeOffset?)item.UpdatedAt)))
                .ToArrayAsync();
            return Results.Ok(clients);
        });

        provider.MapGet("/templates", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var templates = await db.ProgramTemplates
                .Where(item => item.ProviderId == providerRecord.Id)
                .OrderByDescending(item => item.UpdatedAt)
                .ToArrayAsync();
            return Results.Ok(await ToTemplateDtos(db, templates));
        });

        provider.MapGet("/templates/{id:guid}", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var template = await db.ProgramTemplates.FirstOrDefaultAsync(item => item.Id == id && item.ProviderId == providerRecord.Id);
            return template is null ? Results.NotFound() : Results.Ok(await ToTemplateDto(db, template));
        });

        provider.MapPost("/templates", async (UpsertProgramTemplateDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var validation = ValidateTemplate(request);
            if (validation is not null) return Results.BadRequest(new { message = validation });

            var now = DateTimeOffset.UtcNow;
            var template = new ProgramTemplate { Id = Guid.NewGuid(), ProviderId = providerRecord.Id, CreatedAt = now, UpdatedAt = now };
            ApplyTemplate(request, template);
            db.ProgramTemplates.Add(template);
            AddTemplateGraph(db, template.Id, request.Weeks);
            await db.SaveChangesAsync();
            return Results.Created($"/api/providers/me/programs/templates/{template.Id}", await ToTemplateDto(db, template));
        });

        provider.MapPut("/templates/{id:guid}", async (Guid id, UpsertProgramTemplateDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var template = await db.ProgramTemplates.FirstOrDefaultAsync(item => item.Id == id && item.ProviderId == providerRecord.Id);
            if (template is null) return Results.NotFound();
            var validation = ValidateTemplate(request);
            if (validation is not null) return Results.BadRequest(new { message = validation });

            ApplyTemplate(request, template);
            template.UpdatedAt = DateTimeOffset.UtcNow;
            await DeleteTemplateGraph(db, id);
            AddTemplateGraph(db, id, request.Weeks);
            await db.SaveChangesAsync();
            return Results.Ok(await ToTemplateDto(db, template));
        });

        provider.MapPost("/assign", async (AssignProgramDto request, WithinDbContext db, ClaimsPrincipal principal, NotificationService notifications) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var template = await db.ProgramTemplates.FirstOrDefaultAsync(item => item.Id == request.ProgramTemplateId && item.ProviderId == providerRecord.Id);
            if (template is null) return Results.NotFound();
            if (!await db.Users.AnyAsync(item => item.Id == request.ClientUserId && !item.IsDeleted)) return Results.BadRequest(new { message = "Client not found." });

            var assigned = await CloneTemplateToAssigned(db, template, providerRecord.Id, request.ClientUserId, request.StartDate, request.Title, request.ProviderNotes);
            await db.SaveChangesAsync();
            await notifications.NotifyProgramAssigned(request.ClientUserId, providerRecord.OwnerUserId, assigned.Id, assigned.Title);
            return Results.Created($"/api/providers/me/programs/assigned/{assigned.Id}", await ToAssignedDto(db, assigned));
        });

        provider.MapGet("/assigned", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var programs = await db.AssignedPrograms
                .Where(item => item.ProviderId == providerRecord.Id)
                .OrderByDescending(item => item.UpdatedAt)
                .ToArrayAsync();
            return Results.Ok(await ToAssignedDtos(db, programs));
        });

        provider.MapGet("/assigned/{id:guid}", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var program = await ProviderProgram(db, principal, id);
            return program is null ? Results.NotFound() : Results.Ok(await ToAssignedDto(db, program));
        });

        provider.MapPut("/assigned/{id:guid}", async (Guid id, UpdateAssignedProgramDto request, WithinDbContext db, ClaimsPrincipal principal, NotificationService notifications) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var program = await db.AssignedPrograms.FirstOrDefaultAsync(item => item.Id == id && item.ProviderId == providerRecord.Id);
            if (program is null) return Results.NotFound();
            program.Title = CleanRequired(request.Title);
            program.Description = CleanRequired(request.Description);
            program.Category = request.Category;
            program.StartDate = request.StartDate;
            program.EndDate = request.EndDate;
            program.Status = request.Status;
            program.ProviderNotes = CleanOptional(request.ProviderNotes);
            program.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            await notifications.NotifyProgramUpdated(program.ClientUserId, providerRecord.OwnerUserId, program.Id, program.Title);
            return Results.Ok(await ToAssignedDto(db, program));
        });

        provider.MapPost("/assigned/{id:guid}/tasks", async (Guid id, UpsertAssignedProgramTaskDto request, WithinDbContext db, ClaimsPrincipal principal, NotificationService notifications) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var program = await db.AssignedPrograms.FirstOrDefaultAsync(item => item.Id == id && item.ProviderId == providerRecord.Id);
            if (program is null) return Results.NotFound();
            var task = new AssignedProgramTask { Id = Guid.NewGuid(), AssignedProgramId = id, Status = AssignedProgramTaskStatus.Pending };
            ApplyAssignedTask(request, task);
            db.AssignedProgramTasks.Add(task);
            program.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            await notifications.NotifyProgramUpdated(program.ClientUserId, providerRecord.OwnerUserId, program.Id, program.Title);
            return Results.Created($"/api/providers/me/programs/assigned/{id}/tasks/{task.Id}", ToTaskDto(task));
        });

        provider.MapPut("/assigned/{id:guid}/tasks/{taskId:guid}", async (Guid id, Guid taskId, UpsertAssignedProgramTaskDto request, WithinDbContext db, ClaimsPrincipal principal, NotificationService notifications) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var program = await db.AssignedPrograms.FirstOrDefaultAsync(item => item.Id == id && item.ProviderId == providerRecord.Id);
            if (program is null) return Results.NotFound();
            var task = await db.AssignedProgramTasks.FirstOrDefaultAsync(item => item.Id == taskId && item.AssignedProgramId == id);
            if (task is null) return Results.NotFound();
            ApplyAssignedTask(request, task);
            program.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            await notifications.NotifyProgramUpdated(program.ClientUserId, providerRecord.OwnerUserId, program.Id, program.Title);
            return Results.Ok(ToTaskDto(task));
        });

        provider.MapDelete("/assigned/{id:guid}/tasks/{taskId:guid}", async (Guid id, Guid taskId, WithinDbContext db, ClaimsPrincipal principal, NotificationService notifications) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var program = await db.AssignedPrograms.FirstOrDefaultAsync(item => item.Id == id && item.ProviderId == providerRecord.Id);
            if (program is null) return Results.NotFound();
            await db.AssignedProgramTasks.Where(item => item.Id == taskId && item.AssignedProgramId == id).ExecuteDeleteAsync();
            program.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            await notifications.NotifyProgramUpdated(program.ClientUserId, providerRecord.OwnerUserId, program.Id, program.Title);
            return Results.NoContent();
        });

        provider.MapPut("/assigned/{id:guid}/tasks/{taskId:guid}/feedback", async (Guid id, Guid taskId, ProviderTaskFeedbackDto request, WithinDbContext db, ClaimsPrincipal principal, NotificationService notifications) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var program = await db.AssignedPrograms.FirstOrDefaultAsync(item => item.Id == id && item.ProviderId == providerRecord.Id);
            if (program is null) return Results.NotFound();
            var task = await db.AssignedProgramTasks.FirstOrDefaultAsync(item => item.Id == taskId && item.AssignedProgramId == id);
            if (task is null) return Results.NotFound();
            task.ProviderFeedback = CleanOptional(request.ProviderFeedback);
            program.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            await notifications.NotifyProgramFeedbackAdded(program.ClientUserId, providerRecord.OwnerUserId, program.Id, $"Feedback added on {task.Title}.");
            return Results.Ok(ToTaskDto(task));
        });

        provider.MapGet("/assigned/{id:guid}/check-ins", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var program = await ProviderProgram(db, principal, id);
            if (program is null) return Results.NotFound();
            var checkIns = await db.ClientCheckIns.Where(item => item.AssignedProgramId == id).OrderByDescending(item => item.CheckInDate).ToArrayAsync();
            return Results.Ok(checkIns.Select(ToCheckInDto).ToArray());
        });

        provider.MapPut("/assigned/{id:guid}/check-ins/{checkInId:guid}/feedback", async (Guid id, Guid checkInId, ProviderCheckInFeedbackDto request, WithinDbContext db, ClaimsPrincipal principal, NotificationService notifications) =>
        {
            var providerRecord = await CurrentProvider(db, principal);
            if (providerRecord is null) return Results.Forbid();
            var program = await db.AssignedPrograms.FirstOrDefaultAsync(item => item.Id == id && item.ProviderId == providerRecord.Id);
            if (program is null) return Results.NotFound();
            var checkIn = await db.ClientCheckIns.FirstOrDefaultAsync(item => item.Id == checkInId && item.AssignedProgramId == id);
            if (checkIn is null) return Results.NotFound();
            checkIn.ProviderFeedback = CleanOptional(request.ProviderFeedback);
            checkIn.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            await notifications.NotifyProgramFeedbackAdded(program.ClientUserId, providerRecord.OwnerUserId, program.Id, "Feedback added to your check-in.");
            return Results.Ok(ToCheckInDto(checkIn));
        });

        var client = app.MapGroup("/api/programs").RequireAuthorization();

        client.MapGet("", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var programs = await db.AssignedPrograms
                .Where(item => item.ClientUserId == userId)
                .OrderByDescending(item => item.Status == AssignedProgramStatus.Active)
                .ThenBy(item => item.StartDate)
                .ToArrayAsync();
            return Results.Ok(await ToAssignedDtos(db, programs));
        });

        client.MapGet("/today", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var programIds = await db.AssignedPrograms
                .Where(item => item.ClientUserId == userId && item.Status == AssignedProgramStatus.Active)
                .Select(item => item.Id)
                .ToArrayAsync();
            var tasks = await db.AssignedProgramTasks
                .Where(item => programIds.Contains(item.AssignedProgramId) && item.ScheduledDate == today)
                .OrderBy(item => item.SortOrder)
                .ToArrayAsync();
            return Results.Ok(tasks.Select(ToTaskDto).ToArray());
        });

        client.MapGet("/{id:guid}", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var program = await ClientProgram(db, principal, id);
            return program is null ? Results.NotFound() : Results.Ok(await ToAssignedDto(db, program));
        });

        client.MapPut("/{id:guid}/tasks/{taskId:guid}/status", async (Guid id, Guid taskId, UpdateAssignedProgramTaskStatusDto request, WithinDbContext db, ClaimsPrincipal principal, NotificationService notifications) =>
        {
            var program = await ClientProgram(db, principal, id);
            if (program is null) return Results.NotFound();
            var task = await db.AssignedProgramTasks.FirstOrDefaultAsync(item => item.Id == taskId && item.AssignedProgramId == id);
            if (task is null) return Results.NotFound();
            task.Status = request.Status;
            task.ClientNotes = CleanOptional(request.ClientNotes);
            task.CompletedAt = request.Status == AssignedProgramTaskStatus.Completed ? DateTimeOffset.UtcNow : null;
            program.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();

            var stats = await TaskStats(db, program.Id);
            if (stats.total > 0 && stats.completed == stats.total && program.Status != AssignedProgramStatus.Completed)
            {
                program.Status = AssignedProgramStatus.Completed;
                program.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync();
                var providerOwnerUserId = await db.Providers.Where(item => item.Id == program.ProviderId).Select(item => item.OwnerUserId).FirstAsync();
                await notifications.NotifyProgramCompleted(providerOwnerUserId, program.ClientUserId, program.Id, program.Title);
            }

            return Results.Ok(ToTaskDto(task));
        });

        client.MapPost("/{id:guid}/check-ins", async (Guid id, UpsertClientCheckInDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var program = await ClientProgram(db, principal, id);
            if (program is null) return Results.NotFound();
            var now = DateTimeOffset.UtcNow;
            var checkIn = await db.ClientCheckIns.FirstOrDefaultAsync(item => item.AssignedProgramId == id && item.CheckInDate == request.CheckInDate);
            var created = checkIn is null;
            checkIn ??= new ClientCheckIn
            {
                Id = Guid.NewGuid(),
                AssignedProgramId = id,
                ClientUserId = program.ClientUserId,
                ProviderId = program.ProviderId,
                CreatedAt = now
            };
            ApplyCheckIn(request, checkIn);
            checkIn.UpdatedAt = now;
            if (created) db.ClientCheckIns.Add(checkIn);
            await db.SaveChangesAsync();
            return created ? Results.Created($"/api/programs/{id}/check-ins/{checkIn.Id}", ToCheckInDto(checkIn)) : Results.Ok(ToCheckInDto(checkIn));
        });

        client.MapGet("/{id:guid}/check-ins", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var program = await ClientProgram(db, principal, id);
            if (program is null) return Results.NotFound();
            var checkIns = await db.ClientCheckIns.Where(item => item.AssignedProgramId == id).OrderByDescending(item => item.CheckInDate).ToArrayAsync();
            return Results.Ok(checkIns.Select(ToCheckInDto).ToArray());
        });

        return app;
    }

    private static async Task<Provider?> CurrentProvider(WithinDbContext db, ClaimsPrincipal principal) =>
        await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == principal.UserId());

    private static async Task<AssignedProgram?> ProviderProgram(WithinDbContext db, ClaimsPrincipal principal, Guid id)
    {
        var provider = await CurrentProvider(db, principal);
        return provider is null ? null : await db.AssignedPrograms.FirstOrDefaultAsync(item => item.Id == id && item.ProviderId == provider.Id);
    }

    private static async Task<AssignedProgram?> ClientProgram(WithinDbContext db, ClaimsPrincipal principal, Guid id)
    {
        var userId = principal.UserId();
        return await db.AssignedPrograms.FirstOrDefaultAsync(item => item.Id == id && item.ClientUserId == userId);
    }

    private static string? ValidateTemplate(UpsertProgramTemplateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) return "Program title is required.";
        if (request.DurationWeeks < 1 || request.DurationWeeks > 52) return "Duration must be between 1 and 52 weeks.";
        if (request.Weeks.Length == 0) return "Add at least one week.";
        return null;
    }

    private static void ApplyTemplate(UpsertProgramTemplateDto request, ProgramTemplate template)
    {
        template.Title = CleanRequired(request.Title);
        template.Description = CleanRequired(request.Description);
        template.Category = request.Category;
        template.DurationWeeks = request.DurationWeeks;
        template.DifficultyLevel = CleanRequired(request.DifficultyLevel);
        template.Goal = CleanRequired(request.Goal);
        template.IsPublicTemplate = request.IsPublicTemplate;
    }

    private static void AddTemplateGraph(WithinDbContext db, Guid templateId, ProgramTemplateWeekInputDto[] weeks)
    {
        foreach (var weekInput in weeks.OrderBy(item => item.WeekNumber))
        {
            var week = new ProgramTemplateWeek
            {
                Id = Guid.NewGuid(),
                ProgramTemplateId = templateId,
                WeekNumber = weekInput.WeekNumber,
                Title = CleanRequired(weekInput.Title),
                Description = CleanOptional(weekInput.Description)
            };
            db.ProgramTemplateWeeks.Add(week);
            foreach (var dayInput in weekInput.Days.OrderBy(item => item.DayNumber))
            {
                var day = new ProgramTemplateDay
                {
                    Id = Guid.NewGuid(),
                    ProgramTemplateWeekId = week.Id,
                    DayNumber = dayInput.DayNumber,
                    Title = CleanRequired(dayInput.Title),
                    Description = CleanOptional(dayInput.Description)
                };
                db.ProgramTemplateDays.Add(day);
                foreach (var taskInput in dayInput.Tasks.OrderBy(item => item.SortOrder))
                {
                    var task = new ProgramTemplateTask { Id = Guid.NewGuid(), ProgramTemplateDayId = day.Id };
                    ApplyTemplateTask(taskInput, task);
                    db.ProgramTemplateTasks.Add(task);
                }
            }
        }
    }

    private static async Task DeleteTemplateGraph(WithinDbContext db, Guid templateId)
    {
        var weekIds = await db.ProgramTemplateWeeks.Where(item => item.ProgramTemplateId == templateId).Select(item => item.Id).ToArrayAsync();
        var dayIds = await db.ProgramTemplateDays.Where(item => weekIds.Contains(item.ProgramTemplateWeekId)).Select(item => item.Id).ToArrayAsync();
        await db.ProgramTemplateTasks.Where(item => dayIds.Contains(item.ProgramTemplateDayId)).ExecuteDeleteAsync();
        await db.ProgramTemplateDays.Where(item => weekIds.Contains(item.ProgramTemplateWeekId)).ExecuteDeleteAsync();
        await db.ProgramTemplateWeeks.Where(item => item.ProgramTemplateId == templateId).ExecuteDeleteAsync();
    }

    private static void ApplyTemplateTask(ProgramTaskInputDto input, ProgramTemplateTask task)
    {
        task.TaskType = input.TaskType;
        task.Title = CleanRequired(input.Title);
        task.Description = CleanOptional(input.Description);
        task.Instructions = CleanOptional(input.Instructions);
        task.DurationMinutes = Positive(input.DurationMinutes);
        task.Sets = Positive(input.Sets);
        task.Reps = CleanOptional(input.Reps);
        task.Weight = input.Weight;
        task.Distance = input.Distance;
        task.Calories = Positive(input.Calories);
        task.Protein = Positive(input.Protein);
        task.Carbs = Positive(input.Carbs);
        task.Fat = Positive(input.Fat);
        task.AttachmentUrl = CleanOptional(input.AttachmentUrl);
        task.SortOrder = input.SortOrder;
    }

    private static async Task<AssignedProgram> CloneTemplateToAssigned(WithinDbContext db, ProgramTemplate template, Guid providerId, Guid clientUserId, DateOnly startDate, string? title, string? providerNotes)
    {
        var now = DateTimeOffset.UtcNow;
        var assigned = new AssignedProgram
        {
            Id = Guid.NewGuid(),
            ProgramTemplateId = template.Id,
            ProviderId = providerId,
            ClientUserId = clientUserId,
            Title = CleanOptional(title) ?? template.Title,
            Description = template.Description,
            Category = template.Category,
            StartDate = startDate,
            EndDate = startDate.AddDays(template.DurationWeeks * 7 - 1),
            Status = AssignedProgramStatus.Active,
            ProviderNotes = CleanOptional(providerNotes),
            CreatedAt = now,
            UpdatedAt = now
        };
        db.AssignedPrograms.Add(assigned);

        var weeks = await db.ProgramTemplateWeeks.Where(item => item.ProgramTemplateId == template.Id).OrderBy(item => item.WeekNumber).ToArrayAsync();
        var weekIds = weeks.Select(item => item.Id).ToArray();
        var days = await db.ProgramTemplateDays.Where(item => weekIds.Contains(item.ProgramTemplateWeekId)).ToArrayAsync();
        var dayIds = days.Select(item => item.Id).ToArray();
        var tasks = await db.ProgramTemplateTasks.Where(item => dayIds.Contains(item.ProgramTemplateDayId)).OrderBy(item => item.SortOrder).ToArrayAsync();
        foreach (var week in weeks)
        {
            foreach (var day in days.Where(item => item.ProgramTemplateWeekId == week.Id).OrderBy(item => item.DayNumber))
            {
                var scheduledDate = startDate.AddDays((week.WeekNumber - 1) * 7 + (day.DayNumber - 1));
                foreach (var task in tasks.Where(item => item.ProgramTemplateDayId == day.Id))
                {
                    db.AssignedProgramTasks.Add(new AssignedProgramTask
                    {
                        Id = Guid.NewGuid(),
                        AssignedProgramId = assigned.Id,
                        WeekNumber = week.WeekNumber,
                        DayNumber = day.DayNumber,
                        TaskType = task.TaskType,
                        Title = task.Title,
                        Description = task.Description,
                        Instructions = task.Instructions,
                        DurationMinutes = task.DurationMinutes,
                        Sets = task.Sets,
                        Reps = task.Reps,
                        Weight = task.Weight,
                        Distance = task.Distance,
                        Calories = task.Calories,
                        Protein = task.Protein,
                        Carbs = task.Carbs,
                        Fat = task.Fat,
                        AttachmentUrl = task.AttachmentUrl,
                        ScheduledDate = scheduledDate,
                        Status = AssignedProgramTaskStatus.Pending,
                        SortOrder = task.SortOrder
                    });
                }
            }
        }

        return assigned;
    }

    private static void ApplyAssignedTask(UpsertAssignedProgramTaskDto input, AssignedProgramTask task)
    {
        task.WeekNumber = input.WeekNumber;
        task.DayNumber = input.DayNumber;
        task.TaskType = input.TaskType;
        task.Title = CleanRequired(input.Title);
        task.Description = CleanOptional(input.Description);
        task.Instructions = CleanOptional(input.Instructions);
        task.DurationMinutes = Positive(input.DurationMinutes);
        task.Sets = Positive(input.Sets);
        task.Reps = CleanOptional(input.Reps);
        task.Weight = input.Weight;
        task.Distance = input.Distance;
        task.Calories = Positive(input.Calories);
        task.Protein = Positive(input.Protein);
        task.Carbs = Positive(input.Carbs);
        task.Fat = Positive(input.Fat);
        task.AttachmentUrl = CleanOptional(input.AttachmentUrl);
        task.ScheduledDate = input.ScheduledDate;
        task.SortOrder = input.SortOrder;
    }

    private static void ApplyCheckIn(UpsertClientCheckInDto input, ClientCheckIn checkIn)
    {
        checkIn.CheckInDate = input.CheckInDate;
        checkIn.Weight = input.Weight;
        checkIn.EnergyLevel = input.EnergyLevel;
        checkIn.StressLevel = input.StressLevel;
        checkIn.SleepQuality = input.SleepQuality;
        checkIn.Mood = CleanOptional(input.Mood);
        checkIn.ComplianceScore = input.ComplianceScore;
        checkIn.ClientNotes = CleanOptional(input.ClientNotes);
    }

    private static async Task<ProgramTemplateDto[]> ToTemplateDtos(WithinDbContext db, ProgramTemplate[] templates)
    {
        var result = new List<ProgramTemplateDto>();
        foreach (var template in templates) result.Add(await ToTemplateDto(db, template));
        return result.ToArray();
    }

    private static async Task<ProgramTemplateDto> ToTemplateDto(WithinDbContext db, ProgramTemplate template)
    {
        var weeks = await db.ProgramTemplateWeeks.Where(item => item.ProgramTemplateId == template.Id).OrderBy(item => item.WeekNumber).ToArrayAsync();
        var weekIds = weeks.Select(item => item.Id).ToArray();
        var days = await db.ProgramTemplateDays.Where(item => weekIds.Contains(item.ProgramTemplateWeekId)).OrderBy(item => item.DayNumber).ToArrayAsync();
        var dayIds = days.Select(item => item.Id).ToArray();
        var tasks = await db.ProgramTemplateTasks.Where(item => dayIds.Contains(item.ProgramTemplateDayId)).OrderBy(item => item.SortOrder).ToArrayAsync();
        var weekDtos = weeks.Select(week => new ProgramTemplateWeekDto(
            week.Id,
            week.WeekNumber,
            week.Title,
            week.Description,
            days.Where(day => day.ProgramTemplateWeekId == week.Id).Select(day => new ProgramTemplateDayDto(
                day.Id,
                day.DayNumber,
                day.Title,
                day.Description,
                tasks.Where(task => task.ProgramTemplateDayId == day.Id).Select(ToTemplateTaskDto).ToArray())).ToArray())).ToArray();
        return new ProgramTemplateDto(template.Id, template.ProviderId, template.Title, template.Description, template.Category, template.DurationWeeks, template.DifficultyLevel, template.Goal, template.IsPublicTemplate, weekDtos, template.CreatedAt, template.UpdatedAt);
    }

    private static ProgramTemplateTaskDto ToTemplateTaskDto(ProgramTemplateTask task) => new(task.Id, task.TaskType, task.Title, task.Description, task.Instructions, task.DurationMinutes, task.Sets, task.Reps, task.Weight, task.Distance, task.Calories, task.Protein, task.Carbs, task.Fat, task.AttachmentUrl, task.SortOrder);

    private static async Task<AssignedProgramDto[]> ToAssignedDtos(WithinDbContext db, AssignedProgram[] programs)
    {
        var result = new List<AssignedProgramDto>();
        foreach (var program in programs) result.Add(await ToAssignedDto(db, program));
        return result.ToArray();
    }

    private static async Task<AssignedProgramDto> ToAssignedDto(WithinDbContext db, AssignedProgram program)
    {
        var tasks = await db.AssignedProgramTasks.Where(item => item.AssignedProgramId == program.Id).OrderBy(item => item.ScheduledDate).ThenBy(item => item.SortOrder).ToArrayAsync();
        var providerName = await db.Providers.Where(item => item.Id == program.ProviderId).Select(item => item.Name).FirstOrDefaultAsync() ?? "Provider";
        var clientName = await db.Users.Where(item => item.Id == program.ClientUserId).Select(item => item.DisplayName).FirstOrDefaultAsync() ?? "Client";
        var completed = tasks.Count(item => item.Status == AssignedProgramTaskStatus.Completed);
        var skipped = tasks.Count(item => item.Status == AssignedProgramTaskStatus.Skipped);
        var pending = tasks.Count(item => item.Status == AssignedProgramTaskStatus.Pending);
        var progress = tasks.Length == 0 ? 0 : (int)Math.Round((double)completed / tasks.Length * 100);
        var streak = CurrentStreak(tasks);
        return new AssignedProgramDto(program.Id, program.ProgramTemplateId, program.ProviderId, providerName, program.ClientUserId, clientName, program.Title, program.Description, program.Category, program.StartDate, program.EndDate, program.Status, program.ProviderNotes, progress, completed, skipped, pending, streak, tasks.Select(ToTaskDto).ToArray(), program.CreatedAt, program.UpdatedAt);
    }

    private static AssignedProgramTaskDto ToTaskDto(AssignedProgramTask task) => new(task.Id, task.AssignedProgramId, task.WeekNumber, task.DayNumber, task.TaskType, task.Title, task.Description, task.Instructions, task.DurationMinutes, task.Sets, task.Reps, task.Weight, task.Distance, task.Calories, task.Protein, task.Carbs, task.Fat, task.AttachmentUrl, task.ScheduledDate, task.Status, task.ClientNotes, task.ProviderFeedback, task.CompletedAt, task.SortOrder);

    private static ClientCheckInDto ToCheckInDto(ClientCheckIn checkIn) => new(checkIn.Id, checkIn.AssignedProgramId, checkIn.ClientUserId, checkIn.ProviderId, checkIn.CheckInDate, checkIn.Weight, checkIn.EnergyLevel, checkIn.StressLevel, checkIn.SleepQuality, checkIn.Mood, checkIn.ComplianceScore, checkIn.ClientNotes, checkIn.ProviderFeedback, checkIn.CreatedAt, checkIn.UpdatedAt);

    private static async Task<(int total, int completed)> TaskStats(WithinDbContext db, Guid programId)
    {
        var statuses = await db.AssignedProgramTasks.Where(item => item.AssignedProgramId == programId).Select(item => item.Status).ToArrayAsync();
        return (statuses.Length, statuses.Count(item => item == AssignedProgramTaskStatus.Completed));
    }

    private static int CurrentStreak(AssignedProgramTask[] tasks)
    {
        var completedDates = tasks.Where(item => item.Status == AssignedProgramTaskStatus.Completed).Select(item => item.ScheduledDate).Distinct().ToHashSet();
        var day = DateOnly.FromDateTime(DateTime.UtcNow);
        var streak = 0;
        while (completedDates.Contains(day))
        {
            streak++;
            day = day.AddDays(-1);
        }
        return streak;
    }

    private static int? Positive(int? value) => value is > 0 ? value : null;

    private static string CleanRequired(string value) => value.Trim();

    private static string? CleanOptional(string? value)
    {
        var clean = value?.Trim();
        return string.IsNullOrWhiteSpace(clean) ? null : clean;
    }
}
