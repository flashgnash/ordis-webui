using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class BuffTemplateSourceLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceTemplateId",
                table: "buff_templates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_buff_templates_SourceTemplateId",
                table: "buff_templates",
                column: "SourceTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_buff_templates_buff_templates_SourceTemplateId",
                table: "buff_templates",
                column: "SourceTemplateId",
                principalTable: "buff_templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_buff_templates_buff_templates_SourceTemplateId",
                table: "buff_templates");

            migrationBuilder.DropIndex(
                name: "IX_buff_templates_SourceTemplateId",
                table: "buff_templates");

            migrationBuilder.DropColumn(
                name: "SourceTemplateId",
                table: "buff_templates");
        }
    }
}
