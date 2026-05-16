using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterPresets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CharacterPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    stat_block = table.Column<string>(type: "text", nullable: true),
                    GaugesJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterPresets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterPresets_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterPresets_CampaignId",
                table: "CharacterPresets",
                column: "CampaignId");

            // Seed the default "Character" preset for all existing campaigns
            migrationBuilder.Sql(@"
                INSERT INTO ""CharacterPresets"" (""Name"", ""CampaignId"", stat_block, ""GaugesJson"")
                SELECT
                    'Character',
                    ""Id"",
                    '{""stats"":{""strength"":10,""agility"":10,""constitution"":10,""intelligence"":10,""wisdom"":10,""charisma"":10},""special_stats"":{}}',
                    '[{""Name"":""health"",""Max"":100,""Colour"":""red"",""GaugeType"":1}]'
                FROM ""Campaigns"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterPresets");
        }
    }
}
