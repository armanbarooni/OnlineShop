using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedByMahakToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCouponUsages_Coupons_CouponId1",
                table: "UserCouponUsages");

            migrationBuilder.DropIndex(
                name: "IX_UserCouponUsages_CouponId1",
                table: "UserCouponUsages");

            migrationBuilder.DropColumn(
                name: "CouponId1",
                table: "UserCouponUsages");

            migrationBuilder.AddColumn<bool>(
                name: "DeletedByMahak",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedByMahak",
                table: "Products");

            migrationBuilder.AddColumn<Guid>(
                name: "CouponId1",
                table: "UserCouponUsages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCouponUsages_CouponId1",
                table: "UserCouponUsages",
                column: "CouponId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCouponUsages_Coupons_CouponId1",
                table: "UserCouponUsages",
                column: "CouponId1",
                principalTable: "Coupons",
                principalColumn: "Id");
        }
    }
}
