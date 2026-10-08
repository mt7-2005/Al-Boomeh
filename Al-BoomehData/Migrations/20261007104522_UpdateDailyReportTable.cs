using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Al_BoomehDAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDailyReportTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyReports_Product_ProductId",
                table: "DailyReports");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReportDateUtc",
                table: "DailyReports",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_StoreId_ReportDateUtc",
                table: "DailyReports",
                columns: new[] { "StoreId", "ReportDateUtc" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyReports_Product_ProductId",
                table: "DailyReports",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyReports_Product_ProductId",
                table: "DailyReports");

            migrationBuilder.DropIndex(
                name: "IX_DailyReports_StoreId_ReportDateUtc",
                table: "DailyReports");

            migrationBuilder.DropColumn(
                name: "ReportDateUtc",
                table: "DailyReports");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyReports_Product_ProductId",
                table: "DailyReports",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
