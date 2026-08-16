using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModelaFlow.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuthPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_Email",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "users");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "users");
        }
    }
}
