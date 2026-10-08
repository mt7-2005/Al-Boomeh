using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Al_BoomehDAL.Migrations
{
    /// <inheritdoc />
    public partial class SetPlaceAtUtcNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlacedAtUTC",
                table: "Order",
                newName: "PlacedAtUtc");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PlacedAtUtc",
                table: "Order",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
            migrationBuilder.Sql("UPDATE [Order] SET PlacedAtUtc = NULL WHERE Status = 0;"); 
            migrationBuilder.Sql("UPDATE [Order] SET PlacedAtUtc = CreatedAtUtc " + "WHERE Status <> 0 AND (PlacedAtUtc IS NULL OR PlacedAtUtc < '2000-01-01');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlacedAtUtc",
                table: "Order",
                newName: "PlacedAtUTC");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PlacedAtUTC",
                table: "Order",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
