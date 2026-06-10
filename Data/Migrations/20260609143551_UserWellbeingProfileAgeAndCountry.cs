using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserWellbeingProfileAgeAndCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                schema: "within",
                table: "UserWellbeingProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationCountry",
                schema: "within",
                table: "UserWellbeingProfiles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                schema: "within",
                table: "UserWellbeingProfiles");

            migrationBuilder.DropColumn(
                name: "LocationCountry",
                schema: "within",
                table: "UserWellbeingProfiles");
        }
    }
}
