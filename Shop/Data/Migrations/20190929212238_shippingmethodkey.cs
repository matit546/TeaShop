using Microsoft.EntityFrameworkCore.Migrations;

namespace Shop.Data.Migrations
{
    public partial class shippingmethodkey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingMethod",
                table: "OrderHeader");

            migrationBuilder.AddColumn<int>(
                name: "ShippingMethodId",
                table: "OrderHeader",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeader_ShippingMethodId",
                table: "OrderHeader",
                column: "ShippingMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeader_ShippingMethod_ShippingMethodId",
                table: "OrderHeader",
                column: "ShippingMethodId",
                principalTable: "ShippingMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeader_ShippingMethod_ShippingMethodId",
                table: "OrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_OrderHeader_ShippingMethodId",
                table: "OrderHeader");

            migrationBuilder.DropColumn(
                name: "ShippingMethodId",
                table: "OrderHeader");

            migrationBuilder.AddColumn<string>(
                name: "ShippingMethod",
                table: "OrderHeader",
                nullable: false,
                defaultValue: "");
        }
    }
}
