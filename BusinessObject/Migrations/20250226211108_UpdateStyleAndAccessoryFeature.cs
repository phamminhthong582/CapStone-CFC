using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class UpdateStyleAndAccessoryFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Feature",
                table: "Style",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Feature",
                table: "Accessory",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Feature",
                table: "Style");

            migrationBuilder.DropColumn(
                name: "Feature",
                table: "Accessory");
        }
    }
}
