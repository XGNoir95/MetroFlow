using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroFlow.Migrations
{
    /// <inheritdoc />
    public partial class Popularity_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only change the primary key since Id and PopularityIndex already exist
            migrationBuilder.DropPrimaryKey(
                name: "PK_Stations",
                table: "Stations");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Stations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // Remove these lines - columns already exist:
            // migrationBuilder.AddColumn<int>(...Id...)
            // migrationBuilder.AddColumn<int>(...PopularityIndex...)

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stations",
                table: "Stations",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Stations",
                table: "Stations");

            // Don't actually drop these columns in rollback since they contain data
            // migrationBuilder.DropColumn(name: "Id", table: "Stations");
            // migrationBuilder.DropColumn(name: "PopularityIndex", table: "Stations");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Stations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stations",
                table: "Stations",
                column: "Name");
        }
    }
}
