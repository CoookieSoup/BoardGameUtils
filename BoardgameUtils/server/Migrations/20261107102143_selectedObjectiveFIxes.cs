using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class selectedObjectiveFIxes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerObjectives_PlayerId",
                table: "PlayerObjectives");

            migrationBuilder.RenameColumn(
                name: "MaxPlayers",
                table: "PlayerObjectives",
                newName: "MinPlayers");

            migrationBuilder.AddColumn<bool>(
                name: "IsChosen",
                table: "PlayerObjectives",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerObjectives_PlayerId",
                table: "PlayerObjectives",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerObjectives_PlayerId",
                table: "PlayerObjectives");

            migrationBuilder.DropColumn(
                name: "IsChosen",
                table: "PlayerObjectives");

            migrationBuilder.RenameColumn(
                name: "MinPlayers",
                table: "PlayerObjectives",
                newName: "MaxPlayers");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerObjectives_PlayerId",
                table: "PlayerObjectives",
                column: "PlayerId",
                unique: true);
        }
    }
}
