using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OverAudible.Migrations
{
    /// <inheritdoc />
    public partial class full_library : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FullLibrary",
                columns: table => new
                {
                    Asin = table.Column<string>(type: "TEXT", nullable: false),
                    Item = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FullLibrary", x => x.Asin);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FullLibrary");
        }
    }
}
