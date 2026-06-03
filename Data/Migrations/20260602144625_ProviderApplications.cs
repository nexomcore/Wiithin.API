using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProviderApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:event_join_state", "interested,going,attended")
                .Annotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .Annotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .Annotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .Annotation("Npgsql:Enum:within_role", "user,provider,admin")
                .OldAnnotation("Npgsql:Enum:event_join_state", "interested,going,attended")
                .OldAnnotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .OldAnnotation("Npgsql:Enum:signup_type", "internal,external")
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin");

            migrationBuilder.CreateTable(
                name: "ProviderApplications",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProviderCategory = table.Column<int>(type: "integer", nullable: false),
                    PrimaryLens = table.Column<int>(type: "integer", nullable: false),
                    ServiceAreas = table.Column<string[]>(type: "text[]", nullable: false),
                    ContactName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PreferredContactMethod = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    BusinessType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Abn = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "text", nullable: true),
                    InstagramUrl = table.Column<string>(type: "text", nullable: true),
                    OtherSocialUrl = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    DeliveryModes = table.Column<string[]>(type: "text[]", nullable: false),
                    VenueNames = table.Column<string>(type: "text", nullable: true),
                    ServicesOffered = table.Column<string[]>(type: "text[]", nullable: false),
                    YearsPracticing = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    TypicalAudience = table.Column<string>(type: "text", nullable: false),
                    Bio = table.Column<string>(type: "text", nullable: false),
                    JoinReason = table.Column<string>(type: "text", nullable: false),
                    Certifications = table.Column<string>(type: "text", nullable: false),
                    InsuranceStatus = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    WorkingWithChildrenCheck = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    FirstAidCpr = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ProfessionalMemberships = table.Column<string>(type: "text", nullable: true),
                    CredentialLinks = table.Column<string>(type: "text", nullable: true),
                    HasEventsReady = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ExpectedFirstEvent = table.Column<string>(type: "text", nullable: false),
                    BookingTools = table.Column<string>(type: "text", nullable: false),
                    AdminFacingNotes = table.Column<string>(type: "text", nullable: true),
                    DeclarationAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    AdminNotes = table.Column<string>(type: "text", nullable: false),
                    ReviewDecisionReason = table.Column<string>(type: "text", nullable: false),
                    SubmittedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedProviderId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderApplications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderApplications_ContactEmail",
                schema: "within",
                table: "ProviderApplications",
                column: "ContactEmail");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderApplications_Status_SubmittedUtc",
                schema: "within",
                table: "ProviderApplications",
                columns: new[] { "Status", "SubmittedUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderApplications",
                schema: "within");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:event_join_state", "interested,going,attended")
                .Annotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .Annotation("Npgsql:Enum:within_role", "user,provider,admin")
                .OldAnnotation("Npgsql:Enum:event_join_state", "interested,going,attended")
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
