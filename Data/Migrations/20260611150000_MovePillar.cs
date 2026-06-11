using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class MovePillar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BodyMetricLogs",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(6,1)", nullable: false),
                    BodyFatPercentage = table.Column<decimal>(type: "numeric(6,1)", nullable: true),
                    WaistCm = table.Column<decimal>(type: "numeric(6,1)", nullable: true),
                    HipCm = table.Column<decimal>(type: "numeric(6,1)", nullable: true),
                    ChestCm = table.Column<decimal>(type: "numeric(6,1)", nullable: true),
                    ArmCm = table.Column<decimal>(type: "numeric(6,1)", nullable: true),
                    ThighCm = table.Column<decimal>(type: "numeric(6,1)", nullable: true),
                    Bmi = table.Column<decimal>(type: "numeric(6,1)", nullable: true),
                    Bmr = table.Column<decimal>(type: "numeric(6,1)", nullable: true),
                    Tdee = table.Column<decimal>(type: "numeric(6,1)", nullable: true),
                    LoggedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodyMetricLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalculatorResults",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalculatorType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InputJson = table.Column<string>(type: "jsonb", nullable: false),
                    ResultJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculatorResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeParticipants",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeParticipants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeProgresses",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ValueCompleted = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    ProofImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LogDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Challenges",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CircleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ChallengeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Visibility = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsTemplate = table.Column<bool>(type: "boolean", nullable: false),
                    AllowAnonymousParticipation = table.Column<bool>(type: "boolean", nullable: false),
                    LeaderboardEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeTasks",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TargetValue = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    TargetUnit = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DietLogs",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DietPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    DietMealId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LogDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LoggedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DietMealItems",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DietMealId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Quantity = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Unit = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Calories = table.Column<int>(type: "integer", nullable: true),
                    ProteinGrams = table.Column<int>(type: "integer", nullable: true),
                    CarbsGrams = table.Column<int>(type: "integer", nullable: true),
                    FatGrams = table.Column<int>(type: "integer", nullable: true),
                    SubstitutionNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietMealItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DietMeals",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DietPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    MealName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    MealTime = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Calories = table.Column<int>(type: "integer", nullable: false),
                    ProteinGrams = table.Column<int>(type: "integer", nullable: false),
                    CarbsGrams = table.Column<int>(type: "integer", nullable: false),
                    FatGrams = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietMeals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DietPlans",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Goal = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Calories = table.Column<int>(type: "integer", nullable: false),
                    ProteinGrams = table.Column<int>(type: "integer", nullable: false),
                    CarbsGrams = table.Column<int>(type: "integer", nullable: false),
                    FatGrams = table.Column<int>(type: "integer", nullable: false),
                    DietPreference = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DurationWeeks = table.Column<int>(type: "integer", nullable: false),
                    IsTemplate = table.Column<bool>(type: "boolean", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MoveProfiles",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeightCm = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Age = table.Column<int>(type: "integer", nullable: true),
                    GenderForCalculation = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ActivityLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FitnessGoal = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ExperienceLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    PreferredWorkoutType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    AvailableDaysPerWeek = table.Column<int>(type: "integer", nullable: true),
                    SessionDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    EquipmentAvailable = table.Column<string[]>(type: "text[]", nullable: false, defaultValue: new string[0]),
                    InjuriesOrLimitations = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DietPreference = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FoodAllergies = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoveProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainerClients",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ShareMoveProfileWithTrainer = table.Column<bool>(type: "boolean", nullable: false),
                    ShareBodyMetricsWithTrainer = table.Column<bool>(type: "boolean", nullable: false),
                    ShareWorkoutLogsWithTrainer = table.Column<bool>(type: "boolean", nullable: false),
                    ShareDietLogsWithTrainer = table.Column<bool>(type: "boolean", nullable: false),
                    ShareChallengeProgressWithTrainer = table.Column<bool>(type: "boolean", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerClients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainerNotes",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Visibility = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerNotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutDays",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutDays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutExerciseLogs",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualSets = table.Column<int>(type: "integer", nullable: true),
                    ActualReps = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ActualWeight = table.Column<decimal>(type: "numeric(6,1)", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutExerciseLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutExercises",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutDayId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Sets = table.Column<int>(type: "integer", nullable: true),
                    Reps = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(6,1)", nullable: true),
                    RestSeconds = table.Column<int>(type: "integer", nullable: true),
                    Tempo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutExercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutLogs",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkoutDayId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LogDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutPlans",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Goal = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    WorkoutType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ExperienceLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DurationWeeks = table.Column<int>(type: "integer", nullable: false),
                    DaysPerWeek = table.Column<int>(type: "integer", nullable: false),
                    IsTemplate = table.Column<bool>(type: "boolean", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutPlans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BodyMetricLogs_UserId_LoggedAt",
                schema: "within",
                table: "BodyMetricLogs",
                columns: new[] { "UserId", "LoggedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CalculatorResults_UserId_CreatedUtc",
                schema: "within",
                table: "CalculatorResults",
                columns: new[] { "UserId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_ChallengeId_UserId",
                schema: "within",
                table: "ChallengeParticipants",
                columns: new[] { "ChallengeId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_UserId_Status",
                schema: "within",
                table: "ChallengeParticipants",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeProgresses_ChallengeId_UserId_LogDate",
                schema: "within",
                table: "ChallengeProgresses",
                columns: new[] { "ChallengeId", "UserId", "LogDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_CircleId",
                schema: "within",
                table: "Challenges",
                column: "CircleId");

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_Visibility_IsTemplate",
                schema: "within",
                table: "Challenges",
                columns: new[] { "Visibility", "IsTemplate" });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeTasks_ChallengeId_SortOrder",
                schema: "within",
                table: "ChallengeTasks",
                columns: new[] { "ChallengeId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_DietLogs_UserId_DietMealId_LogDate",
                schema: "within",
                table: "DietLogs",
                columns: new[] { "UserId", "DietMealId", "LogDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DietLogs_UserId_LogDate",
                schema: "within",
                table: "DietLogs",
                columns: new[] { "UserId", "LogDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DietMealItems_DietMealId",
                schema: "within",
                table: "DietMealItems",
                column: "DietMealId");

            migrationBuilder.CreateIndex(
                name: "IX_DietMeals_DietPlanId_SortOrder",
                schema: "within",
                table: "DietMeals",
                columns: new[] { "DietPlanId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_DietPlans_AssignedToUserId_Status",
                schema: "within",
                table: "DietPlans",
                columns: new[] { "AssignedToUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DietPlans_IsTemplate_DietPreference",
                schema: "within",
                table: "DietPlans",
                columns: new[] { "IsTemplate", "DietPreference" });

            migrationBuilder.CreateIndex(
                name: "IX_MoveProfiles_UserId",
                schema: "within",
                table: "MoveProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainerClients_ClientUserId_Status",
                schema: "within",
                table: "TrainerClients",
                columns: new[] { "ClientUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainerClients_TrainerUserId_ClientUserId",
                schema: "within",
                table: "TrainerClients",
                columns: new[] { "TrainerUserId", "ClientUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainerClients_TrainerUserId_Status",
                schema: "within",
                table: "TrainerClients",
                columns: new[] { "TrainerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainerNotes_ClientUserId_CreatedUtc",
                schema: "within",
                table: "TrainerNotes",
                columns: new[] { "ClientUserId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainerNotes_TrainerUserId_ClientUserId",
                schema: "within",
                table: "TrainerNotes",
                columns: new[] { "TrainerUserId", "ClientUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutDays_WorkoutPlanId_SortOrder",
                schema: "within",
                table: "WorkoutDays",
                columns: new[] { "WorkoutPlanId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutExerciseLogs_WorkoutLogId",
                schema: "within",
                table: "WorkoutExerciseLogs",
                column: "WorkoutLogId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutExercises_WorkoutDayId_SortOrder",
                schema: "within",
                table: "WorkoutExercises",
                columns: new[] { "WorkoutDayId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_UserId_LogDate",
                schema: "within",
                table: "WorkoutLogs",
                columns: new[] { "UserId", "LogDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_UserId_WorkoutDayId_LogDate",
                schema: "within",
                table: "WorkoutLogs",
                columns: new[] { "UserId", "WorkoutDayId", "LogDate" },
                unique: true,
                filter: "\"WorkoutDayId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_AssignedToUserId_Status",
                schema: "within",
                table: "WorkoutPlans",
                columns: new[] { "AssignedToUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_IsTemplate_WorkoutType",
                schema: "within",
                table: "WorkoutPlans",
                columns: new[] { "IsTemplate", "WorkoutType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BodyMetricLogs",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CalculatorResults",
                schema: "within");

            migrationBuilder.DropTable(
                name: "ChallengeParticipants",
                schema: "within");

            migrationBuilder.DropTable(
                name: "ChallengeProgresses",
                schema: "within");

            migrationBuilder.DropTable(
                name: "Challenges",
                schema: "within");

            migrationBuilder.DropTable(
                name: "ChallengeTasks",
                schema: "within");

            migrationBuilder.DropTable(
                name: "DietLogs",
                schema: "within");

            migrationBuilder.DropTable(
                name: "DietMealItems",
                schema: "within");

            migrationBuilder.DropTable(
                name: "DietMeals",
                schema: "within");

            migrationBuilder.DropTable(
                name: "DietPlans",
                schema: "within");

            migrationBuilder.DropTable(
                name: "MoveProfiles",
                schema: "within");

            migrationBuilder.DropTable(
                name: "TrainerClients",
                schema: "within");

            migrationBuilder.DropTable(
                name: "TrainerNotes",
                schema: "within");

            migrationBuilder.DropTable(
                name: "WorkoutDays",
                schema: "within");

            migrationBuilder.DropTable(
                name: "WorkoutExerciseLogs",
                schema: "within");

            migrationBuilder.DropTable(
                name: "WorkoutExercises",
                schema: "within");

            migrationBuilder.DropTable(
                name: "WorkoutLogs",
                schema: "within");

            migrationBuilder.DropTable(
                name: "WorkoutPlans",
                schema: "within");
        }
    }
}
