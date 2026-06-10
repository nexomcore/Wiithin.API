using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCommunityPostsAndUserSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityComments",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CommunityHelpfulReactions",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CommunityPosts",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CommunityPostTopics",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CommunityReports",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CommunityTopics",
                schema: "within");

            migrationBuilder.DropTable(
                name: "SavedCommunityPosts",
                schema: "within");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedUtc",
                schema: "within",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "within",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedUtc",
                schema: "within",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "within",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "CommunityComments",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityComments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunityHelpfulReactions",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityHelpfulReactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunityPosts",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LinkedEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunityPostTopics",
                schema: "within",
                columns: table => new
                {
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityPostTopics", x => new { x.PostId, x.TopicId });
                });

            migrationBuilder.CreateTable(
                name: "CommunityReports",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PostId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunityTopics",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityTopics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedCommunityPosts",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedCommunityPosts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityComments_PostId_CreatedAt",
                schema: "within",
                table: "CommunityComments",
                columns: new[] { "PostId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityComments_UserId",
                schema: "within",
                table: "CommunityComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityHelpfulReactions_CommentId_UserId",
                schema: "within",
                table: "CommunityHelpfulReactions",
                columns: new[] { "CommentId", "UserId" },
                unique: true,
                filter: "\"CommentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityHelpfulReactions_PostId_UserId",
                schema: "within",
                table: "CommunityHelpfulReactions",
                columns: new[] { "PostId", "UserId" },
                unique: true,
                filter: "\"PostId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_LinkedEventId",
                schema: "within",
                table: "CommunityPosts",
                column: "LinkedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_Status_CreatedAt",
                schema: "within",
                table: "CommunityPosts",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_UserId",
                schema: "within",
                table: "CommunityPosts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPostTopics_TopicId",
                schema: "within",
                table: "CommunityPostTopics",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityReports_CommentId",
                schema: "within",
                table: "CommunityReports",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityReports_PostId",
                schema: "within",
                table: "CommunityReports",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityReports_Status_CreatedAt",
                schema: "within",
                table: "CommunityReports",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityTopics_Slug",
                schema: "within",
                table: "CommunityTopics",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedCommunityPosts_PostId_UserId",
                schema: "within",
                table: "SavedCommunityPosts",
                columns: new[] { "PostId", "UserId" },
                unique: true);
        }
    }
}
