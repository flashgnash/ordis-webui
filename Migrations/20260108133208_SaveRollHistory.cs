using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class SaveRollHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RollResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Result = table.Column<float>(type: "real", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    PlayerCharacterId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RollResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RollResults_characters_PlayerCharacterId",
                        column: x => x.PlayerCharacterId,
                        principalTable: "characters",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "PastRolls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Result = table.Column<float>(type: "real", nullable: false),
                    Expression = table.Column<string>(type: "text", nullable: false),
                    RollResultId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastRolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastRolls_RollResults_RollResultId",
                        column: x => x.RollResultId,
                        principalTable: "RollResults",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PastRolls_RollResultId",
                table: "PastRolls",
                column: "RollResultId");

            migrationBuilder.CreateIndex(
                name: "IX_RollResults_PlayerCharacterId",
                table: "RollResults",
                column: "PlayerCharacterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PastRolls");

            migrationBuilder.DropTable(
                name: "RollResults");
        }
    }
}
