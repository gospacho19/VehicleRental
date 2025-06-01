using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxuryCarRental.Migrations
{
    /// <inheritdoc />
    public partial class Availability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CurrentlyAvailable",
                table: "Vehicles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentlyAvailable",
                table: "Vehicles");
        }
    }
}
