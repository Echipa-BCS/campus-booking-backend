using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "complexe_sportive",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nume = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Adresa = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OraDeschidere = table.Column<TimeSpan>(type: "time", nullable: false),
                    OraInchidere = table.Column<TimeSpan>(type: "time", nullable: false),
                    Descriere = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_complexe_sportive", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "utilizatori",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nume = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Prenume = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParolaHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rol = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInregistrare = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utilizatori", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "echipamente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ComplexId = table.Column<int>(type: "int", nullable: false),
                    Denumire = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipSport = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StocTotal = table.Column<int>(type: "int", nullable: false),
                    TarifInchirierePerOra = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_echipamente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_echipamente_complexe_sportive_ComplexId",
                        column: x => x.ComplexId,
                        principalTable: "complexe_sportive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "terenuri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ComplexId = table.Column<int>(type: "int", nullable: false),
                    Nume = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipSport = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EsteAcoperit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TarifOrarStandard = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DurataMinimaOre = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_terenuri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_terenuri_complexe_sportive_ComplexId",
                        column: x => x.ComplexId,
                        principalTable: "complexe_sportive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "abonamente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilizatorId = table.Column<int>(type: "int", nullable: false),
                    Tip = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataExpirare = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SedinteRamase = table.Column<int>(type: "int", nullable: false),
                    Activ = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_abonamente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_abonamente_utilizatori_UtilizatorId",
                        column: x => x.UtilizatorId,
                        principalTable: "utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rezervari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilizatorId = table.Column<int>(type: "int", nullable: false),
                    TerenId = table.Column<int>(type: "int", nullable: false),
                    CodRezervare = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataEnd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataExpirareBlocaj = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PretTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DiscountAplicat = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Stare = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rezervari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rezervari_terenuri_TerenId",
                        column: x => x.TerenId,
                        principalTable: "terenuri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rezervari_utilizatori_UtilizatorId",
                        column: x => x.UtilizatorId,
                        principalTable: "utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "plati",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilizatorId = table.Column<int>(type: "int", nullable: false),
                    RezervareId = table.Column<int>(type: "int", nullable: true),
                    AbonamentId = table.Column<int>(type: "int", nullable: true),
                    StripeSessionId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Suma = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DataPlata = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Stare = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MetodaPlata = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plati", x => x.Id);
                    table.ForeignKey(
                        name: "FK_plati_abonamente_AbonamentId",
                        column: x => x.AbonamentId,
                        principalTable: "abonamente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_plati_rezervari_RezervareId",
                        column: x => x.RezervareId,
                        principalTable: "rezervari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_plati_utilizatori_UtilizatorId",
                        column: x => x.UtilizatorId,
                        principalTable: "utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rezervare_echipamente",
                columns: table => new
                {
                    RezervareId = table.Column<int>(type: "int", nullable: false),
                    EchipamentId = table.Column<int>(type: "int", nullable: false),
                    Cantitate = table.Column<int>(type: "int", nullable: false),
                    SubtotalPret = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rezervare_echipamente", x => new { x.RezervareId, x.EchipamentId });
                    table.ForeignKey(
                        name: "FK_rezervare_echipamente_echipamente_EchipamentId",
                        column: x => x.EchipamentId,
                        principalTable: "echipamente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rezervare_echipamente_rezervari_RezervareId",
                        column: x => x.RezervareId,
                        principalTable: "rezervari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_abonamente_UtilizatorId",
                table: "abonamente",
                column: "UtilizatorId");

            migrationBuilder.CreateIndex(
                name: "IX_echipamente_ComplexId",
                table: "echipamente",
                column: "ComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_plati_AbonamentId",
                table: "plati",
                column: "AbonamentId");

            migrationBuilder.CreateIndex(
                name: "IX_plati_RezervareId",
                table: "plati",
                column: "RezervareId");

            migrationBuilder.CreateIndex(
                name: "IX_plati_StripeSessionId",
                table: "plati",
                column: "StripeSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plati_UtilizatorId",
                table: "plati",
                column: "UtilizatorId");

            migrationBuilder.CreateIndex(
                name: "IX_rezervare_echipamente_EchipamentId",
                table: "rezervare_echipamente",
                column: "EchipamentId");

            migrationBuilder.CreateIndex(
                name: "IX_rezervari_CodRezervare",
                table: "rezervari",
                column: "CodRezervare",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rezervari_TerenId",
                table: "rezervari",
                column: "TerenId");

            migrationBuilder.CreateIndex(
                name: "IX_rezervari_UtilizatorId",
                table: "rezervari",
                column: "UtilizatorId");

            migrationBuilder.CreateIndex(
                name: "IX_terenuri_ComplexId",
                table: "terenuri",
                column: "ComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_utilizatori_Email",
                table: "utilizatori",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plati");

            migrationBuilder.DropTable(
                name: "rezervare_echipamente");

            migrationBuilder.DropTable(
                name: "abonamente");

            migrationBuilder.DropTable(
                name: "echipamente");

            migrationBuilder.DropTable(
                name: "rezervari");

            migrationBuilder.DropTable(
                name: "terenuri");

            migrationBuilder.DropTable(
                name: "utilizatori");

            migrationBuilder.DropTable(
                name: "complexe_sportive");
        }
    }
}
