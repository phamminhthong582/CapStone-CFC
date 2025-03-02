using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class updateOrderByRecipientTimeAndRecipientName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliveryDateTime",
                table: "Order",
                newName: "RecipientTime");

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "Order",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "Order");

            migrationBuilder.RenameColumn(
                name: "RecipientTime",
                table: "Order",
                newName: "DeliveryDateTime");
        }
    }
}
