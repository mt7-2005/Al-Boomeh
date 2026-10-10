using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Al_BoomehDAL.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumerToProcessedMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProcessedMessages_MessageId",
                table: "ProcessedMessages");

            migrationBuilder.AddColumn<string>(
                name: "Consumer",
                table: "ProcessedMessages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "analytics");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessages_MessageId_Consumer",
                table: "ProcessedMessages",
                columns: new[] { "MessageId", "Consumer" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProcessedMessages_MessageId_Consumer",
                table: "ProcessedMessages");

            migrationBuilder.DropColumn(
                name: "Consumer",
                table: "ProcessedMessages");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessages_MessageId",
                table: "ProcessedMessages",
                column: "MessageId",
                unique: true);
        }
    }
}
