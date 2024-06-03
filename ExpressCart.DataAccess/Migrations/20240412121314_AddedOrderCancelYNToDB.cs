using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpressCart.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrderCancelYNToDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNo",
                table: "OrderHeaders");

            migrationBuilder.AddColumn<bool>(
                name: "OrderCancelledYN",
                table: "OrderHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderCancelledYN",
                table: "OrderHeaders");

            migrationBuilder.AddColumn<string>(
                name: "OrderNo",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
