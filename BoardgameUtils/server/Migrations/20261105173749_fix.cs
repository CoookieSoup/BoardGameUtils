using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerObjectives_PlayerId",
                table: "PlayerObjectives");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerObjectives_PlayerId",
                table: "PlayerObjectives",
                column: "PlayerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerObjectives_PlayerId",
                table: "PlayerObjectives");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerObjectives_PlayerId",
                table: "PlayerObjectives",
                column: "PlayerId");
        }
    }
}
