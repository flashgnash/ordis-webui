using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class AddNpcFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNpc",
                table: "characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNpc",
                table: "characters");
        }
    }
}
