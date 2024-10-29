using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fall2024_Assignment3_jtadams5.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMoviePlotSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlotSummary",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlotSummary",
                table: "Movies");
        }
    }
}
