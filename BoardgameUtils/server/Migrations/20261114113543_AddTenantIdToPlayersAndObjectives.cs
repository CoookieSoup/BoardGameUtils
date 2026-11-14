using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToPlayersAndObjectives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PlayerObjectives",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Players_TenantId",
                table: "Players",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerObjectives_TenantId",
                table: "PlayerObjectives",
                column: "TenantId");

            migrationBuilder.Sql("""
                UPDATE "Players" p SET "TenantId" = r."TenantId" FROM "Rooms" r WHERE p."RoomId" = r."Id";
                UPDATE "PlayerObjectives" o SET "TenantId" = r."TenantId" FROM "Rooms" r WHERE o."RoomId" = r."Id";
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerObjectives_Tenants_TenantId",
                table: "PlayerObjectives",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Tenants_TenantId",
                table: "Players",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerObjectives_Tenants_TenantId",
                table: "PlayerObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Tenants_TenantId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_TenantId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_PlayerObjectives_TenantId",
                table: "PlayerObjectives");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PlayerObjectives");
        }
    }
}
