using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxuryCarRental.Migrations
{
    /// <inheritdoc />
    public partial class DisableFKChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("PRAGMA foreign_keys = OFF;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("PRAGMA foreign_keys = ON;");
        }
    }
}
