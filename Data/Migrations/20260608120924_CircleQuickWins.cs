using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class CircleQuickWins : Migration
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
                .OldAnnotation("Npgsql:Enum:circle_join_request_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:circle_member_role", "admin,moderator,member")
                .OldAnnotation("Npgsql:Enum:circle_member_status", "active,left,removed,pending,rejected,blocked")
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
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder,circle_join_request")
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
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin");

            migrationBuilder.AddColumn<string[]>(
                name: "AccessibilityFeatures",
                schema: "within",
                table: "Events",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "AgeRestriction",
                schema: "within",
                table: "Events",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "AtmosphereTags",
                schema: "within",
                table: "Events",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "BringItems",
                schema: "within",
                table: "Events",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "BringNotes",
                schema: "within",
                table: "Events",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "DietaryOptions",
                schema: "within",
                table: "Events",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<bool>(
                name: "DrinksProvided",
                schema: "within",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExperienceLevel",
                schema: "within",
                table: "Events",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Facilities",
                schema: "within",
                table: "Events",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "FoodNotes",
                schema: "within",
                table: "Events",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FoodProvided",
                schema: "within",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhysicalIntensity",
                schema: "within",
                table: "Events",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SafetyNotes",
                schema: "within",
                table: "Events",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialInteractionLevel",
                schema: "within",
                table: "Events",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "within",
                table: "CircleThreads",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                schema: "within",
                table: "CircleThreads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                schema: "within",
                table: "CircleThreads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PostType",
                schema: "within",
                table: "CircleThreads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "WeeklyCheckInWeekStart",
                schema: "within",
                table: "CircleThreads",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                schema: "within",
                table: "CircleThreadComments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Rules",
                schema: "within",
                table: "Circles",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CircleInvites",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CircleId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleInvites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CirclePollOptions",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PollId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CirclePollOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CirclePolls",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    ClosesAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CirclePolls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CirclePollVotes",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PollId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CirclePollVotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CircleReactions",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReactionType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleReactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CircleWeeklyCheckInResponses",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mood = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleWeeklyCheckInResponses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CircleThreads_CircleId_IsPinned_PostType_CreatedAt",
                schema: "within",
                table: "CircleThreads",
                columns: new[] { "CircleId", "IsPinned", "PostType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CircleThreads_CircleId_PostType_WeeklyCheckInWeekStart",
                schema: "within",
                table: "CircleThreads",
                columns: new[] { "CircleId", "PostType", "WeeklyCheckInWeekStart" });

            migrationBuilder.CreateIndex(
                name: "IX_CircleInvites_CircleId_InvitedUserId_Status",
                schema: "within",
                table: "CircleInvites",
                columns: new[] { "CircleId", "InvitedUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CircleInvites_InvitedUserId_Status",
                schema: "within",
                table: "CircleInvites",
                columns: new[] { "InvitedUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CirclePollOptions_PollId_SortOrder",
                schema: "within",
                table: "CirclePollOptions",
                columns: new[] { "PollId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CirclePolls_ThreadId",
                schema: "within",
                table: "CirclePolls",
                column: "ThreadId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CirclePollVotes_OptionId",
                schema: "within",
                table: "CirclePollVotes",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CirclePollVotes_PollId_UserId",
                schema: "within",
                table: "CirclePollVotes",
                columns: new[] { "PollId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CircleReactions_CommentId_UserId_ReactionType",
                schema: "within",
                table: "CircleReactions",
                columns: new[] { "CommentId", "UserId", "ReactionType" },
                unique: true,
                filter: "\"CommentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CircleReactions_ThreadId_UserId_ReactionType",
                schema: "within",
                table: "CircleReactions",
                columns: new[] { "ThreadId", "UserId", "ReactionType" },
                unique: true,
                filter: "\"ThreadId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWeeklyCheckInResponses_ThreadId_UserId",
                schema: "within",
                table: "CircleWeeklyCheckInResponses",
                columns: new[] { "ThreadId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CircleInvites",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CirclePollOptions",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CirclePolls",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CirclePollVotes",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CircleReactions",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CircleWeeklyCheckInResponses",
                schema: "within");

            migrationBuilder.DropIndex(
                name: "IX_CircleThreads_CircleId_IsPinned_PostType_CreatedAt",
                schema: "within",
                table: "CircleThreads");

            migrationBuilder.DropIndex(
                name: "IX_CircleThreads_CircleId_PostType_WeeklyCheckInWeekStart",
                schema: "within",
                table: "CircleThreads");

            migrationBuilder.DropColumn(
                name: "AccessibilityFeatures",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "AgeRestriction",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "AtmosphereTags",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "BringItems",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "BringNotes",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "DietaryOptions",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "DrinksProvided",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ExperienceLevel",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Facilities",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "FoodNotes",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "FoodProvided",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PhysicalIntensity",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SafetyNotes",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SocialInteractionLevel",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "within",
                table: "CircleThreads");

            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                schema: "within",
                table: "CircleThreads");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                schema: "within",
                table: "CircleThreads");

            migrationBuilder.DropColumn(
                name: "PostType",
                schema: "within",
                table: "CircleThreads");

            migrationBuilder.DropColumn(
                name: "WeeklyCheckInWeekStart",
                schema: "within",
                table: "CircleThreads");

            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                schema: "within",
                table: "CircleThreadComments");

            migrationBuilder.DropColumn(
                name: "Rules",
                schema: "within",
                table: "Circles");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:circle_event_status", "active,removed")
                .Annotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .Annotation("Npgsql:Enum:circle_join_request_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:circle_member_role", "admin,moderator,member")
                .Annotation("Npgsql:Enum:circle_member_status", "active,left,removed,pending,rejected,blocked")
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
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder,circle_join_request")
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
        }
    }
}
