using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class UpdateOrderByShipper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Delivery",
                table: "Order",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Delivery",
                table: "Order");
        }
    }
}
