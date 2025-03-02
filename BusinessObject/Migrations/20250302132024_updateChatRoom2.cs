using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class updateChatRoom2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoom_Customer",
                table: "ChatRoom");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoom_Employee",
                table: "ChatRoom");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoom_CustomerId",
                table: "ChatRoom");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoom_EmployeeId",
                table: "ChatRoom");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ChatRoom_CustomerId",
                table: "ChatRoom",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoom_EmployeeId",
                table: "ChatRoom",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoom_Customer",
                table: "ChatRoom",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoom_Employee",
                table: "ChatRoom",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId");
        }
    }
}
