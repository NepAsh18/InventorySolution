using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySolution.Migrations
{
    /// <inheritdoc />
    public partial class AddDBForProductBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductBatch_Products_ProductId",
                table: "ProductBatch");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductBatch",
                table: "ProductBatch");

            migrationBuilder.RenameTable(
                name: "ProductBatch",
                newName: "ProductBatches");

            migrationBuilder.RenameIndex(
                name: "IX_ProductBatch_ProductId",
                table: "ProductBatches",
                newName: "IX_ProductBatches_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductBatches",
                table: "ProductBatches",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductBatches_Products_ProductId",
                table: "ProductBatches",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductBatches_Products_ProductId",
                table: "ProductBatches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductBatches",
                table: "ProductBatches");

            migrationBuilder.RenameTable(
                name: "ProductBatches",
                newName: "ProductBatch");

            migrationBuilder.RenameIndex(
                name: "IX_ProductBatches_ProductId",
                table: "ProductBatch",
                newName: "IX_ProductBatch_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductBatch",
                table: "ProductBatch",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductBatch_Products_ProductId",
                table: "ProductBatch",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
