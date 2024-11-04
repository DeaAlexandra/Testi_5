using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LemmikkitietokantaSovellus.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Omistajat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nimi = table.Column<string>(type: "TEXT", nullable: false),
                    Puhelin = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Omistajat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lemmikit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nimi = table.Column<string>(type: "TEXT", nullable: false),
                    Rotu = table.Column<string>(type: "TEXT", nullable: false),
                    OmistajaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lemmikit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lemmikit_Omistajat_OmistajaId",
                        column: x => x.OmistajaId,
                        principalTable: "Omistajat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lemmikit_OmistajaId",
                table: "Lemmikit",
                column: "OmistajaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lemmikit");

            migrationBuilder.DropTable(
                name: "Omistajat");
        }
    }
}
