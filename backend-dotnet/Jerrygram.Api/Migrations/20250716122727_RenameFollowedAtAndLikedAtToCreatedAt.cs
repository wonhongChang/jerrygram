using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jerrygram.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameFollowedAtAndLikedAtToCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FollowedAt",
                table: "UserFollows",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "LikedAt",
                table: "PostLikes",
                newName: "CreatedAt");

            migrationBuilder.UpdateData(
                table: "Comments",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 16, 12, 27, 27, 542, DateTimeKind.Utc).AddTicks(7210));

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 16, 12, 27, 27, 542, DateTimeKind.Utc).AddTicks(7195));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 16, 12, 27, 27, 542, DateTimeKind.Utc).AddTicks(7117));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "UserFollows",
                newName: "FollowedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "PostLikes",
                newName: "LikedAt");

            migrationBuilder.UpdateData(
                table: "Comments",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 14, 13, 32, 6, 28, DateTimeKind.Utc).AddTicks(672));

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 14, 13, 32, 6, 28, DateTimeKind.Utc).AddTicks(656));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 14, 13, 32, 6, 28, DateTimeKind.Utc).AddTicks(576));
        }
    }
}
