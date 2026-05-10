using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructureInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHtmlFileType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "HtmlFiles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "HtmlFiles");
        }
    }
}
