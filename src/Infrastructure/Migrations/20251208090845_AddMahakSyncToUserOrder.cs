using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMahakSyncToUserOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MahakOrderId",
                table: "UserOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MahakSyncedAt",
                table: "UserOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SyncedToMahak",
                table: "UserOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MahakOrderId",
                table: "UserOrders");

            migrationBuilder.DropColumn(
                name: "MahakSyncedAt",
                table: "UserOrders");

            migrationBuilder.DropColumn(
                name: "SyncedToMahak",
                table: "UserOrders");
        }
    }
}
