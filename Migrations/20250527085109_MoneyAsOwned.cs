using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxuryCarRental.Migrations
{
    /// <inheritdoc />
    

    public partial class MoneyAsOwned : Migration
    {   
        /// <inheritdoc />
        /// 
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Cars_CarId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Cars_CarId",
                table: "Rentals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cars",
                table: "Cars");

            migrationBuilder.DropIndex(
                name: "IX_Cars_Make_Model",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Cars");

            migrationBuilder.RenameTable(
                name: "Cars",
                newName: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "TotalCost",
                table: "Rentals",
                newName: "TotalCost_Currency");

            migrationBuilder.RenameColumn(
                name: "CarId",
                table: "Rentals",
                newName: "VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_CarId",
                table: "Rentals",
                newName: "IX_Rentals_VehicleId");

            migrationBuilder.RenameColumn(
                name: "Subtotal",
                table: "CartItems",
                newName: "Subtotal_Currency");

            migrationBuilder.RenameColumn(
                name: "CarId",
                table: "CartItems",
                newName: "VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_CarId",
                table: "CartItems",
                newName: "IX_CartItems_VehicleId");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost_Amount",
                table: "Rentals",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal_Amount",
                table: "CartItems",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "Year",
                table: "Vehicles",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "Vehicles",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Make",
                table: "Vehicles",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Vehicles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Vehicles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VehicleType",
                table: "Vehicles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vehicles",
                table: "Vehicles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Vehicles_VehicleId",
                table: "CartItems",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Vehicles_VehicleId",
                table: "Rentals",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Vehicles_VehicleId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Vehicles_VehicleId",
                table: "Rentals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vehicles",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TotalCost_Amount",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Subtotal_Amount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "Vehicles");

            migrationBuilder.RenameTable(
                name: "Vehicles",
                newName: "Cars");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "Rentals",
                newName: "CarId");

            migrationBuilder.RenameColumn(
                name: "TotalCost_Currency",
                table: "Rentals",
                newName: "TotalCost");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_VehicleId",
                table: "Rentals",
                newName: "IX_Rentals_CarId");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "CartItems",
                newName: "CarId");

            migrationBuilder.RenameColumn(
                name: "Subtotal_Currency",
                table: "CartItems",
                newName: "Subtotal");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_VehicleId",
                table: "CartItems",
                newName: "IX_CartItems_CarId");

            migrationBuilder.AlterColumn<int>(
                name: "Year",
                table: "Cars",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "Cars",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Make",
                table: "Cars",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Cars",
                type: "TEXT",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cars",
                table: "Cars",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Make_Model",
                table: "Cars",
                columns: new[] { "Make", "Model" });

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Cars_CarId",
                table: "CartItems",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Cars_CarId",
                table: "Rentals",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
