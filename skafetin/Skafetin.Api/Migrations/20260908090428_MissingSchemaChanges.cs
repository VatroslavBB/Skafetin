using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Skafetin.Api.Migrations
{
    /// <inheritdoc />
    public partial class MissingSchemaChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCover",
                table: "EquipmentMedia",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Equipment",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedByEmployeeId",
                table: "Assignments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "Assignments",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnNote",
                table: "Assignments",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "InventoryManager");

            migrationBuilder.UpdateData(
                table: "EquipmentCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Računalo");

            migrationBuilder.UpdateData(
                table: "InventoryStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Nacrt");

            migrationBuilder.InsertData(
                table: "LocationTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 5, "Skladište" },
                    { 6, "Terenska lokacija" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_SerialNumber",
                table: "Equipment",
                column: "SerialNumber",
                unique: true,
                filter: "\"SerialNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_AssignedByEmployeeId",
                table: "Assignments",
                column: "AssignedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Employees_AssignedByEmployeeId",
                table: "Assignments",
                column: "AssignedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Employees_AssignedByEmployeeId",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Equipment_SerialNumber",
                table: "Equipment");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_AssignedByEmployeeId",
                table: "Assignments");

            migrationBuilder.DeleteData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "IsCover",
                table: "EquipmentMedia");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "AssignedByEmployeeId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "ReturnNote",
                table: "Assignments");

            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "AssetManager");

            migrationBuilder.UpdateData(
                table: "EquipmentCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Računalna oprema");

            migrationBuilder.UpdateData(
                table: "InventoryStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Skica");
        }
    }
}
