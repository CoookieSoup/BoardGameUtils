using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class Migration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ObjectiveType",
                table: "PlayerObjectives",
                newName: "ObjectiveId");

            migrationBuilder.AddColumn<int>(
                name: "MaxPlayers",
                table: "Rooms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Objective",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    MinPlayers = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objective", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerObjectives_ObjectiveId",
                table: "PlayerObjectives",
                column: "ObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerObjectives_PlayerId",
                table: "PlayerObjectives",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerObjectives_RoomId",
                table: "PlayerObjectives",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerObjectives_Objective_ObjectiveId",
                table: "PlayerObjectives",
                column: "ObjectiveId",
                principalTable: "Objective",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerObjectives_Players_PlayerId",
                table: "PlayerObjectives",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerObjectives_Rooms_RoomId",
                table: "PlayerObjectives",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerObjectives_Objective_ObjectiveId",
                table: "PlayerObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerObjectives_Players_PlayerId",
                table: "PlayerObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerObjectives_Rooms_RoomId",
                table: "PlayerObjectives");

            migrationBuilder.DropTable(
                name: "Objective");

            migrationBuilder.DropIndex(
                name: "IX_PlayerObjectives_ObjectiveId",
                table: "PlayerObjectives");

            migrationBuilder.DropIndex(
                name: "IX_PlayerObjectives_PlayerId",
                table: "PlayerObjectives");

            migrationBuilder.DropIndex(
                name: "IX_PlayerObjectives_RoomId",
                table: "PlayerObjectives");

            migrationBuilder.DropColumn(
                name: "MaxPlayers",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "ObjectiveId",
                table: "PlayerObjectives",
                newName: "ObjectiveType");
        }
    }
}
