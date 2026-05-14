using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace auth_service.UC1_Auth.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAndAdminSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_login_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Member");

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "avatar_url", "created_at", "email", "full_name", "is_active", "last_login_at", "password_hash", "provider", "provider_id", "role", "updated_at" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@flowboard.app", "Platform Admin", true, null, "$2a$12$bbV6V/fE0YPFu3WtSf99A.oMOX1s5lhODJWxZQ0RhOl90j.NgDVAu", "LOCAL", null, "PlatformAdmin", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "users");

            migrationBuilder.DropColumn(
                name: "last_login_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "role",
                table: "users");
        }
    }
}
