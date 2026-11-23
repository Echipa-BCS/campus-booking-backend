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
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nume = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    adresa = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ora_deschidere = table.Column<TimeSpan>(type: "time", nullable: false),
                    ora_inchidere = table.Column<TimeSpan>(type: "time", nullable: false),
                    descriere = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_complexe_sportive", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "utilizatori",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nume = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    prenume = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    parola_hash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rol = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_inregistrare = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utilizatori", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "echipamente",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    complex_id = table.Column<int>(type: "int", nullable: false),
                    denumire = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tip_sport = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    stoc_total = table.Column<int>(type: "int", nullable: false),
                    tarif_inchiriere_per_ora = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_echipamente", x => x.id);
                    table.ForeignKey(
                        name: "FK_echipamente_complexe_sportive_complex_id",
                        column: x => x.complex_id,
                        principalTable: "complexe_sportive",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "terenuri",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    complex_id = table.Column<int>(type: "int", nullable: false),
                    nume = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tip_sport = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    este_acoperit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    tarif_orar_standard = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    durata_minima_ore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_terenuri", x => x.id);
                    table.ForeignKey(
                        name: "FK_terenuri_complexe_sportive_complex_id",
                        column: x => x.complex_id,
                        principalTable: "complexe_sportive",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "abonamente",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    utilizator_id = table.Column<int>(type: "int", nullable: false),
                    tip = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_start = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    data_expirare = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    sedinte_ramase = table.Column<int>(type: "int", nullable: false),
                    activ = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_abonamente", x => x.id);
                    table.ForeignKey(
                        name: "FK_abonamente_utilizatori_utilizator_id",
                        column: x => x.utilizator_id,
                        principalTable: "utilizatori",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rezervari",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    utilizator_id = table.Column<int>(type: "int", nullable: false),
                    teren_id = table.Column<int>(type: "int", nullable: false),
                    cod_rezervare = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_start = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    data_end = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    data_expirare_blocaj = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    pret_total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    discount_aplicat = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    stare = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rezervari", x => x.id);
                    table.ForeignKey(
                        name: "FK_rezervari_terenuri_teren_id",
                        column: x => x.teren_id,
                        principalTable: "terenuri",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rezervari_utilizatori_utilizator_id",
                        column: x => x.utilizator_id,
                        principalTable: "utilizatori",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "plati",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    utilizator_id = table.Column<int>(type: "int", nullable: false),
                    rezervare_id = table.Column<int>(type: "int", nullable: true),
                    abonament_id = table.Column<int>(type: "int", nullable: true),
                    stripe_session_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    suma = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    data_plata = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    stare = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    metoda_plata = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plati", x => x.id);
                    table.ForeignKey(
                        name: "FK_plati_abonamente_abonament_id",
                        column: x => x.abonament_id,
                        principalTable: "abonamente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_plati_rezervari_rezervare_id",
                        column: x => x.rezervare_id,
                        principalTable: "rezervari",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_plati_utilizatori_utilizator_id",
                        column: x => x.utilizator_id,
                        principalTable: "utilizatori",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rezervare_echipamente",
                columns: table => new
                {
                    rezervare_id = table.Column<int>(type: "int", nullable: false),
                    echipament_id = table.Column<int>(type: "int", nullable: false),
                    cantitate = table.Column<int>(type: "int", nullable: false),
                    subtotal_pret = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rezervare_echipamente", x => new { x.rezervare_id, x.echipament_id });
                    table.ForeignKey(
                        name: "FK_rezervare_echipamente_echipamente_echipament_id",
                        column: x => x.echipament_id,
                        principalTable: "echipamente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rezervare_echipamente_rezervari_rezervare_id",
                        column: x => x.rezervare_id,
                        principalTable: "rezervari",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_abonamente_utilizator_id",
                table: "abonamente",
                column: "utilizator_id");

            migrationBuilder.CreateIndex(
                name: "IX_echipamente_complex_id",
                table: "echipamente",
                column: "complex_id");

            migrationBuilder.CreateIndex(
                name: "IX_plati_abonament_id",
                table: "plati",
                column: "abonament_id");

            migrationBuilder.CreateIndex(
                name: "IX_plati_rezervare_id",
                table: "plati",
                column: "rezervare_id");

            migrationBuilder.CreateIndex(
                name: "IX_plati_stripe_session_id",
                table: "plati",
                column: "stripe_session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plati_utilizator_id",
                table: "plati",
                column: "utilizator_id");

            migrationBuilder.CreateIndex(
                name: "IX_rezervare_echipamente_echipament_id",
                table: "rezervare_echipamente",
                column: "echipament_id");

            migrationBuilder.CreateIndex(
                name: "IX_rezervari_cod_rezervare",
                table: "rezervari",
                column: "cod_rezervare",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rezervari_teren_id",
                table: "rezervari",
                column: "teren_id");

            migrationBuilder.CreateIndex(
                name: "IX_rezervari_utilizator_id",
                table: "rezervari",
                column: "utilizator_id");

            migrationBuilder.CreateIndex(
                name: "IX_terenuri_complex_id",
                table: "terenuri",
                column: "complex_id");

            migrationBuilder.CreateIndex(
                name: "IX_utilizatori_email",
                table: "utilizatori",
                column: "email",
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
