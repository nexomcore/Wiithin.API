using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class UnifiedProviderProfilesAndServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:circle_event_status", "active,removed")
                .Annotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .Annotation("Npgsql:Enum:circle_invite_status", "pending,accepted,declined,cancelled")
                .Annotation("Npgsql:Enum:circle_join_request_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:circle_member_role", "admin,moderator,member")
                .Annotation("Npgsql:Enum:circle_member_status", "active,left,removed,pending,rejected,blocked")
                .Annotation("Npgsql:Enum:circle_post_type", "standard,system,announcement,event_share,weekly_check_in,poll")
                .Annotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .Annotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
                .Annotation("Npgsql:Enum:circle_reaction_type", "support,grateful,inspired,motivated,growing")
                .Annotation("Npgsql:Enum:circle_role_kind", "moderator,admin")
                .Annotation("Npgsql:Enum:circle_status", "active,archived")
                .Annotation("Npgsql:Enum:circle_type", "platform,provider,event_cohort,private_support")
                .Annotation("Npgsql:Enum:circle_visibility", "public,private,hidden")
                .Annotation("Npgsql:Enum:community_content_status", "active,hidden,removed,under_review")
                .Annotation("Npgsql:Enum:community_post_type", "ask_community,share_experience,find_buddy,local_recommendation,reflection")
                .Annotation("Npgsql:Enum:community_report_reason", "spam_or_promotion,harassment_or_abuse,medical_misinformation,inappropriate_content,safety_concern,other")
                .Annotation("Npgsql:Enum:community_report_status", "pending,reviewed,action_taken,dismissed")
                .Annotation("Npgsql:Enum:connection_status", "pending,accepted,rejected,cancelled,removed,blocked")
                .Annotation("Npgsql:Enum:event_invite_status", "pending,accepted,declined,cancelled")
                .Annotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .Annotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .Annotation("Npgsql:Enum:friend_request_permission", "everyone,friends_of_friends,same_circle_or_event,no_one")
                .Annotation("Npgsql:Enum:member_list_visibility", "public,members_only,admins_only,hidden")
                .Annotation("Npgsql:Enum:mention_source_type", "event_comment,circle_post,circle_comment")
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder,circle_join_request,circle_invite")
                .Annotation("Npgsql:Enum:notification_mute_target_type", "circle,event,user")
                .Annotation("Npgsql:Enum:notification_target_type", "event,circle,circle_thread,community_post,profile,connection,comment")
                .Annotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .Annotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .Annotation("Npgsql:Enum:provider_price_type", "free,fixed,from_price,contact_provider")
                .Annotation("Npgsql:Enum:provider_service_delivery_mode", "in_person,online,hybrid")
                .Annotation("Npgsql:Enum:provider_type", "individual,business")
                .Annotation("Npgsql:Enum:provider_verification_status", "unverified,pending,verified,rejected")
                .Annotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .Annotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .Annotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .Annotation("Npgsql:Enum:weekly_check_in_mood", "great,good,okay,struggling")
                .Annotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .Annotation("Npgsql:Enum:within_role", "user,provider,admin")
                .OldAnnotation("Npgsql:Enum:circle_event_status", "active,removed")
                .OldAnnotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .OldAnnotation("Npgsql:Enum:circle_invite_status", "pending,accepted,declined,cancelled")
                .OldAnnotation("Npgsql:Enum:circle_join_request_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:circle_member_role", "admin,moderator,member")
                .OldAnnotation("Npgsql:Enum:circle_member_status", "active,left,removed,pending,rejected,blocked")
                .OldAnnotation("Npgsql:Enum:circle_post_type", "standard,system,announcement,event_share,weekly_check_in,poll")
                .OldAnnotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .OldAnnotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
                .OldAnnotation("Npgsql:Enum:circle_reaction_type", "support,grateful,inspired,motivated,growing")
                .OldAnnotation("Npgsql:Enum:circle_role_kind", "moderator,admin")
                .OldAnnotation("Npgsql:Enum:circle_status", "active,archived")
                .OldAnnotation("Npgsql:Enum:circle_type", "platform,provider,event_cohort,private_support")
                .OldAnnotation("Npgsql:Enum:circle_visibility", "public,private,hidden")
                .OldAnnotation("Npgsql:Enum:community_content_status", "active,hidden,removed,under_review")
                .OldAnnotation("Npgsql:Enum:community_post_type", "ask_community,share_experience,find_buddy,local_recommendation,reflection")
                .OldAnnotation("Npgsql:Enum:community_report_reason", "spam_or_promotion,harassment_or_abuse,medical_misinformation,inappropriate_content,safety_concern,other")
                .OldAnnotation("Npgsql:Enum:community_report_status", "pending,reviewed,action_taken,dismissed")
                .OldAnnotation("Npgsql:Enum:connection_status", "pending,accepted,rejected,cancelled,removed,blocked")
                .OldAnnotation("Npgsql:Enum:event_invite_status", "pending,accepted,declined,cancelled")
                .OldAnnotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .OldAnnotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .OldAnnotation("Npgsql:Enum:friend_request_permission", "everyone,friends_of_friends,same_circle_or_event,no_one")
                .OldAnnotation("Npgsql:Enum:member_list_visibility", "public,members_only,admins_only,hidden")
                .OldAnnotation("Npgsql:Enum:mention_source_type", "event_comment,circle_post,circle_comment")
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder,circle_join_request,circle_invite")
                .OldAnnotation("Npgsql:Enum:notification_mute_target_type", "circle,event,user")
                .OldAnnotation("Npgsql:Enum:notification_target_type", "event,circle,circle_thread,community_post,profile,connection,comment")
                .OldAnnotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .OldAnnotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .OldAnnotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:signup_type", "internal,external")
                .OldAnnotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .OldAnnotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .OldAnnotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .OldAnnotation("Npgsql:Enum:weekly_check_in_mood", "great,good,okay,struggling")
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                schema: "within",
                table: "Providers",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Abn",
                schema: "within",
                table: "Providers",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "AccessibilityFeatures",
                schema: "within",
                table: "Providers",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "BusinessType",
                schema: "within",
                table: "Providers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Categories",
                schema: "within",
                table: "Providers",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "within",
                table: "Providers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                schema: "within",
                table: "Providers",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                schema: "within",
                table: "Providers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "within",
                table: "Providers",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Facilities",
                schema: "within",
                table: "Providers",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<bool>(
                name: "InPersonAvailable",
                schema: "within",
                table: "Providers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "within",
                table: "Providers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Languages",
                schema: "within",
                table: "Providers",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                schema: "within",
                table: "Providers",
                type: "character varying(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OnlineAvailable",
                schema: "within",
                table: "Providers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OpeningHours",
                schema: "within",
                table: "Providers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "within",
                table: "Providers",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PractitionerTitle",
                schema: "within",
                table: "Providers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                schema: "within",
                table: "Providers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProviderType",
                schema: "within",
                table: "Providers",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Qualifications",
                schema: "within",
                table: "Providers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "ServicesOffered",
                schema: "within",
                table: "Providers",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<bool>(
                name: "ShowEmailPublicly",
                schema: "within",
                table: "Providers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowPhonePublicly",
                schema: "within",
                table: "Providers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowWebsitePublicly",
                schema: "within",
                table: "Providers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                schema: "within",
                table: "Providers",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Suburb",
                schema: "within",
                table: "Providers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "TeamMembers",
                schema: "within",
                table: "Providers",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedUtc",
                schema: "within",
                table: "Providers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                schema: "within",
                table: "Providers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "YearsExperience",
                schema: "within",
                table: "Providers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProviderType",
                schema: "within",
                table: "ProviderApplications",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "ProviderServiceId",
                schema: "within",
                table: "Events",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE within."Providers"
                SET "VerificationStatus" = CASE WHEN "IsVerified" THEN 2 ELSE 0 END,
                    "UpdatedUtc" = "CreatedUtc",
                    "IsActive" = TRUE,
                    "ShowWebsitePublicly" = TRUE
                """);

            migrationBuilder.CreateTable(
                name: "ProviderServices",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Lens = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    PriceAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    PriceType = table.Column<int>(type: "integer", nullable: false),
                    DeliveryMode = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderServices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Providers_ProviderType_Lens_IsActive",
                schema: "within",
                table: "Providers",
                columns: new[] { "ProviderType", "Lens", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Providers_VerificationStatus_CreatedUtc",
                schema: "within",
                table: "Providers",
                columns: new[] { "VerificationStatus", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderApplications_ProviderType",
                schema: "within",
                table: "ProviderApplications",
                column: "ProviderType");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ProviderServiceId",
                schema: "within",
                table: "Events",
                column: "ProviderServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderServices_Lens_DeliveryMode_IsActive",
                schema: "within",
                table: "ProviderServices",
                columns: new[] { "Lens", "DeliveryMode", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderServices_ProviderId_IsActive",
                schema: "within",
                table: "ProviderServices",
                columns: new[] { "ProviderId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderServices",
                schema: "within");

            migrationBuilder.DropIndex(
                name: "IX_Providers_ProviderType_Lens_IsActive",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_VerificationStatus_CreatedUtc",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_ProviderApplications_ProviderType",
                schema: "within",
                table: "ProviderApplications");

            migrationBuilder.DropIndex(
                name: "IX_Events_ProviderServiceId",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Abn",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "AccessibilityFeatures",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "BusinessType",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Categories",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Country",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Facilities",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "InPersonAvailable",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Languages",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "LegalName",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "OnlineAvailable",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "OpeningHours",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "PractitionerTitle",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "ProviderType",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Qualifications",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "ServicesOffered",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "ShowEmailPublicly",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "ShowPhonePublicly",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "ShowWebsitePublicly",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "State",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Suburb",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "TeamMembers",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "YearsExperience",
                schema: "within",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "ProviderType",
                schema: "within",
                table: "ProviderApplications");

            migrationBuilder.DropColumn(
                name: "ProviderServiceId",
                schema: "within",
                table: "Events");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:circle_event_status", "active,removed")
                .Annotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .Annotation("Npgsql:Enum:circle_invite_status", "pending,accepted,declined,cancelled")
                .Annotation("Npgsql:Enum:circle_join_request_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:circle_member_role", "admin,moderator,member")
                .Annotation("Npgsql:Enum:circle_member_status", "active,left,removed,pending,rejected,blocked")
                .Annotation("Npgsql:Enum:circle_post_type", "standard,system,announcement,event_share,weekly_check_in,poll")
                .Annotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .Annotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
                .Annotation("Npgsql:Enum:circle_reaction_type", "support,grateful,inspired,motivated,growing")
                .Annotation("Npgsql:Enum:circle_role_kind", "moderator,admin")
                .Annotation("Npgsql:Enum:circle_status", "active,archived")
                .Annotation("Npgsql:Enum:circle_type", "platform,provider,event_cohort,private_support")
                .Annotation("Npgsql:Enum:circle_visibility", "public,private,hidden")
                .Annotation("Npgsql:Enum:community_content_status", "active,hidden,removed,under_review")
                .Annotation("Npgsql:Enum:community_post_type", "ask_community,share_experience,find_buddy,local_recommendation,reflection")
                .Annotation("Npgsql:Enum:community_report_reason", "spam_or_promotion,harassment_or_abuse,medical_misinformation,inappropriate_content,safety_concern,other")
                .Annotation("Npgsql:Enum:community_report_status", "pending,reviewed,action_taken,dismissed")
                .Annotation("Npgsql:Enum:connection_status", "pending,accepted,rejected,cancelled,removed,blocked")
                .Annotation("Npgsql:Enum:event_invite_status", "pending,accepted,declined,cancelled")
                .Annotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .Annotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .Annotation("Npgsql:Enum:friend_request_permission", "everyone,friends_of_friends,same_circle_or_event,no_one")
                .Annotation("Npgsql:Enum:member_list_visibility", "public,members_only,admins_only,hidden")
                .Annotation("Npgsql:Enum:mention_source_type", "event_comment,circle_post,circle_comment")
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder,circle_join_request,circle_invite")
                .Annotation("Npgsql:Enum:notification_mute_target_type", "circle,event,user")
                .Annotation("Npgsql:Enum:notification_target_type", "event,circle,circle_thread,community_post,profile,connection,comment")
                .Annotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .Annotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .Annotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .Annotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .Annotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .Annotation("Npgsql:Enum:weekly_check_in_mood", "great,good,okay,struggling")
                .Annotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .Annotation("Npgsql:Enum:within_role", "user,provider,admin")
                .OldAnnotation("Npgsql:Enum:circle_event_status", "active,removed")
                .OldAnnotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .OldAnnotation("Npgsql:Enum:circle_invite_status", "pending,accepted,declined,cancelled")
                .OldAnnotation("Npgsql:Enum:circle_join_request_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:circle_member_role", "admin,moderator,member")
                .OldAnnotation("Npgsql:Enum:circle_member_status", "active,left,removed,pending,rejected,blocked")
                .OldAnnotation("Npgsql:Enum:circle_post_type", "standard,system,announcement,event_share,weekly_check_in,poll")
                .OldAnnotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .OldAnnotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
                .OldAnnotation("Npgsql:Enum:circle_reaction_type", "support,grateful,inspired,motivated,growing")
                .OldAnnotation("Npgsql:Enum:circle_role_kind", "moderator,admin")
                .OldAnnotation("Npgsql:Enum:circle_status", "active,archived")
                .OldAnnotation("Npgsql:Enum:circle_type", "platform,provider,event_cohort,private_support")
                .OldAnnotation("Npgsql:Enum:circle_visibility", "public,private,hidden")
                .OldAnnotation("Npgsql:Enum:community_content_status", "active,hidden,removed,under_review")
                .OldAnnotation("Npgsql:Enum:community_post_type", "ask_community,share_experience,find_buddy,local_recommendation,reflection")
                .OldAnnotation("Npgsql:Enum:community_report_reason", "spam_or_promotion,harassment_or_abuse,medical_misinformation,inappropriate_content,safety_concern,other")
                .OldAnnotation("Npgsql:Enum:community_report_status", "pending,reviewed,action_taken,dismissed")
                .OldAnnotation("Npgsql:Enum:connection_status", "pending,accepted,rejected,cancelled,removed,blocked")
                .OldAnnotation("Npgsql:Enum:event_invite_status", "pending,accepted,declined,cancelled")
                .OldAnnotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .OldAnnotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .OldAnnotation("Npgsql:Enum:friend_request_permission", "everyone,friends_of_friends,same_circle_or_event,no_one")
                .OldAnnotation("Npgsql:Enum:member_list_visibility", "public,members_only,admins_only,hidden")
                .OldAnnotation("Npgsql:Enum:mention_source_type", "event_comment,circle_post,circle_comment")
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder,circle_join_request,circle_invite")
                .OldAnnotation("Npgsql:Enum:notification_mute_target_type", "circle,event,user")
                .OldAnnotation("Npgsql:Enum:notification_target_type", "event,circle,circle_thread,community_post,profile,connection,comment")
                .OldAnnotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .OldAnnotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .OldAnnotation("Npgsql:Enum:provider_price_type", "free,fixed,from_price,contact_provider")
                .OldAnnotation("Npgsql:Enum:provider_service_delivery_mode", "in_person,online,hybrid")
                .OldAnnotation("Npgsql:Enum:provider_type", "individual,business")
                .OldAnnotation("Npgsql:Enum:provider_verification_status", "unverified,pending,verified,rejected")
                .OldAnnotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:signup_type", "internal,external")
                .OldAnnotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .OldAnnotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .OldAnnotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .OldAnnotation("Npgsql:Enum:weekly_check_in_mood", "great,good,okay,struggling")
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                schema: "within",
                table: "Providers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(180)",
                oldMaxLength: 180);
        }
    }
}
