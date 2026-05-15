using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace auth_service.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Auth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "password_hash",
                value: "$2a$12$YcemAolnwUuQ.KHej.krBu1SfIgNUnUX9g8pi5JEWt4eXZrdLuY/O");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "password_hash",
                value: "$2a$12$KXpFz.JoyUeI6Frshfao0O3.xZjwDSFnSGsaHpMD5LWLPbYxx9996");
        }
    }
}
