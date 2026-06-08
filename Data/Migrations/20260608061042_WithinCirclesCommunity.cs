using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class WithinCirclesCommunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:community_content_status", "active,hidden,removed,under_review")
                .Annotation("Npgsql:Enum:community_post_type", "ask_community,share_experience,find_buddy,local_recommendation,reflection")
                .Annotation("Npgsql:Enum:community_report_reason", "spam_or_promotion,harassment_or_abuse,medical_misinformation,inappropriate_content,safety_concern,other")
                .Annotation("Npgsql:Enum:community_report_status", "pending,reviewed,action_taken,dismissed")
                .Annotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .Annotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .Annotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .Annotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .Annotation("Npgsql:Enum:within_role", "user,provider,admin")
                .OldAnnotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .OldAnnotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .OldAnnotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .OldAnnotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .OldAnnotation("Npgsql:Enum:signup_type", "internal,external")
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin");

            migrationBuilder.CreateTable(
                name: "CommunityComments",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostType = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Body = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    LinkedEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .Annotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .Annotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .Annotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .Annotation("Npgsql:Enum:within_role", "user,provider,admin")
                .OldAnnotation("Npgsql:Enum:community_content_status", "active,hidden,removed,under_review")
                .OldAnnotation("Npgsql:Enum:community_post_type", "ask_community,share_experience,find_buddy,local_recommendation,reflection")
                .OldAnnotation("Npgsql:Enum:community_report_reason", "spam_or_promotion,harassment_or_abuse,medical_misinformation,inappropriate_content,safety_concern,other")
                .OldAnnotation("Npgsql:Enum:community_report_status", "pending,reviewed,action_taken,dismissed")
                .OldAnnotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .OldAnnotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .OldAnnotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .OldAnnotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .OldAnnotation("Npgsql:Enum:signup_type", "internal,external")
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin");
        }
    }
}
