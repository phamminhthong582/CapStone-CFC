using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class updateDesignCustom2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestCard",
                table: "DesignCustom",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestFlowerType",
                table: "DesignCustom",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestMainColor",
                table: "DesignCustom",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestOccasion",
                table: "DesignCustom",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestCard",
                table: "DesignCustom");

            migrationBuilder.DropColumn(
                name: "RequestFlowerType",
                table: "DesignCustom");

            migrationBuilder.DropColumn(
                name: "RequestMainColor",
                table: "DesignCustom");

            migrationBuilder.DropColumn(
                name: "RequestOccasion",
                table: "DesignCustom");
        }
    }
}
