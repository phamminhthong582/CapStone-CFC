using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class updatechatroom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoom_Employee",
                table: "ChatRoom");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoom_EmployeeId",
                table: "ChatRoom",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoom_Employee",
                table: "ChatRoom",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoom_Employee",
                table: "ChatRoom");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoom_EmployeeId",
                table: "ChatRoom");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoom_Employee",
                table: "ChatRoom",
                column: "CustomerId",
                principalTable: "Employee",
                principalColumn: "EmployeeId");
        }
    }
}
