using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class AddIncomWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawMoney_Refund",
                table: "WithdrawMoney");

            migrationBuilder.CreateTable(
                name: "IncomeWallet",
                columns: table => new
                {
                    IncomeWalletID = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    WalletID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncomePrice = table.Column<double>(type: "float", nullable: true),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeWallet", x => x.IncomeWalletID);
                    table.ForeignKey(
                        name: "FK_IncomeWallet_Wallet",
                        column: x => x.WalletID,
                        principalTable: "Wallet",
                        principalColumn: "WalletId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncomeWallet_WalletID",
                table: "IncomeWallet",
                column: "WalletID");

            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawMoney_Wallet",
                table: "WithdrawMoney",
                column: "WalletId",
                principalTable: "Wallet",
                principalColumn: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawMoney_Wallet",
                table: "WithdrawMoney");

            migrationBuilder.DropTable(
                name: "IncomeWallet");

            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawMoney_Refund",
                table: "WithdrawMoney",
                column: "WalletId",
                principalTable: "Wallet",
                principalColumn: "WalletId");
        }
    }
}
