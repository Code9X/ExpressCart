using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpressCart.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedAddvertisementAndAddvertisementImageToDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Addvertisements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addvertisements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AddvertisementImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddvertisementImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddvertisementImages_Addvertisements_AdId",
                        column: x => x.AdId,
                        principalTable: "Addvertisements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddvertisementImages_AdId",
                table: "AddvertisementImages",
                column: "AdId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddvertisementImages");

            migrationBuilder.DropTable(
                name: "Addvertisements");
        }
    }
}
