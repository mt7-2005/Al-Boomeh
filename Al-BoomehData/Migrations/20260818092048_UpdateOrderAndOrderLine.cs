using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Al_BoomehDAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderAndOrderLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimestampUtc",
                table: "OrderStatusHistory");

            migrationBuilder.DropColumn(
                name: "Cash",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Credit",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Wallet",
                table: "Order");

            migrationBuilder.AddColumn<int>(
                name: "ExtraId",
                table: "OrderLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraName",
                table: "OrderLine",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtraPrice",
                table: "OrderLine",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DriverCode",
                table: "Order",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<double>(
                name: "Distance",
                table: "Order",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "ExtraName",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "ExtraPrice",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Order");

            migrationBuilder.AddColumn<DateTime>(
                name: "TimestampUtc",
                table: "OrderStatusHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "DriverCode",
                table: "Order",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Cash",
                table: "Order",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Credit",
                table: "Order",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Wallet",
                table: "Order",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
