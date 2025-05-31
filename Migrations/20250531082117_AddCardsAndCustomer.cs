using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxuryCarRental.Migrations
{
    /// <inheritdoc />
    public partial class AddCardsAndCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CardId",
                table: "Cards",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "Cards",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "Cards");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Cards",
                newName: "CardId");
        }
    }
}
