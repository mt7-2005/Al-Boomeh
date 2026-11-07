using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Al_BoomehDAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToOutboxMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Pending",
                table: "OutboxMessages",
                column: "Id",
                filter: "[SentAtUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Pending",
                table: "OutboxMessages");

            
        }
    }
}
