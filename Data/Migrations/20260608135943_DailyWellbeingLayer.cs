using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class DailyWellbeingLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionScore",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "EnergyScore",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "MeaningScore",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "MoodScore",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "StressScore",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                schema: "within",
                table: "DailyCheckIns",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                schema: "within",
                table: "DailyCheckIns",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Energy",
                schema: "within",
                table: "DailyCheckIns",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Intention",
                schema: "within",
                table: "DailyCheckIns",
                type: "character varying(48)",
                maxLength: 48,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Mood",
                schema: "within",
                table: "DailyCheckIns",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SleepHours",
                schema: "within",
                table: "DailyCheckIns",
                type: "numeric(4,1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SleepQuality",
                schema: "within",
                table: "DailyCheckIns",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuggestedActionKey",
                schema: "within",
                table: "DailyCheckIns",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                schema: "within",
                table: "DailyCheckIns",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateTable(
                name: "HabitCompletions",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserHabitId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitCompletions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HabitTemplates",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    IconKey = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserHabits",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HabitTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    IsCustom = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHabits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HabitCompletions_UserHabitId_CompletionDate",
                schema: "within",
                table: "HabitCompletions",
                columns: new[] { "UserHabitId", "CompletionDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HabitCompletions_UserId_CompletionDate",
                schema: "within",
                table: "HabitCompletions",
                columns: new[] { "UserId", "CompletionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HabitTemplates_Name",
                schema: "within",
                table: "HabitTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserHabits_UserId_IsActive",
                schema: "within",
                table: "UserHabits",
                columns: new[] { "UserId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HabitCompletions",
                schema: "within");

            migrationBuilder.DropTable(
                name: "HabitTemplates",
                schema: "within");

            migrationBuilder.DropTable(
                name: "UserHabits",
                schema: "within");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "Energy",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "Intention",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "Mood",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "SleepHours",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "SleepQuality",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "SuggestedActionKey",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                schema: "within",
                table: "DailyCheckIns",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConnectionScore",
                schema: "within",
                table: "DailyCheckIns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EnergyScore",
                schema: "within",
                table: "DailyCheckIns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MeaningScore",
                schema: "within",
                table: "DailyCheckIns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MoodScore",
                schema: "within",
                table: "DailyCheckIns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StressScore",
                schema: "within",
                table: "DailyCheckIns",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
