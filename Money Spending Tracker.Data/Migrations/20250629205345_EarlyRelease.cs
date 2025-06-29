using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money_Spending_Tracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class EarlyRelease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountName = table.Column<string>(type: "TEXT", nullable: false),
                    AccountIban = table.Column<string>(type: "TEXT", nullable: false),
                    InstitutionId = table.Column<string>(type: "TEXT", nullable: false),
                    InstitutionName = table.Column<string>(type: "TEXT", nullable: false),
                    InstitutionLogo = table.Column<byte[]>(type: "BLOB", nullable: true),
                    InstitutionBic = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    InternalTransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", nullable: true),
                    EntryReference = table.Column<string>(type: "TEXT", nullable: true),
                    EndToEndId = table.Column<string>(type: "TEXT", nullable: true),
                    BookingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ValueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TransactionAmount = table.Column<double>(type: "REAL", nullable: false),
                    CreditorName = table.Column<string>(type: "TEXT", nullable: true),
                    UltimateCreditor = table.Column<string>(type: "TEXT", nullable: true),
                    RemittanceInformationStructured = table.Column<string>(type: "TEXT", nullable: true),
                    AdditionalInformation = table.Column<string>(type: "TEXT", nullable: true),
                    PurposeCode = table.Column<string>(type: "TEXT", nullable: true),
                    ProprietaryBankTransactionCode = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => new { x.InternalTransactionId, x.AccountId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
