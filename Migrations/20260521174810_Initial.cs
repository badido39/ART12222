using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ART122.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NatureImpots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NatureImpots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Redevables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BP = table.Column<int>(type: "INTEGER", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Adresse = table.Column<string>(type: "TEXT", nullable: false),
                    Article = table.Column<int>(type: "INTEGER", nullable: false),
                    Telephone = table.Column<string>(type: "TEXT", nullable: false),
                    Etablissement = table.Column<string>(type: "TEXT", nullable: false),
                    NIF = table.Column<int>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Activite = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redevables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Impots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BP = table.Column<int>(type: "INTEGER", nullable: false),
                    RoleNumber = table.Column<string>(type: "TEXT", nullable: false),
                    YearImpot = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PA = table.Column<decimal>(type: "TEXT", nullable: false),
                    PR = table.Column<decimal>(type: "TEXT", nullable: false),
                    RedevableInfoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Impots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Impots_Redevables_RedevableInfoId",
                        column: x => x.RedevableInfoId,
                        principalTable: "Redevables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImpotNature",
                columns: table => new
                {
                    ImpotId = table.Column<int>(type: "INTEGER", nullable: false),
                    NatureImpotId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImpotNature", x => new { x.ImpotId, x.NatureImpotId });
                    table.ForeignKey(
                        name: "FK_ImpotNature_Impots_ImpotId",
                        column: x => x.ImpotId,
                        principalTable: "Impots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImpotNature_NatureImpots_NatureImpotId",
                        column: x => x.NatureImpotId,
                        principalTable: "NatureImpots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImpotNature_NatureImpotId",
                table: "ImpotNature",
                column: "NatureImpotId");

            migrationBuilder.CreateIndex(
                name: "IX_Impots_RedevableInfoId",
                table: "Impots",
                column: "RedevableInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Impots_RoleNumber",
                table: "Impots",
                column: "RoleNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Redevables_BP",
                table: "Redevables",
                column: "BP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImpotNature");

            migrationBuilder.DropTable(
                name: "Impots");

            migrationBuilder.DropTable(
                name: "NatureImpots");

            migrationBuilder.DropTable(
                name: "Redevables");
        }
    }
}
