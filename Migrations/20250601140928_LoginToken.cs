using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxuryCarRental.Migrations
{
    /// <inheritdoc />
    public partial class LoginToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RememberMeToken",
                table: "Customers");

            migrationBuilder.AddColumn<bool>(
                name: "RememberMe",
                table: "Customers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RememberMe",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "RememberMeToken",
                table: "Customers",
                type: "TEXT",
                nullable: true);
        }
    }
}
