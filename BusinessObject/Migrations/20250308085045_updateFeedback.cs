using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class updateFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Customer",
                table: "Feedback");

            //migrationBuilder.DropIndex(
            //    name: "IX_Feedback_CustomerId",
            //    table: "Feedback");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Product");

            migrationBuilder.RenameColumn(
                name: "Descripstion",
                table: "Feedback",
                newName: "ResponseFeedImageByStore");

            migrationBuilder.AddColumn<string>(
                name: "FeedBackImageByCustomer",
                table: "Feedback",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackByCustomer",
                table: "Feedback",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequestRefundByCustomer",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseFeedBackStore",
                table: "Feedback",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Feedback",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedBackImageByCustomer",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "FeedbackByCustomer",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "RequestRefundByCustomer",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "ResponseFeedBackStore",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Feedback");

            migrationBuilder.RenameColumn(
                name: "ResponseFeedImageByStore",
                table: "Feedback",
                newName: "Descripstion");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_CustomerId",
                table: "Feedback",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Customer",
                table: "Feedback",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId");
        }
    }
}
