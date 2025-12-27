using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money_Spending_Tracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class DebtorName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DebtorName",
                table: "Transactions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DebtorName",
                table: "Transactions");
        }
    }
}
