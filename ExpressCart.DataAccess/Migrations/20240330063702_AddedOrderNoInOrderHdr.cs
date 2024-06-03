using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpressCart.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrderNoInOrderHdr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderNo",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNo",
                table: "OrderHeaders");
        }
    }
}
