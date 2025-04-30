using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class updateDesignCustom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DesignCustomId",
                table: "Order",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DesignCustom",
                columns: table => new
                {
                    DesignCustomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    RequestImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestPrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponsePrice = table.Column<double>(type: "float", nullable: true),
                    ResponseImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignCustom", x => x.DesignCustomId);
                    table.ForeignKey(
                        name: "FK_Order_DesignCustom",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DesignCustom_OrderId",
                table: "DesignCustom",
                column: "OrderId",
                unique: true,
                filter: "[OrderId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DesignCustom");

            migrationBuilder.DropColumn(
                name: "DesignCustomId",
                table: "Order");
        }
    }
}
