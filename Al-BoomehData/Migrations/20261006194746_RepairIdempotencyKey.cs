using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Al_BoomehDAL.Migrations
{
    /// <inheritdoc />
    public partial class RepairIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@" IF COL_LENGTH('dbo.[Order]', 'IdempotencyKey') IS NULL ALTER TABLE [Order] ADD [IdempotencyKey] nvarchar(450) NULL;"); migrationBuilder.Sql(@" UPDATE [Order] SET [IdempotencyKey] = CONVERT(nvarchar(450), NEWID()) WHERE [IdempotencyKey] IS NULL OR [IdempotencyKey] = '';"); migrationBuilder.Sql(@" IF COLUMNPROPERTY(OBJECT_ID('dbo.[Order]'), 'IdempotencyKey', 'AllowsNull') = 1 ALTER TABLE [Order] ALTER COLUMN [IdempotencyKey] nvarchar(450) NOT NULL;"); migrationBuilder.Sql(@" IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Order_IdempotencyKey' AND object_id = OBJECT_ID('dbo.[Order]')) CREATE UNIQUE INDEX [IX_Order_IdempotencyKey] ON [Order] ([IdempotencyKey]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
