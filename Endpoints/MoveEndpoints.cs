using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

// Functional MVP for the Move pillar (physical wellbeing). Wires the app's existing
// /api/move/* contract to real data. Private Move data (profile, body metrics, logs,
// trainer notes) is owner-only unless an Active trainer relationship + share flag allows
// it — enforced via MoveAccessRules.
public static class MoveEndpoints
{
    // Calculator request bodies (kept local to the module).
    public sealed record BmiRequest(double HeightCm, double WeightKg);
    public sealed record BmrRequest(double WeightKg, double HeightCm, int Age, string? Gender);
    public sealed record TdeeRequest(double Bmr, string? ActivityLevel);
    public sealed record MacroRequest(int CalorieTarget, string? Goal);
    public sealed record BodyFatRequest(string? Gender, double HeightCm, double NeckCm, double WaistCm, double? HipCm);
    public sealed record OneRepMaxRequest(double Weight, int Reps);
    public sealed record WaterRequest(double WeightKg, string? ActivityLevel);
    public sealed record HeartRateRequest(int Age, double IntensityPercentMin, double IntensityPercentMax);

    public static IEndpointRouteBuilder MapMoveEndpoints(this IEndpointRouteBuilder app)
    {
        var move = app.MapGroup("/api/move").RequireAuthorization();

        MapDashboard(move);
        MapProfile(move);
        MapBodyMetrics(move);
        MapCalculators(move);
        MapWorkouts(move);
        MapDiet(move);
        MapTemplates(move);
        MapChallenges(move);
        MapTrainer(move);
        return app;
    }

    // ── Dashboard ──
    private static void MapDashboard(RouteGroupBuilder move)
    {
        move.MapGet("/dashboard", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var profile = await db.MoveProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var latestMetric = await db.BodyMetricLogs.Where(m => m.UserId == userId)
                .OrderByDescending(m => m.LoggedAt).FirstOrDefaultAsync();

            var activeWorkout = await db.WorkoutPlans
                .Where(p => p.AssignedToUserId == userId && p.Status == MovePlanStatus.Active)
                .OrderByDescending(p => p.UpdatedUtc).FirstOrDefaultAsync();
            var activeDiet = await db.DietPlans
                .Where(p => p.AssignedToUserId == userId && p.Status == MovePlanStatus.Active)
                .OrderByDescending(p => p.UpdatedUtc).FirstOrDefaultAsync();

            var activeParticipant = await db.ChallengeParticipants
                .Where(p => p.UserId == userId && (p.Status == ChallengeParticipantStatus.Joined || p.Status == ChallengeParticipantStatus.Active))
                .OrderByDescending(p => p.JoinedAt).FirstOrDefaultAsync();
            DashboardChallengeDto? challengeDto = null;
            if (activeParticipant is not null)
            {
                var challenge = await db.Challenges.FindAsync(activeParticipant.ChallengeId);
                if (challenge is not null)
                {
                    var progressDays = await db.ChallengeProgresses
                        .Where(p => p.ChallengeId == challenge.Id && p.UserId == userId && p.Status == ChallengeProgressStatus.Completed)
                        .Select(p => p.LogDate).Distinct().CountAsync();
                    challengeDto = new DashboardChallengeDto(challenge.Id.ToString(), challenge.Title, progressDays, challenge.DurationDays);
                }
            }

            var trainerLink = await db.TrainerClients
                .FirstOrDefaultAsync(t => t.ClientUserId == userId && t.Status == TrainerClientStatus.Active);
            DashboardTrainerDto? trainerDto = null;
            if (trainerLink is not null)
            {
                var name = await UserName(db, trainerLink.TrainerUserId);
                trainerDto = new DashboardTrainerDto(true, name);
            }

            var weekStart = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-6);
            var weeklyWorkouts = await db.WorkoutLogs
                .Where(l => l.UserId == userId && l.LogDate >= weekStart && l.Status == WorkoutLogStatus.Completed)
                .Select(l => l.LogDate).Distinct().CountAsync();
            var dietAdherence = await WeeklyDietAdherence(db, userId, weekStart);

            return Results.Ok(new MoveDashboardDto(
                profile is not null,
                profile is null ? null : ToProfileDto(profile),
                latestMetric is null ? null : ToMetricDto(latestMetric),
                activeWorkout is null ? null : new DashboardWorkoutDto(activeWorkout.Id.ToString(), activeWorkout.Title, activeWorkout.DaysPerWeek),
                activeDiet is null ? null : new DashboardDietDto(activeDiet.Id.ToString(), activeDiet.Title, activeDiet.Calories),
                challengeDto,
                trainerDto,
                weeklyWorkouts,
                dietAdherence));
        });
    }

    // ── Profile ──
    private static void MapProfile(RouteGroupBuilder move)
    {
        move.MapGet("/profile", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var profile = await db.MoveProfiles.FirstOrDefaultAsync(p => p.UserId == principal.UserId());
            return profile is null ? Results.Ok(null as MoveProfileDto) : Results.Ok(ToProfileDto(profile));
        });

        move.MapPost("/profile", (MoveProfileInputDto body, WithinDbContext db, ClaimsPrincipal principal) =>
            UpsertProfile(body, db, principal, mustExist: false));

        move.MapPut("/profile", (MoveProfileInputDto body, WithinDbContext db, ClaimsPrincipal principal) =>
            UpsertProfile(body, db, principal, mustExist: false));
    }

    private static async Task<IResult> UpsertProfile(MoveProfileInputDto body, WithinDbContext db, ClaimsPrincipal principal, bool mustExist)
    {
        var userId = principal.UserId();
        if (body.HeightCm is < 50 or > 300)
            return Results.BadRequest(new { message = "Enter a height between 50 and 300 cm." });
        if (body.WeightKg is < 20 or > 500)
            return Results.BadRequest(new { message = "Enter a weight between 20 and 500 kg." });
        if (body.AvailableDaysPerWeek is < 1 or > 7)
            return Results.BadRequest(new { message = "Available days per week must be between 1 and 7." });
        if (body.SessionDurationMinutes is < 1)
            return Results.BadRequest(new { message = "Session duration must be a positive number of minutes." });

        var now = DateTimeOffset.UtcNow;
        var profile = await db.MoveProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        var created = profile is null;
        profile ??= new MoveProfile { Id = Guid.NewGuid(), UserId = userId, CreatedUtc = now };

        profile.HeightCm = body.HeightCm;
        profile.WeightKg = body.WeightKg;
        profile.DateOfBirth = ParseDate(body.DateOfBirth);
        profile.Age = profile.DateOfBirth is DateOnly dob ? AgeFrom(dob) : profile.Age;
        profile.GenderForCalculation = Lower(body.Gender);
        profile.ActivityLevel = Lower(body.ActivityLevel);
        profile.FitnessGoal = Lower(body.FitnessGoal);
        profile.ExperienceLevel = Lower(body.ExperienceLevel);
        profile.PreferredWorkoutType = Lower(body.PreferredWorkoutType);
        profile.AvailableDaysPerWeek = body.AvailableDaysPerWeek;
        profile.SessionDurationMinutes = body.SessionDurationMinutes;
        profile.EquipmentAvailable = body.Equipment ?? [];
        profile.InjuriesOrLimitations = Trim(body.Injuries);
        profile.DietPreference = Lower(body.DietPreference);
        profile.FoodAllergies = Trim(body.FoodAllergies);
        profile.UpdatedUtc = now;

        if (created) db.MoveProfiles.Add(profile);
        await db.SaveChangesAsync();
        // TODO(move-notify): emit MoveProfileCreated on first save when notification kinds are added.
        return Results.Ok(ToProfileDto(profile));
    }

    // ── Body metrics ──
    private static void MapBodyMetrics(RouteGroupBuilder move)
    {
        move.MapGet("/body-metrics", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var metrics = await db.BodyMetricLogs.Where(m => m.UserId == principal.UserId())
                .OrderByDescending(m => m.LoggedAt).Take(60).ToArrayAsync();
            return Results.Ok(metrics.Select(ToMetricDto).ToArray());
        });

        move.MapPost("/body-metrics", async (BodyMetricInputDto body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (body.WeightKg is < 20 or > 500)
                return Results.BadRequest(new { message = "Enter a weight between 20 and 500 kg." });

            var profile = await db.MoveProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            decimal? bmi = null, bmr = null, tdee = null;
            if (profile?.HeightCm is decimal h && h > 0)
            {
                bmi = (decimal)MoveCalculators.Bmi((double)h, (double)body.WeightKg).Bmi;
                if (profile.Age is int age)
                {
                    var bmrResult = MoveCalculators.Bmr((double)body.WeightKg, (double)h, age, profile.GenderForCalculation);
                    bmr = (decimal)bmrResult.Bmr;
                    tdee = (decimal)MoveCalculators.Tdee(bmrResult.Bmr, profile.ActivityLevel);
                }
            }

            var now = DateTimeOffset.UtcNow;
            var metric = new BodyMetricLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WeightKg = body.WeightKg,
                BodyFatPercentage = body.BodyFatPercentage,
                WaistCm = body.WaistCm,
                HipCm = body.HipCm,
                ChestCm = body.ChestCm,
                ArmCm = body.ArmCm,
                ThighCm = body.ThighCm,
                Bmi = bmi,
                Bmr = bmr,
                Tdee = tdee,
                LoggedAt = now,
                CreatedUtc = now
            };
            db.BodyMetricLogs.Add(metric);
            await db.SaveChangesAsync();
            return Results.Created($"/api/move/body-metrics/{metric.Id}", ToMetricDto(metric));
        });
    }

    // ── Calculators ──
    private static void MapCalculators(RouteGroupBuilder move)
    {
        var calc = move.MapGroup("/calculators");

        calc.MapPost("/bmi", (BmiRequest r) =>
        {
            var result = MoveCalculators.Bmi(r.HeightCm, r.WeightKg);
            return Results.Ok(new { bmi = result.Bmi, category = result.Category, disclaimer = Disclaimer });
        });
        calc.MapPost("/bmr", (BmrRequest r) =>
        {
            var result = MoveCalculators.Bmr(r.WeightKg, r.HeightCm, r.Age, r.Gender);
            return Results.Ok(new { bmr = result.Bmr, isEstimate = result.IsEstimate, unit = "kcal/day", disclaimer = Disclaimer });
        });
        calc.MapPost("/tdee", (TdeeRequest r) =>
            Results.Ok(new { tdee = MoveCalculators.Tdee(r.Bmr, r.ActivityLevel), unit = "kcal/day", disclaimer = Disclaimer }));
        calc.MapPost("/macros", (MacroRequest r) =>
        {
            var m = MoveCalculators.Macros(r.CalorieTarget, r.Goal);
            return Results.Ok(new { calories = m.Calories, proteinG = m.ProteinGrams, carbsG = m.CarbsGrams, fatG = m.FatGrams, disclaimer = Disclaimer });
        });
        calc.MapPost("/body-fat", (BodyFatRequest r) =>
        {
            try
            {
                var result = MoveCalculators.BodyFat(r.Gender, r.HeightCm, r.NeckCm, r.WaistCm, r.HipCm);
                return Results.Ok(new { bodyFatPercentage = result.Percentage, unit = "%", disclaimer = Disclaimer });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });
        calc.MapPost("/one-rep-max", (OneRepMaxRequest r) =>
        {
            try { return Results.Ok(new { oneRepMax = MoveCalculators.OneRepMax(r.Weight, r.Reps), unit = "kg", disclaimer = Disclaimer }); }
            catch (ArgumentOutOfRangeException) { return Results.BadRequest(new { message = "Reps must be between 1 and 20." }); }
        });
        calc.MapPost("/water-intake", (WaterRequest r) =>
            Results.Ok(new { waterMl = MoveCalculators.WaterIntakeMl(r.WeightKg, r.ActivityLevel), unit = "ml/day", disclaimer = Disclaimer }));
        calc.MapPost("/target-heart-rate", (HeartRateRequest r) =>
        {
            try
            {
                var result = MoveCalculators.TargetHeartRate(r.Age, r.IntensityPercentMin, r.IntensityPercentMax);
                return Results.Ok(new { maxHeartRate = result.MaxHeartRate, targetMin = result.TargetMin, targetMax = result.TargetMax, unit = "bpm", disclaimer = Disclaimer });
            }
            catch (ArgumentOutOfRangeException) { return Results.BadRequest(new { message = "Enter a valid age." }); }
        });

        // Persist a computed result (input + output kept as JSON).
        calc.MapPost("/save", async (CalculatorSaveDto body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!Enum.TryParse<CalculatorType>(body.CalculatorType, ignoreCase: true, out var type))
                return Results.BadRequest(new { message = "Unknown calculator type." });

            var result = new CalculatorResult
            {
                Id = Guid.NewGuid(),
                UserId = principal.UserId(),
                CalculatorType = type,
                InputJson = JsonSerializer.Serialize(body.Input),
                ResultJson = JsonSerializer.Serialize(body.Result),
                CreatedUtc = DateTimeOffset.UtcNow
            };
            db.CalculatorResults.Add(result);
            await db.SaveChangesAsync();
            return Results.Created($"/api/move/calculators/history/{result.Id}",
                new CalculatorHistoryDto(result.Id.ToString(), type.ToString(), result.InputJson, result.ResultJson, result.CreatedUtc.ToString("O")));
        });

        calc.MapGet("/history", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var rows = await db.CalculatorResults.Where(r => r.UserId == principal.UserId())
                .OrderByDescending(r => r.CreatedUtc).Take(50).ToArrayAsync();
            return Results.Ok(rows.Select(r => new CalculatorHistoryDto(
                r.Id.ToString(), r.CalculatorType.ToString(), r.InputJson, r.ResultJson, r.CreatedUtc.ToString("O"))).ToArray());
        });
    }

    // ── Workouts ──
    private static void MapWorkouts(RouteGroupBuilder move)
    {
        move.MapGet("/workout-plans", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var plans = await db.WorkoutPlans
                .Where(p => p.AssignedToUserId == userId && !p.IsTemplate)
                .OrderByDescending(p => p.Status == MovePlanStatus.Active)
                .ThenByDescending(p => p.UpdatedUtc)
                .ToArrayAsync();
            var dtos = new List<WorkoutPlanDto>();
            foreach (var plan in plans) dtos.Add(await LoadWorkoutDto(db, plan));
            return Results.Ok(dtos.ToArray());
        });

        move.MapGet("/workout-plans/{id:guid}", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var plan = await db.WorkoutPlans.FindAsync(id);
            if (plan is null || plan.AssignedToUserId != principal.UserId()) return Results.NotFound();
            return Results.Ok(await LoadWorkoutDto(db, plan));
        });

        move.MapPost("/workout-plans/generate", async (GeneratePlanRequestDto? body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var profile = await db.MoveProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var goal = body?.Goal ?? profile?.FitnessGoal;
            var type = body?.WorkoutType ?? profile?.PreferredWorkoutType;
            var level = body?.ExperienceLevel ?? profile?.ExperienceLevel;
            var days = body?.DaysPerWeek ?? profile?.AvailableDaysPerWeek;

            var templates = await db.WorkoutPlans.Where(p => p.IsTemplate).ToArrayAsync();
            var template = MovePlanRules.MatchWorkoutTemplate(templates, goal, type, level, days);
            if (template is null) return Results.BadRequest(new { message = "No workout templates are available yet." });

            var activate = body?.Activate ?? true;
            var plan = await CloneWorkoutTemplate(db, template, userId, MovePlanSourceType.SelfGenerated, userId, null, activate);
            await db.SaveChangesAsync();
            return Results.Created($"/api/move/workout-plans/{plan.Id}", await LoadWorkoutDto(db, plan));
        });

        move.MapPost("/workout-plans/{id:guid}/activate", (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
            ChangeWorkoutStatus(id, db, principal, MovePlanStatus.Active));
        move.MapPost("/workout-plans/{id:guid}/pause", (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
            ChangeWorkoutStatus(id, db, principal, MovePlanStatus.Paused));
        move.MapPost("/workout-plans/{id:guid}/archive", (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
            ChangeWorkoutStatus(id, db, principal, MovePlanStatus.Archived));

        move.MapGet("/workout-logs", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var logs = await db.WorkoutLogs.Where(l => l.UserId == principal.UserId())
                .OrderByDescending(l => l.LogDate).ThenByDescending(l => l.CreatedUtc).Take(60).ToArrayAsync();
            return Results.Ok(logs.Select(ToWorkoutLogDto).ToArray());
        });

        move.MapPost("/workout-logs", async (LogWorkoutInputDto body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var status = Enum.TryParse<WorkoutLogStatus>(body.Status, ignoreCase: true, out var s) ? s : WorkoutLogStatus.Completed;
            Guid? dayId = Guid.TryParse(body.WorkoutDayId, out var d) ? d : null;
            Guid? planId = null;
            if (dayId is Guid dv)
            {
                planId = await db.WorkoutDays.Where(x => x.Id == dv).Select(x => (Guid?)x.WorkoutPlanId).FirstOrDefaultAsync();
                // Guard against logging a day that doesn't belong to the user's plan.
                if (planId is Guid pv && !await db.WorkoutPlans.AnyAsync(p => p.Id == pv && p.AssignedToUserId == userId))
                    return Results.Forbid();
                if (await db.WorkoutLogs.AnyAsync(l => l.UserId == userId && l.WorkoutDayId == dayId && l.LogDate == today))
                    return Results.Conflict(new { message = "You've already logged this session today." });
            }

            var now = DateTimeOffset.UtcNow;
            var log = new WorkoutLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WorkoutPlanId = planId,
                WorkoutDayId = dayId,
                Title = Trim(body.Title),
                DurationMinutes = body.DurationMinutes,
                Status = status,
                LogDate = today,
                CompletedAt = status == WorkoutLogStatus.Completed ? now : null,
                Notes = Trim(body.Notes),
                CreatedUtc = now
            };
            db.WorkoutLogs.Add(log);
            await db.SaveChangesAsync();
            return Results.Created($"/api/move/workout-logs/{log.Id}", ToWorkoutLogDto(log));
        });
    }

    // ── Diet ──
    private static void MapDiet(RouteGroupBuilder move)
    {
        move.MapGet("/diet-plans", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var plans = await db.DietPlans.Where(p => p.AssignedToUserId == userId && !p.IsTemplate)
                .OrderByDescending(p => p.Status == MovePlanStatus.Active).ThenByDescending(p => p.UpdatedUtc).ToArrayAsync();
            var dtos = new List<DietPlanDto>();
            foreach (var plan in plans) dtos.Add(await LoadDietDto(db, plan, userId));
            return Results.Ok(dtos.ToArray());
        });

        move.MapGet("/diet-plans/{id:guid}", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var plan = await db.DietPlans.FindAsync(id);
            if (plan is null || plan.AssignedToUserId != principal.UserId()) return Results.NotFound();
            return Results.Ok(await LoadDietDto(db, plan, principal.UserId()));
        });

        move.MapPost("/diet-plans/generate", async (GeneratePlanRequestDto? body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var profile = await db.MoveProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var goal = body?.Goal ?? profile?.FitnessGoal;
            var preference = body?.DietPreference ?? profile?.DietPreference;
            var calorieTarget = body?.CalorieTarget;

            var templates = await db.DietPlans.Where(p => p.IsTemplate).ToArrayAsync();
            var template = MovePlanRules.MatchDietTemplate(templates, goal, preference, calorieTarget);
            if (template is null) return Results.BadRequest(new { message = "No diet templates are available yet." });

            var activate = body?.Activate ?? true;
            var plan = await CloneDietTemplate(db, template, userId, MovePlanSourceType.SelfGenerated, userId, null, activate);
            await db.SaveChangesAsync();
            return Results.Created($"/api/move/diet-plans/{plan.Id}", await LoadDietDto(db, plan, userId));
        });

        move.MapPost("/diet-plans/{id:guid}/activate", (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
            ChangeDietStatus(id, db, principal, MovePlanStatus.Active));
        move.MapPost("/diet-plans/{id:guid}/pause", (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
            ChangeDietStatus(id, db, principal, MovePlanStatus.Paused));
        move.MapPost("/diet-plans/{id:guid}/archive", (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
            ChangeDietStatus(id, db, principal, MovePlanStatus.Archived));

        move.MapGet("/diet-logs", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var logs = await db.DietLogs.Where(l => l.UserId == principal.UserId())
                .OrderByDescending(l => l.LogDate).Take(120).ToArrayAsync();
            return Results.Ok(logs.Select(l => new { id = l.Id.ToString(), mealId = l.DietMealId.ToString(), status = DietStatusToken(l.Status), date = l.LogDate.ToString("yyyy-MM-dd"), note = l.Notes }).ToArray());
        });

        move.MapPost("/diet-logs", async (LogMealInputDto body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!Guid.TryParse(body.MealId, out var mealId)) return Results.BadRequest(new { message = "Invalid meal." });
            var meal = await db.DietMeals.FindAsync(mealId);
            if (meal is null) return Results.NotFound(new { message = "Meal not found." });
            var plan = await db.DietPlans.FindAsync(meal.DietPlanId);
            if (plan is null || plan.AssignedToUserId != userId) return Results.Forbid();

            var status = DietStatusFromToken(body.Status);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var existing = await db.DietLogs.FirstOrDefaultAsync(l => l.UserId == userId && l.DietMealId == mealId && l.LogDate == today);
            var now = DateTimeOffset.UtcNow;
            if (existing is not null)
            {
                existing.Status = status;
                existing.Notes = Trim(body.Note);
                existing.LoggedAt = now;
            }
            else
            {
                db.DietLogs.Add(new DietLog
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    DietPlanId = plan.Id,
                    DietMealId = mealId,
                    Status = status,
                    LogDate = today,
                    LoggedAt = now,
                    Notes = Trim(body.Note),
                    CreatedUtc = now
                });
            }
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    // ── Templates (read-only system catalogue; used by self-generate UI + trainer assignment) ──
    private static void MapTemplates(RouteGroupBuilder move)
    {
        move.MapGet("/workout-templates", async (WithinDbContext db) =>
        {
            var templates = await db.WorkoutPlans.Where(p => p.IsTemplate)
                .OrderBy(p => p.DaysPerWeek).ThenBy(p => p.Title).ToArrayAsync();
            return Results.Ok(templates.Select(t => new WorkoutTemplateDto(
                t.Id.ToString(), t.Title, t.Goal, t.WorkoutType, t.ExperienceLevel, t.DaysPerWeek, t.DurationWeeks)).ToArray());
        });

        move.MapGet("/diet-templates", async (WithinDbContext db) =>
        {
            var templates = await db.DietPlans.Where(p => p.IsTemplate)
                .OrderBy(p => p.Calories).ThenBy(p => p.Title).ToArrayAsync();
            return Results.Ok(templates.Select(t => new DietTemplateDto(
                t.Id.ToString(), t.Title, t.Goal, t.DietPreference, t.Calories, t.DurationWeeks)).ToArray());
        });
    }

    // ── Challenges ──
    private static void MapChallenges(RouteGroupBuilder move)
    {
        move.MapGet("/challenges", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            // Discover: public catalogue (templates + any public, non-circle challenge).
            var challenges = await db.Challenges
                .Where(c => c.IsPublic && c.Visibility == ChallengeVisibility.Public)
                .OrderByDescending(c => c.CreatedUtc).ToArrayAsync();
            var dtos = new List<ChallengeDto>();
            foreach (var c in challenges) dtos.Add(await ToChallengeDto(db, c, userId));
            return Results.Ok(dtos.ToArray());
        });

        move.MapGet("/challenges/my", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var joinedIds = await db.ChallengeParticipants
                .Where(p => p.UserId == userId && p.Status != ChallengeParticipantStatus.Dropped)
                .Select(p => p.ChallengeId).ToArrayAsync();
            var challenges = await db.Challenges.Where(c => joinedIds.Contains(c.Id)).ToArrayAsync();
            var dtos = new List<ChallengeDto>();
            foreach (var c in challenges) dtos.Add(await ToChallengeDto(db, c, userId));
            return Results.Ok(dtos.ToArray());
        });

        move.MapGet("/challenges/{id:guid}", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var challenge = await db.Challenges.FindAsync(id);
            if (challenge is null || !await ChallengeVisibleTo(db, challenge, principal.UserId())) return Results.NotFound();
            return Results.Ok(await ToChallengeDto(db, challenge, principal.UserId()));
        });

        move.MapPost("/challenges/{id:guid}/join", async (Guid id, JoinChallengeInputDto? body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var challenge = await db.Challenges.FindAsync(id);
            if (challenge is null || !await ChallengeVisibleTo(db, challenge, userId)) return Results.NotFound();

            var existing = await db.ChallengeParticipants.FirstOrDefaultAsync(p => p.ChallengeId == id && p.UserId == userId);
            var mode = DisplayModeFromPrivacy(body?.Privacy);
            if (existing is not null)
            {
                existing.Status = ChallengeParticipantStatus.Active;
                existing.DisplayMode = mode;
            }
            else
            {
                db.ChallengeParticipants.Add(new ChallengeParticipant
                {
                    Id = Guid.NewGuid(),
                    ChallengeId = id,
                    UserId = userId,
                    DisplayMode = mode,
                    Status = ChallengeParticipantStatus.Active,
                    JoinedAt = DateTimeOffset.UtcNow
                });
            }
            await db.SaveChangesAsync();
            return Results.Ok(await ToChallengeDto(db, challenge, userId));
        });

        move.MapPost("/challenges/{id:guid}/leave", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var participant = await db.ChallengeParticipants.FirstOrDefaultAsync(p => p.ChallengeId == id && p.UserId == userId);
            if (participant is null) return Results.NotFound();
            participant.Status = ChallengeParticipantStatus.Dropped;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        move.MapPost("/challenges/{id:guid}/progress", async (Guid id, ChallengeProgressInputDto? body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var participant = await db.ChallengeParticipants.FirstOrDefaultAsync(p => p.ChallengeId == id && p.UserId == userId && p.Status != ChallengeParticipantStatus.Dropped);
            if (participant is null) return Results.BadRequest(new { message = "Join the challenge before logging progress." });

            var status = Enum.TryParse<ChallengeProgressStatus>(body?.Status, ignoreCase: true, out var s) ? s : ChallengeProgressStatus.Completed;
            var now = DateTimeOffset.UtcNow;
            db.ChallengeProgresses.Add(new ChallengeProgress
            {
                Id = Guid.NewGuid(),
                ChallengeId = id,
                UserId = userId,
                ChallengeTaskId = Guid.TryParse(body?.TaskId, out var t) ? t : null,
                Status = status,
                ValueCompleted = body?.ValueCompleted,
                Notes = Trim(body?.Note),
                LogDate = DateOnly.FromDateTime(DateTime.UtcNow),
                CompletedAt = status == ChallengeProgressStatus.Completed ? now : null,
                CreatedUtc = now
            });
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        move.MapGet("/challenges/{id:guid}/participants", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var participants = await db.ChallengeParticipants.Where(p => p.ChallengeId == id && p.Status != ChallengeParticipantStatus.Dropped).ToArrayAsync();
            var result = new List<ChallengeParticipantDto>();
            foreach (var p in participants)
            {
                var isSelf = p.UserId == userId;
                // Private participants are hidden from everyone but themselves.
                if (!isSelf && !MoveAccessRules.IsPubliclyVisible(p.DisplayMode)) continue;
                var realName = await UserName(db, p.UserId);
                var display = MoveAccessRules.ResolveParticipantDisplay(realName, p.DisplayMode, isSelf);
                var completed = await db.ChallengeProgresses.CountAsync(x => x.ChallengeId == id && x.UserId == p.UserId && x.Status == ChallengeProgressStatus.Completed);
                result.Add(new ChallengeParticipantDto(p.Id.ToString(), display.DisplayName, display.ShowAvatar, completed, p.Status.ToString()));
            }
            return Results.Ok(result.ToArray());
        });
    }

    // ── Trainer ──
    private static void MapTrainer(RouteGroupBuilder move)
    {
        var trainer = move.MapGroup("/trainer");

        trainer.MapGet("/summary", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var link = await db.TrainerClients.FirstOrDefaultAsync(t => t.ClientUserId == userId && t.Status == TrainerClientStatus.Active);
            var assignedWorkout = await db.WorkoutPlans.Where(p => p.AssignedToUserId == userId && p.SourceType == MovePlanSourceType.TrainerAssigned && p.Status != MovePlanStatus.Archived)
                .OrderByDescending(p => p.UpdatedUtc).Select(p => (Guid?)p.Id).FirstOrDefaultAsync();
            var assignedDiet = await db.DietPlans.Where(p => p.AssignedToUserId == userId && p.SourceType == MovePlanSourceType.TrainerAssigned && p.Status != MovePlanStatus.Archived)
                .OrderByDescending(p => p.UpdatedUtc).Select(p => (Guid?)p.Id).FirstOrDefaultAsync();
            string? latestNote = null;
            if (link is not null)
            {
                latestNote = await db.TrainerNotes
                    .Where(n => n.ClientUserId == userId && n.TrainerUserId == link.TrainerUserId && n.Visibility == TrainerNoteVisibility.SharedWithClient)
                    .OrderByDescending(n => n.CreatedUtc).Select(n => n.Note).FirstOrDefaultAsync();
            }
            var isTrainer = IsProvider(principal);
            var pendingIncoming = isTrainer
                ? await db.TrainerClients.CountAsync(t => t.TrainerUserId == userId && t.Status == TrainerClientStatus.Pending)
                : 0;
            // A connection the user has requested that the trainer hasn't accepted yet.
            var pendingOutgoing = await db.TrainerClients
                .FirstOrDefaultAsync(t => t.ClientUserId == userId && t.Status == TrainerClientStatus.Pending);

            return Results.Ok(new TrainerSummaryDto(
                link is not null,
                link is null ? null : await UserName(db, link.TrainerUserId),
                null,
                link?.Id.ToString(),
                pendingOutgoing is null ? null : await UserName(db, pendingOutgoing.TrainerUserId),
                assignedWorkout?.ToString(),
                assignedDiet?.ToString(),
                latestNote,
                isTrainer,
                pendingIncoming,
                ToPermissionsDto(link)));
        });

        // Client-side: request a connection with a provider/trainer.
        trainer.MapPost("/clients/request", async (TrainerRequestDto body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            // Accept either an explicit trainer user id or a providerId (resolve its owner — the
            // app's provider discovery only exposes providerId, not the owner's user id).
            Guid trainerUserId;
            if (Guid.TryParse(body.TrainerUserId, out var direct))
            {
                trainerUserId = direct;
            }
            else if (Guid.TryParse(body.ProviderId, out var providerId))
            {
                var owner = await db.Providers.Where(p => p.Id == providerId).Select(p => (Guid?)p.OwnerUserId).FirstOrDefaultAsync();
                if (owner is null) return Results.BadRequest(new { message = "Choose a valid trainer." });
                trainerUserId = owner.Value;
            }
            else
            {
                return Results.BadRequest(new { message = "Choose a valid trainer." });
            }
            if (trainerUserId == userId) return Results.BadRequest(new { message = "Choose a valid trainer." });
            var trainerUser = await db.Users.FirstOrDefaultAsync(u => u.Id == trainerUserId && !u.IsDeleted);
            if (trainerUser is null || trainerUser.RoleEnum != WithinRole.Provider)
                return Results.BadRequest(new { message = "That person isn't available as a trainer." });

            var existing = await db.TrainerClients.FirstOrDefaultAsync(t => t.TrainerUserId == trainerUserId && t.ClientUserId == userId);
            var now = DateTimeOffset.UtcNow;
            if (existing is not null)
            {
                if (existing.Status is TrainerClientStatus.Active or TrainerClientStatus.Pending)
                    return Results.Conflict(new { message = "You already have a request or connection with this trainer." });
                existing.Status = TrainerClientStatus.Pending;
                existing.UpdatedUtc = now;
            }
            else
            {
                db.TrainerClients.Add(new TrainerClient
                {
                    Id = Guid.NewGuid(),
                    TrainerUserId = trainerUserId,
                    ClientUserId = userId,
                    Status = TrainerClientStatus.Pending,
                    CreatedUtc = now,
                    UpdatedUtc = now
                });
            }
            await db.SaveChangesAsync();
            // TODO(move-notify): emit TrainerClientRequestCreated to the trainer.
            return Results.Ok(new { message = "Request sent." });
        });

        trainer.MapPut("/clients/{id:guid}/permissions", async (Guid id, TrainerPermissionsDto body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var link = await db.TrainerClients.FirstOrDefaultAsync(t => t.Id == id && t.ClientUserId == userId);
            if (link is null) return Results.NotFound();
            link.ShareMoveProfileWithTrainer = body.ShareProfile;
            link.ShareBodyMetricsWithTrainer = body.ShareBodyMetrics;
            link.ShareWorkoutLogsWithTrainer = body.ShareWorkoutLogs;
            link.ShareDietLogsWithTrainer = body.ShareDietLogs;
            link.ShareChallengeProgressWithTrainer = body.ShareChallengeProgress;
            link.UpdatedUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ToPermissionsDto(link));
        });

        // Convenience PUT used by the app (operates on the user's single active link).
        move.MapPut("/trainer/permissions", async (TrainerPermissionsDto body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var link = await db.TrainerClients.FirstOrDefaultAsync(t => t.ClientUserId == userId && t.Status == TrainerClientStatus.Active);
            if (link is null) return Results.NotFound(new { message = "No active trainer to update." });
            link.ShareMoveProfileWithTrainer = body.ShareProfile;
            link.ShareBodyMetricsWithTrainer = body.ShareBodyMetrics;
            link.ShareWorkoutLogsWithTrainer = body.ShareWorkoutLogs;
            link.ShareDietLogsWithTrainer = body.ShareDietLogs;
            link.ShareChallengeProgressWithTrainer = body.ShareChallengeProgress;
            link.UpdatedUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ToPermissionsDto(link));
        });

        // Client ends / pauses their own relationship.
        trainer.MapPost("/clients/{id:guid}/pause", (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
            SetRelationshipStatus(id, db, principal, TrainerClientStatus.Paused, eitherParty: true));
        trainer.MapPost("/clients/{id:guid}/end", (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
            SetRelationshipStatus(id, db, principal, TrainerClientStatus.Ended, eitherParty: true));

        // Trainer-only surface.
        var console = trainer.MapGroup("").RequireAuthorization("ProviderOnly");

        console.MapGet("/clients", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var trainerId = principal.UserId();
            var links = await db.TrainerClients
                .Where(t => t.TrainerUserId == trainerId && (t.Status == TrainerClientStatus.Active || t.Status == TrainerClientStatus.Pending))
                .ToArrayAsync();
            var clients = new List<TrainerClientDto>();
            foreach (var link in links)
            {
                var name = await UserName(db, link.ClientUserId);
                string? goal = null;
                if (MoveAccessRules.TrainerCanView(link, MoveAccessRules.MoveShareScope.Profile))
                    goal = await db.MoveProfiles.Where(p => p.UserId == link.ClientUserId).Select(p => p.FitnessGoal).FirstOrDefaultAsync();
                clients.Add(new TrainerClientDto(link.Id.ToString(), link.ClientUserId.ToString(), name, goal, link.Status.ToString(), null));
            }
            var pending = links.Count(l => l.Status == TrainerClientStatus.Pending);
            var assigned = await db.WorkoutPlans.CountAsync(p => p.TrainerId == trainerId && p.SourceType == MovePlanSourceType.TrainerAssigned)
                + await db.DietPlans.CountAsync(p => p.TrainerId == trainerId && p.SourceType == MovePlanSourceType.TrainerAssigned);
            return Results.Ok(new TrainerConsoleDto(clients.ToArray(), pending, assigned));
        });

        console.MapPost("/clients/{id:guid}/accept", (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
            TrainerRespondToRequest(id, db, principal, accept: true));
        console.MapPost("/clients/{id:guid}/reject", (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
            TrainerRespondToRequest(id, db, principal, accept: false));

        console.MapGet("/clients/{clientId:guid}/progress", async (Guid clientId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var trainerId = principal.UserId();
            var link = await db.TrainerClients.FirstOrDefaultAsync(t => t.TrainerUserId == trainerId && t.ClientUserId == clientId);
            if (!MoveAccessRules.RelationshipAllowsAssignment(link)) return Results.Forbid();

            object? profile = MoveAccessRules.TrainerCanView(link, MoveAccessRules.MoveShareScope.Profile)
                ? await db.MoveProfiles.Where(p => p.UserId == clientId).Select(p => ToProfileDto(p)).FirstOrDefaultAsync()
                : null;
            object[]? workoutLogs = MoveAccessRules.TrainerCanView(link, MoveAccessRules.MoveShareScope.WorkoutLogs)
                ? (await db.WorkoutLogs.Where(l => l.UserId == clientId).OrderByDescending(l => l.LogDate).Take(30).ToArrayAsync()).Select(ToWorkoutLogDto).Cast<object>().ToArray()
                : null;
            object[]? dietLogs = MoveAccessRules.TrainerCanView(link, MoveAccessRules.MoveShareScope.DietLogs)
                ? (await db.DietLogs.Where(l => l.UserId == clientId).OrderByDescending(l => l.LogDate).Take(60).ToArrayAsync())
                    .Select(l => (object)new { date = l.LogDate.ToString("yyyy-MM-dd"), status = DietStatusToken(l.Status) }).ToArray()
                : null;

            return Results.Ok(new { profile, workoutLogs, dietLogs });
        });

        console.MapPost("/clients/{clientId:guid}/assign-workout", async (Guid clientId, AssignPlanDto body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var trainerId = principal.UserId();
            var link = await db.TrainerClients.FirstOrDefaultAsync(t => t.TrainerUserId == trainerId && t.ClientUserId == clientId);
            if (!MoveAccessRules.RelationshipAllowsAssignment(link)) return Results.Forbid();
            if (!Guid.TryParse(body.TemplateId, out var templateId)) return Results.BadRequest(new { message = "Choose a template." });
            var template = await db.WorkoutPlans.FirstOrDefaultAsync(p => p.Id == templateId && p.IsTemplate);
            if (template is null) return Results.NotFound(new { message = "Template not found." });

            var plan = await CloneWorkoutTemplate(db, template, clientId, MovePlanSourceType.TrainerAssigned, trainerId, trainerId, body.Activate);
            await db.SaveChangesAsync();
            // TODO(move-notify): emit WorkoutPlanAssigned to the client.
            return Results.Created($"/api/move/workout-plans/{plan.Id}", await LoadWorkoutDto(db, plan));
        });

        console.MapPost("/clients/{clientId:guid}/assign-diet", async (Guid clientId, AssignPlanDto body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var trainerId = principal.UserId();
            var link = await db.TrainerClients.FirstOrDefaultAsync(t => t.TrainerUserId == trainerId && t.ClientUserId == clientId);
            if (!MoveAccessRules.RelationshipAllowsAssignment(link)) return Results.Forbid();
            if (!Guid.TryParse(body.TemplateId, out var templateId)) return Results.BadRequest(new { message = "Choose a template." });
            var template = await db.DietPlans.FirstOrDefaultAsync(p => p.Id == templateId && p.IsTemplate);
            if (template is null) return Results.NotFound(new { message = "Template not found." });

            var plan = await CloneDietTemplate(db, template, clientId, MovePlanSourceType.TrainerAssigned, trainerId, trainerId, body.Activate);
            await db.SaveChangesAsync();
            // TODO(move-notify): emit DietPlanAssigned to the client.
            return Results.Created($"/api/move/diet-plans/{plan.Id}", await LoadDietDto(db, plan, clientId));
        });

        console.MapPost("/clients/{clientId:guid}/notes", async (Guid clientId, CreateTrainerNoteDto body, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var trainerId = principal.UserId();
            var link = await db.TrainerClients.FirstOrDefaultAsync(t => t.TrainerUserId == trainerId && t.ClientUserId == clientId);
            if (!MoveAccessRules.RelationshipAllowsAssignment(link)) return Results.Forbid();
            var text = body.Note?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return Results.BadRequest(new { message = "Note can't be empty." });
            var visibility = Enum.TryParse<TrainerNoteVisibility>(body.Visibility, ignoreCase: true, out var v) ? v : TrainerNoteVisibility.TrainerOnly;

            var note = new TrainerNote
            {
                Id = Guid.NewGuid(),
                TrainerUserId = trainerId,
                ClientUserId = clientId,
                Note = text,
                Visibility = visibility,
                CreatedUtc = DateTimeOffset.UtcNow
            };
            db.TrainerNotes.Add(note);
            await db.SaveChangesAsync();
            // TODO(move-notify): emit TrainerNoteShared when visibility is SharedWithClient.
            return Results.Created($"/api/move/trainer/clients/{clientId}/notes/{note.Id}",
                new TrainerNoteDto(note.Id.ToString(), note.Note, note.Visibility.ToString(), note.CreatedUtc.ToString("O")));
        });
    }

    // ── Status-change helpers ──
    private static async Task<IResult> ChangeWorkoutStatus(Guid id, WithinDbContext db, ClaimsPrincipal principal, MovePlanStatus status)
    {
        var userId = principal.UserId();
        var plan = await db.WorkoutPlans.FindAsync(id);
        if (plan is null || plan.AssignedToUserId != userId) return Results.NotFound();
        // Trainer-assigned plans are read-only for the client (they may still log against them).
        if (plan.SourceType == MovePlanSourceType.TrainerAssigned && status != MovePlanStatus.Active)
            return Results.Forbid();

        if (status == MovePlanStatus.Active)
        {
            var userPlans = await db.WorkoutPlans.Where(p => p.AssignedToUserId == userId && !p.IsTemplate)
                .Select(p => new { p.Id, p.Status }).ToArrayAsync();
            foreach (var pid in MovePlanRules.ActivePlansToPause(userPlans.Select(p => (p.Id, p.Status)), id))
                await db.WorkoutPlans.Where(p => p.Id == pid).ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, MovePlanStatus.Paused));
        }
        plan.Status = status;
        plan.UpdatedUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok(await LoadWorkoutDto(db, plan));
    }

    private static async Task<IResult> ChangeDietStatus(Guid id, WithinDbContext db, ClaimsPrincipal principal, MovePlanStatus status)
    {
        var userId = principal.UserId();
        var plan = await db.DietPlans.FindAsync(id);
        if (plan is null || plan.AssignedToUserId != userId) return Results.NotFound();
        if (plan.SourceType == MovePlanSourceType.TrainerAssigned && status != MovePlanStatus.Active)
            return Results.Forbid();

        if (status == MovePlanStatus.Active)
        {
            var userPlans = await db.DietPlans.Where(p => p.AssignedToUserId == userId && !p.IsTemplate)
                .Select(p => new { p.Id, p.Status }).ToArrayAsync();
            foreach (var pid in MovePlanRules.ActivePlansToPause(userPlans.Select(p => (p.Id, p.Status)), id))
                await db.DietPlans.Where(p => p.Id == pid).ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, MovePlanStatus.Paused));
        }
        plan.Status = status;
        plan.UpdatedUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok(await LoadDietDto(db, plan, userId));
    }

    private static async Task<IResult> SetRelationshipStatus(Guid id, WithinDbContext db, ClaimsPrincipal principal, TrainerClientStatus status, bool eitherParty)
    {
        var userId = principal.UserId();
        var link = await db.TrainerClients.FindAsync(id);
        if (link is null || (link.ClientUserId != userId && link.TrainerUserId != userId)) return Results.NotFound();
        link.Status = status;
        if (status == TrainerClientStatus.Ended) link.EndedAt = DateTimeOffset.UtcNow;
        link.UpdatedUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> TrainerRespondToRequest(Guid id, WithinDbContext db, ClaimsPrincipal principal, bool accept)
    {
        var trainerId = principal.UserId();
        var link = await db.TrainerClients.FirstOrDefaultAsync(t => t.Id == id && t.TrainerUserId == trainerId);
        if (link is null) return Results.NotFound();
        if (link.Status != TrainerClientStatus.Pending) return Results.BadRequest(new { message = "This request has already been handled." });
        link.Status = accept ? TrainerClientStatus.Active : TrainerClientStatus.Rejected;
        if (accept) link.StartedAt = DateTimeOffset.UtcNow;
        link.UpdatedUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        // TODO(move-notify): emit TrainerClientAccepted to the client on accept.
        return Results.Ok(new { status = link.Status.ToString() });
    }

    // ── Cloning ──
    private static async Task<WorkoutPlan> CloneWorkoutTemplate(WithinDbContext db, WorkoutPlan template, Guid clientUserId,
        MovePlanSourceType source, Guid createdByUserId, Guid? trainerId, bool activate)
    {
        var now = DateTimeOffset.UtcNow;
        var status = activate ? MovePlanStatus.Active : MovePlanStatus.Draft;
        if (activate)
        {
            await db.WorkoutPlans.Where(p => p.AssignedToUserId == clientUserId && !p.IsTemplate && p.Status == MovePlanStatus.Active)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, MovePlanStatus.Paused));
        }

        var plan = new WorkoutPlan
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = createdByUserId,
            AssignedToUserId = clientUserId,
            TrainerId = trainerId,
            Title = template.Title,
            Description = template.Description,
            Goal = template.Goal,
            WorkoutType = template.WorkoutType,
            ExperienceLevel = template.ExperienceLevel,
            DurationWeeks = template.DurationWeeks,
            DaysPerWeek = template.DaysPerWeek,
            IsTemplate = false,
            SourceType = source,
            Status = status,
            StartDate = activate ? DateOnly.FromDateTime(DateTime.UtcNow) : null,
            CreatedUtc = now,
            UpdatedUtc = now
        };
        db.WorkoutPlans.Add(plan);

        var days = await db.WorkoutDays.Where(d => d.WorkoutPlanId == template.Id).OrderBy(d => d.SortOrder).ToArrayAsync();
        var dayIds = days.Select(d => d.Id).ToArray();
        var exercises = await db.WorkoutExercises.Where(e => dayIds.Contains(e.WorkoutDayId)).ToArrayAsync();
        foreach (var day in days)
        {
            var newDay = new WorkoutDay
            {
                Id = Guid.NewGuid(),
                WorkoutPlanId = plan.Id,
                DayNumber = day.DayNumber,
                Title = day.Title,
                Description = day.Description,
                SortOrder = day.SortOrder
            };
            db.WorkoutDays.Add(newDay);
            foreach (var ex in exercises.Where(e => e.WorkoutDayId == day.Id).OrderBy(e => e.SortOrder))
            {
                db.WorkoutExercises.Add(new WorkoutExercise
                {
                    Id = Guid.NewGuid(),
                    WorkoutDayId = newDay.Id,
                    ExerciseName = ex.ExerciseName,
                    Sets = ex.Sets,
                    Reps = ex.Reps,
                    Weight = ex.Weight,
                    RestSeconds = ex.RestSeconds,
                    Tempo = ex.Tempo,
                    Notes = ex.Notes,
                    SortOrder = ex.SortOrder
                });
            }
        }
        return plan;
    }

    private static async Task<DietPlan> CloneDietTemplate(WithinDbContext db, DietPlan template, Guid clientUserId,
        MovePlanSourceType source, Guid createdByUserId, Guid? trainerId, bool activate)
    {
        var now = DateTimeOffset.UtcNow;
        var status = activate ? MovePlanStatus.Active : MovePlanStatus.Draft;
        if (activate)
        {
            await db.DietPlans.Where(p => p.AssignedToUserId == clientUserId && !p.IsTemplate && p.Status == MovePlanStatus.Active)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, MovePlanStatus.Paused));
        }

        var plan = new DietPlan
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = createdByUserId,
            AssignedToUserId = clientUserId,
            TrainerId = trainerId,
            Title = template.Title,
            Description = template.Description,
            Goal = template.Goal,
            Calories = template.Calories,
            ProteinGrams = template.ProteinGrams,
            CarbsGrams = template.CarbsGrams,
            FatGrams = template.FatGrams,
            DietPreference = template.DietPreference,
            DurationWeeks = template.DurationWeeks,
            IsTemplate = false,
            SourceType = source,
            Status = status,
            StartDate = activate ? DateOnly.FromDateTime(DateTime.UtcNow) : null,
            CreatedUtc = now,
            UpdatedUtc = now
        };
        db.DietPlans.Add(plan);

        var meals = await db.DietMeals.Where(m => m.DietPlanId == template.Id).OrderBy(m => m.SortOrder).ToArrayAsync();
        var mealIds = meals.Select(m => m.Id).ToArray();
        var items = await db.DietMealItems.Where(i => mealIds.Contains(i.DietMealId)).ToArrayAsync();
        foreach (var meal in meals)
        {
            var newMeal = new DietMeal
            {
                Id = Guid.NewGuid(),
                DietPlanId = plan.Id,
                MealName = meal.MealName,
                MealTime = meal.MealTime,
                Calories = meal.Calories,
                ProteinGrams = meal.ProteinGrams,
                CarbsGrams = meal.CarbsGrams,
                FatGrams = meal.FatGrams,
                SortOrder = meal.SortOrder
            };
            db.DietMeals.Add(newMeal);
            foreach (var item in items.Where(i => i.DietMealId == meal.Id))
            {
                db.DietMealItems.Add(new DietMealItem
                {
                    Id = Guid.NewGuid(),
                    DietMealId = newMeal.Id,
                    FoodName = item.FoodName,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    Calories = item.Calories,
                    ProteinGrams = item.ProteinGrams,
                    CarbsGrams = item.CarbsGrams,
                    FatGrams = item.FatGrams,
                    SubstitutionNotes = item.SubstitutionNotes
                });
            }
        }
        return plan;
    }

    // ── DTO mapping ──
    private const string Disclaimer = "These are estimates only and not medical advice.";

    private static MoveProfileDto ToProfileDto(MoveProfile p) => new(
        p.Id.ToString(), p.HeightCm, p.WeightKg, p.DateOfBirth?.ToString("yyyy-MM-dd"), p.Age,
        p.GenderForCalculation, p.ActivityLevel, p.FitnessGoal, p.ExperienceLevel, p.PreferredWorkoutType,
        p.AvailableDaysPerWeek, p.SessionDurationMinutes, p.EquipmentAvailable, p.InjuriesOrLimitations,
        p.DietPreference, p.FoodAllergies, p.UpdatedUtc.ToString("O"));

    private static BodyMetricDto ToMetricDto(BodyMetricLog m) => new(
        m.Id.ToString(), m.WeightKg, m.BodyFatPercentage, m.WaistCm, m.Bmi, m.LoggedAt.ToString("O"));

    private static WorkoutLogDto ToWorkoutLogDto(WorkoutLog l) => new(
        l.Id.ToString(), l.LogDate.ToString("yyyy-MM-dd"), l.Title ?? "Workout", l.DurationMinutes, l.Notes);

    private static async Task<WorkoutPlanDto> LoadWorkoutDto(WithinDbContext db, WorkoutPlan plan)
    {
        var days = await db.WorkoutDays.Where(d => d.WorkoutPlanId == plan.Id).OrderBy(d => d.SortOrder).ToArrayAsync();
        var dayIds = days.Select(d => d.Id).ToArray();
        var exercises = await db.WorkoutExercises.Where(e => dayIds.Contains(e.WorkoutDayId)).OrderBy(e => e.SortOrder).ToArrayAsync();
        var dayDtos = days.Select(d => new WorkoutDayDto(
            d.Id.ToString(), d.Title, d.Description,
            exercises.Where(e => e.WorkoutDayId == d.Id)
                .Select(e => new WorkoutExerciseDto(e.ExerciseName, e.Sets, e.Reps, e.Weight, e.RestSeconds, e.Notes)).ToArray())).ToArray();
        return new WorkoutPlanDto(
            plan.Id.ToString(), plan.Title, plan.WorkoutType, SourceToken(plan.SourceType),
            plan.DaysPerWeek, plan.Status == MovePlanStatus.Active, dayDtos, plan.CreatedUtc.ToString("O"));
    }

    private static async Task<DietPlanDto> LoadDietDto(WithinDbContext db, DietPlan plan, Guid userId)
    {
        var meals = await db.DietMeals.Where(m => m.DietPlanId == plan.Id).OrderBy(m => m.SortOrder).ToArrayAsync();
        var mealIds = meals.Select(m => m.Id).ToArray();
        var items = await db.DietMealItems.Where(i => mealIds.Contains(i.DietMealId)).ToArrayAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todaysLogs = await db.DietLogs.Where(l => l.UserId == userId && l.DietPlanId == plan.Id && l.LogDate == today)
            .ToDictionaryAsync(l => l.DietMealId, l => l.Status);

        var mealDtos = meals.Select(m => new DietMealDto(
            m.Id.ToString(), m.MealName, m.Calories,
            items.Where(i => i.DietMealId == m.Id)
                .Select(i => new DietFoodItemDto(i.FoodName, FormatQty(i), i.SubstitutionNotes)).ToArray(),
            todaysLogs.TryGetValue(m.Id, out var st) ? DietStatusToken(st) : null,
            null)).ToArray();

        var weekStart = today.AddDays(-6);
        var adherence = await WeeklyDietAdherence(db, userId, weekStart, plan.Id);
        return new DietPlanDto(
            plan.Id.ToString(), plan.Title, SourceToken(plan.SourceType), plan.Status == MovePlanStatus.Active,
            plan.Calories, plan.ProteinGrams, plan.CarbsGrams, plan.FatGrams, mealDtos, adherence, plan.CreatedUtc.ToString("O"));
    }

    private static async Task<ChallengeDto> ToChallengeDto(WithinDbContext db, Challenge c, Guid userId)
    {
        var participantCount = await db.ChallengeParticipants.CountAsync(p => p.ChallengeId == c.Id && p.Status != ChallengeParticipantStatus.Dropped);
        var participant = await db.ChallengeParticipants.FirstOrDefaultAsync(p => p.ChallengeId == c.Id && p.UserId == userId && p.Status != ChallengeParticipantStatus.Dropped);
        int? progressDays = participant is null ? null
            : await db.ChallengeProgresses.Where(p => p.ChallengeId == c.Id && p.UserId == userId && p.Status == ChallengeProgressStatus.Completed)
                .Select(p => p.LogDate).Distinct().CountAsync();
        string? circleName = c.CircleId is Guid cid
            ? await db.Circles.Where(x => x.Id == cid).Select(x => x.Name).FirstOrDefaultAsync()
            : null;
        var source = c.CircleId is not null ? "circle" : c.TrainerId is not null ? "trainer" : "platform";
        return new ChallengeDto(
            c.Id.ToString(), c.Title, c.Description, c.DurationDays, participantCount, source, circleName,
            participant is not null, progressDays, participant is null ? null : PrivacyToken(participant.DisplayMode));
    }

    private static TrainerPermissionsDto ToPermissionsDto(TrainerClient? link) => new(
        link?.ShareMoveProfileWithTrainer ?? false,
        link?.ShareBodyMetricsWithTrainer ?? false,
        link?.ShareWorkoutLogsWithTrainer ?? false,
        link?.ShareDietLogsWithTrainer ?? false,
        link?.ShareChallengeProgressWithTrainer ?? false);

    // ── Small helpers ──
    private static async Task<bool> ChallengeVisibleTo(WithinDbContext db, Challenge c, Guid userId)
    {
        if (c.Visibility == ChallengeVisibility.Public) return true;
        if (c.CreatedByUserId == userId) return true;
        if (await db.ChallengeParticipants.AnyAsync(p => p.ChallengeId == c.Id && p.UserId == userId)) return true;
        if (c.Visibility == ChallengeVisibility.CircleOnly && c.CircleId is Guid cid)
            return await db.CircleMembers.AnyAsync(m => m.CircleId == cid && m.UserId == userId && m.Status == CircleMemberStatus.Active);
        return false;
    }

    private static async Task<int> WeeklyDietAdherence(WithinDbContext db, Guid userId, DateOnly weekStart, Guid? planId = null)
    {
        var query = db.DietLogs.Where(l => l.UserId == userId && l.LogDate >= weekStart);
        if (planId is Guid p) query = query.Where(l => l.DietPlanId == p);
        var logs = await query.Select(l => l.Status).ToArrayAsync();
        if (logs.Length == 0) return 0;
        var score = logs.Sum(s => s switch { DietLogStatus.Followed => 1.0, DietLogStatus.PartiallyFollowed => 0.5, _ => 0.0 });
        return (int)Math.Round(score / logs.Length * 100);
    }

    private static bool IsProvider(ClaimsPrincipal principal) =>
        principal.IsInRole(nameof(WithinRole.Provider)) || principal.IsInRole(nameof(WithinRole.Admin));

    private static async Task<string> UserName(WithinDbContext db, Guid userId) =>
        await db.Users.Where(u => u.Id == userId).Select(u => u.DisplayName).FirstOrDefaultAsync() ?? "Within member";

    private static string SourceToken(MovePlanSourceType source) => source == MovePlanSourceType.TrainerAssigned ? "trainer" : "generated";

    private static string DietStatusToken(DietLogStatus status) => status switch
    {
        DietLogStatus.Followed => "followed",
        DietLogStatus.PartiallyFollowed => "partial",
        _ => "missed"
    };

    private static DietLogStatus DietStatusFromToken(string? token) => (token ?? "").Trim().ToLowerInvariant() switch
    {
        "followed" => DietLogStatus.Followed,
        "partial" or "partially_followed" => DietLogStatus.PartiallyFollowed,
        _ => DietLogStatus.Missed
    };

    private static ChallengeDisplayMode DisplayModeFromPrivacy(string? privacy) => (privacy ?? "").Trim().ToLowerInvariant() switch
    {
        "friends" => ChallengeDisplayMode.FriendsOnly,
        "anonymous" => ChallengeDisplayMode.Anonymous,
        "private" => ChallengeDisplayMode.Private,
        _ => ChallengeDisplayMode.PublicName
    };

    private static string PrivacyToken(ChallengeDisplayMode mode) => mode switch
    {
        ChallengeDisplayMode.FriendsOnly => "friends",
        ChallengeDisplayMode.Anonymous => "anonymous",
        ChallengeDisplayMode.Private => "private",
        _ => "public"
    };

    private static string? FormatQty(DietMealItem item) =>
        string.IsNullOrWhiteSpace(item.Unit) ? item.Quantity : $"{item.Quantity} {item.Unit}".Trim();

    private static string? Lower(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static DateOnly? ParseDate(string? value) => DateOnly.TryParse(value, out var d) ? d : null;
    private static int AgeFrom(DateOnly dob)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age)) age--;
        return age;
    }
}
