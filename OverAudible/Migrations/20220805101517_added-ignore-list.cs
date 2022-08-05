using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OverAudible.Migrations
{
    /// <inheritdoc />
    public partial class addedignorelist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IgnoreList",
                columns: table => new
                {
                    Asin = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgnoreList", x => x.Asin);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IgnoreList");
        }
    }
}
