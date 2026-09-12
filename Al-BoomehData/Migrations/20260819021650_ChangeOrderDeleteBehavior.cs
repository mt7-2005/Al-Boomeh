using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Al_BoomehDAL.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOrderDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Address_Customer",
                table: "Address");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCards_Card",
                table: "CustomerCards");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCards_Customer",
                table: "CustomerCards");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerIssue_Customer",
                table: "CustomerIssue");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerIssue_Issue",
                table: "CustomerIssue");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverIssue_Driver",
                table: "DriverIssue");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverIssue_Issue",
                table: "DriverIssue");

            migrationBuilder.DropForeignKey(
                name: "FK_Extra_Product",
                table: "Extra");

            migrationBuilder.DropForeignKey(
                name: "FK_FavStore_Customer",
                table: "FavStore");

            migrationBuilder.DropForeignKey(
                name: "FK_FavStore_Store",
                table: "FavStore");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderFeedback_Customer",
                table: "OrderFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderFeedback_Order",
                table: "OrderFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Order",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Product",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistory_Order",
                table: "OrderStatusHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Category",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Store",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreIssue_Issue",
                table: "StoreIssue");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreIssue_Store",
                table: "StoreIssue");

            migrationBuilder.DropForeignKey(
                name: "FK_Voucher_Customer",
                table: "Voucher");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    OrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    StoreID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tips = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AddressID = table.Column<int>(type: "int", nullable: true),
                    DriverNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ServiceFees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeliveryFees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StoreNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VoucherID = table.Column<int>(type: "int", nullable: true),
                    DriverCode = table.Column<int>(type: "int", nullable: true),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EstimatedPreparingTime = table.Column<int>(type: "int", nullable: false),
                    EstimatedDeliveryTime = table.Column<int>(type: "int", nullable: false),
                    DriverInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DriverID = table.Column<int>(type: "int", nullable: true),
                    ActualReceivingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Longitude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Distance = table.Column<double>(type: "float", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Order__3214EC0788C73D8C", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_Address",
                        column: x => x.AddressID,
                        principalTable: "Address",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Order_Customer",
                        column: x => x.CustomerID,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_Driver",
                        column: x => x.DriverID,
                        principalTable: "Driver",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Order_Store",
                        column: x => x.StoreID,
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_Voucher",
                        column: x => x.VoucherID,
                        principalTable: "Voucher",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "CK_Unique_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true,
                filter: "[OrderCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AddressID",
                table: "Orders",
                column: "AddressID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerID",
                table: "Orders",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DriverID",
                table: "Orders",
                column: "DriverID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StoreID",
                table: "Orders",
                column: "StoreID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_VoucherID",
                table: "Orders",
                column: "VoucherID");

            migrationBuilder.AddForeignKey(
                name: "FK_Address_Customer",
                table: "Address",
                column: "CustomerID",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCards_Card",
                table: "CustomerCards",
                column: "CardId",
                principalTable: "Card",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCards_Customer",
                table: "CustomerCards",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerIssue_Customer",
                table: "CustomerIssue",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerIssue_Issue",
                table: "CustomerIssue",
                column: "IssueId",
                principalTable: "Issue",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverIssue_Driver",
                table: "DriverIssue",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverIssue_Issue",
                table: "DriverIssue",
                column: "IssueId",
                principalTable: "Issue",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Extra_Product",
                table: "Extra",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FavStore_Customer",
                table: "FavStore",
                column: "CustomerID",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FavStore_Store",
                table: "FavStore",
                column: "StoreID",
                principalTable: "Store",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderFeedback_Customer",
                table: "OrderFeedback",
                column: "CustomerID",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderFeedback_Order",
                table: "OrderFeedback",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Order",
                table: "OrderLine",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Product",
                table: "OrderLine",
                column: "ProductID",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistory_Order",
                table: "OrderStatusHistory",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Category",
                table: "Product",
                column: "CategoryID",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Store",
                table: "Product",
                column: "StoreID",
                principalTable: "Store",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreIssue_Issue",
                table: "StoreIssue",
                column: "IssueId",
                principalTable: "Issue",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreIssue_Store",
                table: "StoreIssue",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Voucher_Customer",
                table: "Voucher",
                column: "CustomerID",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Address_Customer",
                table: "Address");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCards_Card",
                table: "CustomerCards");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCards_Customer",
                table: "CustomerCards");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerIssue_Customer",
                table: "CustomerIssue");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerIssue_Issue",
                table: "CustomerIssue");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverIssue_Driver",
                table: "DriverIssue");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverIssue_Issue",
                table: "DriverIssue");

            migrationBuilder.DropForeignKey(
                name: "FK_Extra_Product",
                table: "Extra");

            migrationBuilder.DropForeignKey(
                name: "FK_FavStore_Customer",
                table: "FavStore");

            migrationBuilder.DropForeignKey(
                name: "FK_FavStore_Store",
                table: "FavStore");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderFeedback_Customer",
                table: "OrderFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderFeedback_Order",
                table: "OrderFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Order",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Product",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistory_Order",
                table: "OrderStatusHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Category",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Store",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreIssue_Issue",
                table: "StoreIssue");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreIssue_Store",
                table: "StoreIssue");

            migrationBuilder.DropForeignKey(
                name: "FK_Voucher_Customer",
                table: "Voucher");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressID = table.Column<int>(type: "int", nullable: true),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    DriverID = table.Column<int>(type: "int", nullable: true),
                    StoreID = table.Column<int>(type: "int", nullable: false),
                    VoucherID = table.Column<int>(type: "int", nullable: true),
                    ActualReceivingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryFees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Distance = table.Column<double>(type: "float", nullable: true),
                    DriverCode = table.Column<int>(type: "int", nullable: true),
                    DriverInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DriverNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EstimatedDeliveryTime = table.Column<int>(type: "int", nullable: false),
                    EstimatedPreparingTime = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Latitude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Longitude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    ServiceFees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StoreNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Tips = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Order__3214EC0788C73D8C", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_Address",
                        column: x => x.AddressID,
                        principalTable: "Address",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Order_Customer",
                        column: x => x.CustomerID,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Order_Driver",
                        column: x => x.DriverID,
                        principalTable: "Driver",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Order_Store",
                        column: x => x.StoreID,
                        principalTable: "Store",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Order_Voucher",
                        column: x => x.VoucherID,
                        principalTable: "Voucher",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "CK_Unique_OrderCode",
                table: "Order",
                column: "OrderCode",
                unique: true,
                filter: "[OrderCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Order_AddressID",
                table: "Order",
                column: "AddressID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_CustomerID",
                table: "Order",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_DriverID",
                table: "Order",
                column: "DriverID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_StoreID",
                table: "Order",
                column: "StoreID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_VoucherID",
                table: "Order",
                column: "VoucherID");

            migrationBuilder.AddForeignKey(
                name: "FK_Address_Customer",
                table: "Address",
                column: "CustomerID",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCards_Card",
                table: "CustomerCards",
                column: "CardId",
                principalTable: "Card",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCards_Customer",
                table: "CustomerCards",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerIssue_Customer",
                table: "CustomerIssue",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerIssue_Issue",
                table: "CustomerIssue",
                column: "IssueId",
                principalTable: "Issue",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverIssue_Driver",
                table: "DriverIssue",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverIssue_Issue",
                table: "DriverIssue",
                column: "IssueId",
                principalTable: "Issue",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Extra_Product",
                table: "Extra",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FavStore_Customer",
                table: "FavStore",
                column: "CustomerID",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FavStore_Store",
                table: "FavStore",
                column: "StoreID",
                principalTable: "Store",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderFeedback_Customer",
                table: "OrderFeedback",
                column: "CustomerID",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderFeedback_Order",
                table: "OrderFeedback",
                column: "OrderID",
                principalTable: "Order",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Order",
                table: "OrderLine",
                column: "OrderID",
                principalTable: "Order",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Product",
                table: "OrderLine",
                column: "ProductID",
                principalTable: "Product",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistory_Order",
                table: "OrderStatusHistory",
                column: "OrderID",
                principalTable: "Order",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Category",
                table: "Product",
                column: "CategoryID",
                principalTable: "Category",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Store",
                table: "Product",
                column: "StoreID",
                principalTable: "Store",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreIssue_Issue",
                table: "StoreIssue",
                column: "IssueId",
                principalTable: "Issue",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreIssue_Store",
                table: "StoreIssue",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Voucher_Customer",
                table: "Voucher",
                column: "CustomerID",
                principalTable: "Customer",
                principalColumn: "Id");
        }
    }
}
