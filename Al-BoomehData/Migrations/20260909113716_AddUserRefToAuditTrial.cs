using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Al_BoomehDAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRefToAuditTrial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
     name: "UserId",
     table: "AuditTrail");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AuditTrail",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrail_UserId",
                table: "AuditTrail",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditTrail_Users_UserId",
                table: "AuditTrail",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditTrail_Users_UserId",
                table: "AuditTrail");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrail_UserId",
                table: "AuditTrail");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AuditTrail",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
