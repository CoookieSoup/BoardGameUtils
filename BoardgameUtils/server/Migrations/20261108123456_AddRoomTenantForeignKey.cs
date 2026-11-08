using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomTenantForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "Rooms" WHERE "TenantId" NOT IN (SELECT "Id" FROM "Tenants");
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_TenantId",
                table: "Rooms",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Tenants_TenantId",
                table: "Rooms",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Tenants_TenantId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_TenantId",
                table: "Rooms");
        }
    }
}
