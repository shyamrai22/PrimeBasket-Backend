using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrimeBasket.Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessDetailsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessType",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreDescription",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StoreDescription",
                table: "Users");
        }
    }
}
