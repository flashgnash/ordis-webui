using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultPresets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert the default "Character" preset for any campaigns that don't already have one
            migrationBuilder.Sql(@"
                INSERT INTO ""CharacterPresets"" (""Name"", ""CampaignId"", stat_block, ""GaugesJson"")
                SELECT
                    'Character',
                    c.""Id"",
                    '{""stats"":{""strength"":10,""agility"":10,""constitution"":10,""intelligence"":10,""wisdom"":10,""charisma"":10},""special_stats"":{}}',
                    '[{""Name"":""health"",""Max"":100,""Colour"":""red"",""GaugeType"":1}]'
                FROM ""Campaigns"" c
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""CharacterPresets"" p WHERE p.""CampaignId"" = c.""Id""
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
