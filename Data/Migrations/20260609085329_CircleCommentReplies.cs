using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class CircleCommentReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentCommentId",
                schema: "within",
                table: "CircleThreadComments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CircleThreadComments_ParentCommentId",
                schema: "within",
                table: "CircleThreadComments",
                column: "ParentCommentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CircleThreadComments_ParentCommentId",
                schema: "within",
                table: "CircleThreadComments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                schema: "within",
                table: "CircleThreadComments");
        }
    }
}
