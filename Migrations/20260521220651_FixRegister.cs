using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ART122.Migrations
{
    /// <inheritdoc />
    public partial class FixRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImpotNature");

            migrationBuilder.DropIndex(
                name: "IX_Redevables_BP",
                table: "Redevables");

            migrationBuilder.DropIndex(
                name: "IX_Impots_RoleNumber",
                table: "Impots");

            migrationBuilder.AddColumn<string>(
                name: "FilsDe",
                table: "Redevables",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "YearImpot",
                table: "Impots",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<decimal>(
                name: "Droit",
                table: "Impots",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "NatureImpotId",
                table: "Impots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Declarations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RedevableInfoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DafaaWahida = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlaAqsat = table.Column<bool>(type: "INTEGER", nullable: false),
                    Droit = table.Column<decimal>(type: "TEXT", nullable: false),
                    PA = table.Column<decimal>(type: "TEXT", nullable: false),
                    PR = table.Column<decimal>(type: "TEXT", nullable: false),
                    MontantTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    MontantRestant = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Declarations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Declarations_Redevables_RedevableInfoId",
                        column: x => x.RedevableInfoId,
                        principalTable: "Redevables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Redevables_Article",
                table: "Redevables",
                column: "Article",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Redevables_BP",
                table: "Redevables",
                column: "BP",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Redevables_NIF",
                table: "Redevables",
                column: "NIF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NatureImpots_Name",
                table: "NatureImpots",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Impots_NatureImpotId",
                table: "Impots",
                column: "NatureImpotId");

            migrationBuilder.CreateIndex(
                name: "IX_Declarations_RedevableInfoId",
                table: "Declarations",
                column: "RedevableInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Impots_NatureImpots_NatureImpotId",
                table: "Impots",
                column: "NatureImpotId",
                principalTable: "NatureImpots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Impots_NatureImpots_NatureImpotId",
                table: "Impots");

            migrationBuilder.DropTable(
                name: "Declarations");

            migrationBuilder.DropIndex(
                name: "IX_Redevables_Article",
                table: "Redevables");

            migrationBuilder.DropIndex(
                name: "IX_Redevables_BP",
                table: "Redevables");

            migrationBuilder.DropIndex(
                name: "IX_Redevables_NIF",
                table: "Redevables");

            migrationBuilder.DropIndex(
                name: "IX_NatureImpots_Name",
                table: "NatureImpots");

            migrationBuilder.DropIndex(
                name: "IX_Impots_NatureImpotId",
                table: "Impots");

            migrationBuilder.DropColumn(
                name: "FilsDe",
                table: "Redevables");

            migrationBuilder.DropColumn(
                name: "Droit",
                table: "Impots");

            migrationBuilder.DropColumn(
                name: "NatureImpotId",
                table: "Impots");

            migrationBuilder.AlterColumn<DateTime>(
                name: "YearImpot",
                table: "Impots",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

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
                name: "IX_Redevables_BP",
                table: "Redevables",
                column: "BP");

            migrationBuilder.CreateIndex(
                name: "IX_Impots_RoleNumber",
                table: "Impots",
                column: "RoleNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ImpotNature_NatureImpotId",
                table: "ImpotNature",
                column: "NatureImpotId");
        }
    }
}
