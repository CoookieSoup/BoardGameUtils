using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class RoomStateAsNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Rooms" ALTER COLUMN "State" TYPE integer
                USING CASE "State" WHEN 'Lobby' THEN 0 WHEN 'Playing' THEN 1 END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Rooms" ALTER COLUMN "State" TYPE text
                USING CASE "State" WHEN 0 THEN 'Lobby' WHEN 1 THEN 'Playing' END;
                """);
        }
    }
}
