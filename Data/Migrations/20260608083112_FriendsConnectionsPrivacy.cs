using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class FriendsConnectionsPrivacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:circle_event_status", "active,removed")
                .Annotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .Annotation("Npgsql:Enum:circle_member_status", "active,left,removed")
                .Annotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .Annotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
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
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .Annotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .Annotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .Annotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .Annotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .Annotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
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

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                schema: "within",
                table: "EventRegistrations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AllowAnonymousPosts",
                schema: "within",
                table: "Circles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowHiddenProfiles",
                schema: "within",
                table: "Circles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowPseudonyms",
                schema: "within",
                table: "Circles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DefaultEventRsvpVisibility",
                schema: "within",
                table: "Circles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefaultPostVisibility",
                schema: "within",
                table: "Circles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MemberListVisibility",
                schema: "within",
                table: "Circles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PrivacyType",
                schema: "within",
                table: "Circles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayNameOverride",
                schema: "within",
                table: "CircleMembers",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdentityMode",
                schema: "within",
                table: "CircleMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "within",
                table: "CircleMembers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateTable(
                name: "Connections",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiverUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BlockedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BlockedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventInvites",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventInvites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mentions",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MentionedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MentionedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mentions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPrivacySettings",
                schema: "within",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileVisibility = table.Column<int>(type: "integer", nullable: false),
                    DefaultRsvpVisibility = table.Column<int>(type: "integer", nullable: false),
                    TaggingPermission = table.Column<int>(type: "integer", nullable: false),
                    FriendRequestPermission = table.Column<int>(type: "integer", nullable: false),
                    ShowActivityToFriends = table.Column<bool>(type: "boolean", nullable: false),
                    AllowEventInviteFromFriends = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrivacySettings", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "UserReports",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: true),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Connections_ReceiverUserId_Status",
                schema: "within",
                table: "Connections",
                columns: new[] { "ReceiverUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Connections_RequesterUserId_ReceiverUserId",
                schema: "within",
                table: "Connections",
                columns: new[] { "RequesterUserId", "ReceiverUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Connections_RequesterUserId_Status",
                schema: "within",
                table: "Connections",
                columns: new[] { "RequesterUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EventInvites_EventId_InvitedUserId_Status",
                schema: "within",
                table: "EventInvites",
                columns: new[] { "EventId", "InvitedUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EventInvites_InvitedUserId_Status",
                schema: "within",
                table: "EventInvites",
                columns: new[] { "InvitedUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Mentions_MentionedUserId_CreatedAt",
                schema: "within",
                table: "Mentions",
                columns: new[] { "MentionedUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Mentions_SourceType_SourceId",
                schema: "within",
                table: "Mentions",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReportedByUserId_CreatedAt",
                schema: "within",
                table: "UserReports",
                columns: new[] { "ReportedByUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReportedUserId_Status",
                schema: "within",
                table: "UserReports",
                columns: new[] { "ReportedUserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Connections",
                schema: "within");

            migrationBuilder.DropTable(
                name: "EventInvites",
                schema: "within");

            migrationBuilder.DropTable(
                name: "Mentions",
                schema: "within");

            migrationBuilder.DropTable(
                name: "UserPrivacySettings",
                schema: "within");

            migrationBuilder.DropTable(
                name: "UserReports",
                schema: "within");

            migrationBuilder.DropColumn(
                name: "Visibility",
                schema: "within",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "AllowAnonymousPosts",
                schema: "within",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "AllowHiddenProfiles",
                schema: "within",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "AllowPseudonyms",
                schema: "within",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "DefaultEventRsvpVisibility",
                schema: "within",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "DefaultPostVisibility",
                schema: "within",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "MemberListVisibility",
                schema: "within",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "PrivacyType",
                schema: "within",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "DisplayNameOverride",
                schema: "within",
                table: "CircleMembers");

            migrationBuilder.DropColumn(
                name: "IdentityMode",
                schema: "within",
                table: "CircleMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "within",
                table: "CircleMembers");

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
                .OldAnnotation("Npgsql:Enum:circle_event_status", "active,removed")
                .OldAnnotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .OldAnnotation("Npgsql:Enum:circle_member_status", "active,left,removed")
                .OldAnnotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .OldAnnotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
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
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .OldAnnotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .OldAnnotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .OldAnnotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:signup_type", "internal,external")
                .OldAnnotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .OldAnnotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .OldAnnotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin");
        }
    }
}
