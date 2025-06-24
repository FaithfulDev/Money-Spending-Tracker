using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money_Spending_Tracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<string>(type: "TEXT", nullable: false),
                    EntryReference = table.Column<string>(type: "TEXT", nullable: false),
                    EndToEndId = table.Column<string>(type: "TEXT", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ValueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TransactionAmount = table.Column<double>(type: "REAL", nullable: false),
                    CreditorName = table.Column<string>(type: "TEXT", nullable: false),
                    UltimateCreditor = table.Column<string>(type: "TEXT", nullable: false),
                    RemittanceInformationStructured = table.Column<string>(type: "TEXT", nullable: false),
                    AdditionalInformation = table.Column<string>(type: "TEXT", nullable: false),
                    PurposeCode = table.Column<string>(type: "TEXT", nullable: false),
                    ProprietaryBankTransactionCode = table.Column<string>(type: "TEXT", nullable: false),
                    CreditorAgent = table.Column<string>(type: "TEXT", nullable: false),
                    DebtorAgent = table.Column<string>(type: "TEXT", nullable: false),
                    InternalTransactionId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
