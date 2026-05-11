using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class BuffLibraryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "buff_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    effects = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    CampaignId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_buff_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_buff_templates_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_buff_templates_users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_buffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerCharacterId = table.Column<int>(type: "integer", nullable: false),
                    Stacks = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_character_buffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_character_buffs_buff_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "buff_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_character_buffs_characters_PlayerCharacterId",
                        column: x => x.PlayerCharacterId,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_buff_templates_CampaignId",
                table: "buff_templates",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_buff_templates_OwnerId",
                table: "buff_templates",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_character_buffs_PlayerCharacterId",
                table: "character_buffs",
                column: "PlayerCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_character_buffs_TemplateId",
                table: "character_buffs",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "character_buffs");

            migrationBuilder.DropTable(
                name: "buff_templates");
        }
    }
}
