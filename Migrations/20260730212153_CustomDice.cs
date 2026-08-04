using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class CustomDice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FaceImage",
                table: "RollResults",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaceText",
                table: "RollResults",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "custom_dice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Sides = table.Column<int>(type: "integer", nullable: false),
                    faces = table.Column<string>(type: "text", nullable: false),
                    CampaignId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_dice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_custom_dice_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_custom_dice_CampaignId",
                table: "custom_dice",
                column: "CampaignId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "custom_dice");

            migrationBuilder.DropColumn(
                name: "FaceImage",
                table: "RollResults");

            migrationBuilder.DropColumn(
                name: "FaceText",
                table: "RollResults");
        }
    }
}
