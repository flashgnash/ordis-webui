using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStatBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "StatBlock",
            //     table: "users");

            // migrationBuilder.DropColumn(
            //     name: "StatBlockChannelId",
            //     table: "users");

            // migrationBuilder.DropColumn(
            //     name: "StatBlockHash",
            //     table: "users");

            // migrationBuilder.DropColumn(
            //     name: "StatBlockMessageId",
            //     table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatBlock",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatBlockChannelId",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatBlockHash",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatBlockMessageId",
                table: "users",
                type: "text",
                nullable: true);
        }
    }
}
