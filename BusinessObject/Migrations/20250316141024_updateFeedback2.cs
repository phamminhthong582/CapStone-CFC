using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class updateFeedback2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedBackImageByCustomer",
                table: "Feedback");

            migrationBuilder.RenameColumn(
                name: "ResponseFeedImageByStore",
                table: "Feedback",
                newName: "FeedBackVideoByCustomer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FeedBackVideoByCustomer",
                table: "Feedback",
                newName: "ResponseFeedImageByStore");

            migrationBuilder.AddColumn<string>(
                name: "FeedBackImageByCustomer",
                table: "Feedback",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
