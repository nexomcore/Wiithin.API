using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class JoinableWithinCircles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:circle_event_status", "active,removed")
                .Annotation("Npgsql:Enum:circle_member_status", "active,left,removed")
                .Annotation("Npgsql:Enum:circle_role_kind", "moderator,admin")
                .Annotation("Npgsql:Enum:circle_status", "active,archived")
                .Annotation("Npgsql:Enum:circle_type", "platform,provider,event_cohort,private_support")
                .Annotation("Npgsql:Enum:circle_visibility", "public,private,hidden")
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

            migrationBuilder.CreateTable(
                name: "CircleEvents",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CircleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    SharedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionalNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CircleGuidelines",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CircleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleGuidelines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CircleHelpfulReactions",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleHelpfulReactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CircleMembers",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CircleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CircleReports",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CircleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CircleEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CircleRoles",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CircleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Circles",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Lens = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Circles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CircleThreadComments",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleThreadComments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CircleThreads",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CircleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadType = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    Body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    LinkedEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleThreads", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CircleEvents_CircleId_EventId",
                schema: "within",
                table: "CircleEvents",
                columns: new[] { "CircleId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CircleEvents_EventId",
                schema: "within",
                table: "CircleEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleGuidelines_CircleId_SortOrder",
                schema: "within",
                table: "CircleGuidelines",
                columns: new[] { "CircleId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CircleHelpfulReactions_CommentId_UserId",
                schema: "within",
                table: "CircleHelpfulReactions",
                columns: new[] { "CommentId", "UserId" },
                unique: true,
                filter: "\"CommentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CircleHelpfulReactions_ThreadId_UserId",
                schema: "within",
                table: "CircleHelpfulReactions",
                columns: new[] { "ThreadId", "UserId" },
                unique: true,
                filter: "\"ThreadId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CircleMembers_CircleId_UserId",
                schema: "within",
                table: "CircleMembers",
                columns: new[] { "CircleId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CircleMembers_UserId_Status",
                schema: "within",
                table: "CircleMembers",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CircleReports_CircleEventId",
                schema: "within",
                table: "CircleReports",
                column: "CircleEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleReports_CircleId",
                schema: "within",
                table: "CircleReports",
                column: "CircleId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleReports_CommentId",
                schema: "within",
                table: "CircleReports",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleReports_Status_CreatedAt",
                schema: "within",
                table: "CircleReports",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CircleReports_ThreadId",
                schema: "within",
                table: "CircleReports",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleRoles_CircleId_UserId_Role",
                schema: "within",
                table: "CircleRoles",
                columns: new[] { "CircleId", "UserId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CircleRoles_UserId",
                schema: "within",
                table: "CircleRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Circles_Slug",
                schema: "within",
                table: "Circles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Circles_Visibility_Status",
                schema: "within",
                table: "Circles",
                columns: new[] { "Visibility", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CircleThreadComments_ThreadId_CreatedAt",
                schema: "within",
                table: "CircleThreadComments",
                columns: new[] { "ThreadId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CircleThreadComments_UserId",
                schema: "within",
                table: "CircleThreadComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleThreads_CircleId_Status_CreatedAt",
                schema: "within",
                table: "CircleThreads",
                columns: new[] { "CircleId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CircleThreads_LinkedEventId",
                schema: "within",
                table: "CircleThreads",
                column: "LinkedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleThreads_UserId",
                schema: "within",
                table: "CircleThreads",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CircleEvents",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CircleGuidelines",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CircleHelpfulReactions",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CircleMembers",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CircleReports",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CircleRoles",
                schema: "within");

            migrationBuilder.DropTable(
                name: "Circles",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CircleThreadComments",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CircleThreads",
                schema: "within");

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
                .OldAnnotation("Npgsql:Enum:circle_event_status", "active,removed")
                .OldAnnotation("Npgsql:Enum:circle_member_status", "active,left,removed")
                .OldAnnotation("Npgsql:Enum:circle_role_kind", "moderator,admin")
                .OldAnnotation("Npgsql:Enum:circle_status", "active,archived")
                .OldAnnotation("Npgsql:Enum:circle_type", "platform,provider,event_cohort,private_support")
                .OldAnnotation("Npgsql:Enum:circle_visibility", "public,private,hidden")
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
