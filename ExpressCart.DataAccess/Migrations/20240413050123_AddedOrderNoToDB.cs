using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpressCart.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrderNoToDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderNo",
                table: "OrderHeaders",
                type: "int",
                nullable: false,
                defaultValue: 0);
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
