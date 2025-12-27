using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money_Spending_Tracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class IndexHashTagNegativEmbeddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagNegativeEmbeddings_TagId",
                table: "TagNegativeEmbeddings");

            migrationBuilder.CreateIndex(
                name: "IX_TagNegativeEmbeddings_TagId_Hash",
                table: "TagNegativeEmbeddings",
                columns: new[] { "TagId", "Hash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagNegativeEmbeddings_TagId_Hash",
                table: "TagNegativeEmbeddings");

            migrationBuilder.CreateIndex(
                name: "IX_TagNegativeEmbeddings_TagId",
                table: "TagNegativeEmbeddings",
                column: "TagId");
        }
    }
}
