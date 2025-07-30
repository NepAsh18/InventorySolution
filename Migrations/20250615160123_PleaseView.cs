using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySolution.Migrations
{
    /// <inheritdoc />
    public partial class PleaseView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecentlyVieweds_AspNetUsers_UserId",
                table: "RecentlyVieweds");

            migrationBuilder.DropForeignKey(
                name: "FK_RecentlyVieweds_Products_ProductId",
                table: "RecentlyVieweds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecentlyVieweds",
                table: "RecentlyVieweds");

            migrationBuilder.RenameTable(
                name: "RecentlyVieweds",
                newName: "RecentlyViewed");

            migrationBuilder.RenameIndex(
                name: "IX_RecentlyVieweds_UserId",
                table: "RecentlyViewed",
                newName: "IX_RecentlyViewed_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RecentlyVieweds_ProductId",
                table: "RecentlyViewed",
                newName: "IX_RecentlyViewed_ProductId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedOn",
                table: "RecentlyViewed",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecentlyViewed",
                table: "RecentlyViewed",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecentlyViewed_AspNetUsers_UserId",
                table: "RecentlyViewed",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecentlyViewed_Products_ProductId",
                table: "RecentlyViewed",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecentlyViewed_AspNetUsers_UserId",
                table: "RecentlyViewed");

            migrationBuilder.DropForeignKey(
                name: "FK_RecentlyViewed_Products_ProductId",
                table: "RecentlyViewed");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecentlyViewed",
                table: "RecentlyViewed");

            migrationBuilder.DropColumn(
                name: "ViewedOn",
                table: "RecentlyViewed");

            migrationBuilder.RenameTable(
                name: "RecentlyViewed",
                newName: "RecentlyVieweds");

            migrationBuilder.RenameIndex(
                name: "IX_RecentlyViewed_UserId",
                table: "RecentlyVieweds",
                newName: "IX_RecentlyVieweds_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RecentlyViewed_ProductId",
                table: "RecentlyVieweds",
                newName: "IX_RecentlyVieweds_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecentlyVieweds",
                table: "RecentlyVieweds",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecentlyVieweds_AspNetUsers_UserId",
                table: "RecentlyVieweds",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecentlyVieweds_Products_ProductId",
                table: "RecentlyVieweds",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
