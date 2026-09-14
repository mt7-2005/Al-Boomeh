using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Al_BoomehDAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameOrdersToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "Order");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_VoucherID",
                table: "Order",
                newName: "IX_Order_VoucherID");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_StoreID",
                table: "Order",
                newName: "IX_Order_StoreID");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_DriverID",
                table: "Order",
                newName: "IX_Order_DriverID");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_CustomerID",
                table: "Order",
                newName: "IX_Order_CustomerID");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_AddressID",
                table: "Order",
                newName: "IX_Order_AddressID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Order",
                newName: "Orders");

            migrationBuilder.RenameIndex(
                name: "IX_Order_VoucherID",
                table: "Orders",
                newName: "IX_Orders_VoucherID");

            migrationBuilder.RenameIndex(
                name: "IX_Order_StoreID",
                table: "Orders",
                newName: "IX_Orders_StoreID");

            migrationBuilder.RenameIndex(
                name: "IX_Order_DriverID",
                table: "Orders",
                newName: "IX_Orders_DriverID");

            migrationBuilder.RenameIndex(
                name: "IX_Order_CustomerID",
                table: "Orders",
                newName: "IX_Orders_CustomerID");

            migrationBuilder.RenameIndex(
                name: "IX_Order_AddressID",
                table: "Orders",
                newName: "IX_Orders_AddressID");
        }
    }
}
