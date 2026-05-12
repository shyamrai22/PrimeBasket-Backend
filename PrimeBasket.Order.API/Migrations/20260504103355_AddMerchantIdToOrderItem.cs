using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrimeBasket.Orders.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantIdToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MerchantId",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "OrderItems");
        }
    }
}
