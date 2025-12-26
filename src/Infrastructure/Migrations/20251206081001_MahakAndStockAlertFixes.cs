using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MahakAndStockAlertFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockAlerts_AspNetUsers_UserId1",
                table: "StockAlerts");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAlerts_ProductVariants_ProductVariantId",
                table: "StockAlerts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCouponUsages_AspNetUsers_UserId1",
                table: "UserCouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCouponUsages_Coupons_CouponId",
                table: "UserCouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCouponUsages_UserOrders_OrderId",
                table: "UserCouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProductViews_AspNetUsers_UserId1",
                table: "UserProductViews");

            migrationBuilder.DropIndex(
                name: "IX_UserProductViews_UserId1",
                table: "UserProductViews");

            migrationBuilder.DropIndex(
                name: "IX_UserCouponUsages_UserId1",
                table: "UserCouponUsages");

            migrationBuilder.DropIndex(
                name: "IX_StockAlerts_UserId1",
                table: "StockAlerts");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserProductViews");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserCouponUsages");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "StockAlerts");

            migrationBuilder.AlterColumn<int>(
                name: "ViewDuration",
                table: "UserProductViews",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql(@"
                ALTER TABLE ""UserProductViews"" 
                ALTER COLUMN ""UserId"" TYPE uuid 
                USING ""UserId""::uuid;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "UserProductViews",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "UserProductViews",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReferrerUrl",
                table: "UserProductViews",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OperatingSystem",
                table: "UserProductViews",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsReturningView",
                table: "UserProductViews",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "UserProductViews",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceType",
                table: "UserProductViews",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "UserProductViews",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Browser",
                table: "UserProductViews",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.Sql(@"
                ALTER TABLE ""UserCouponUsages"" 
                ALTER COLUMN ""UserId"" TYPE uuid 
                USING ""UserId""::uuid;
            ");

            migrationBuilder.AlterColumn<decimal>(
                name: "OrderTotal",
                table: "UserCouponUsages",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "UserCouponUsages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "UserCouponUsages",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "UserCouponUsages",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<Guid>(
                name: "CouponId1",
                table: "UserCouponUsages",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(@"
                ALTER TABLE ""StockAlerts"" 
                ALTER COLUMN ""UserId"" TYPE uuid 
                USING ""UserId""::uuid;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "StockAlerts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Notified",
                table: "StockAlerts",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "NotificationMethod",
                table: "StockAlerts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "StockAlerts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "StockAlerts",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "ProductSeasons",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "Weight",
                table: "ProductRelations",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "RelationType",
                table: "ProductRelations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ProductRelations",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "ProductRelations",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "ProductMaterials",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductComparisons"" 
                ALTER COLUMN ""UserId"" TYPE uuid 
                USING ""UserId""::uuid;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "OrderStatusHistories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "OrderStatusHistories",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "ChangedBy",
                table: "OrderStatusHistories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProductViews_UserId",
                table: "UserProductViews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProductViews_UserId_ProductId",
                table: "UserProductViews",
                columns: new[] { "UserId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserProductViews_ViewedAt",
                table: "UserProductViews",
                column: "ViewedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserCouponUsages_CouponId1",
                table: "UserCouponUsages",
                column: "CouponId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserCouponUsages_UsedAt",
                table: "UserCouponUsages",
                column: "UsedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserCouponUsages_UserId",
                table: "UserCouponUsages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_Notified",
                table: "StockAlerts",
                column: "Notified");

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_ProductId_UserId",
                table: "StockAlerts",
                columns: new[] { "ProductId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_UserId",
                table: "StockAlerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRelations_IsActive",
                table: "ProductRelations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRelations_ProductId_RelatedProductId",
                table: "ProductRelations",
                columns: new[] { "ProductId", "RelatedProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductRelations_RelationType",
                table: "ProductRelations",
                column: "RelationType");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_ChangedAt",
                table: "OrderStatusHistories",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_Status",
                table: "OrderStatusHistories",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_StockAlerts_AspNetUsers_UserId",
                table: "StockAlerts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAlerts_ProductVariants_ProductVariantId",
                table: "StockAlerts",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCouponUsages_AspNetUsers_UserId",
                table: "UserCouponUsages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCouponUsages_Coupons_CouponId",
                table: "UserCouponUsages",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCouponUsages_Coupons_CouponId1",
                table: "UserCouponUsages",
                column: "CouponId1",
                principalTable: "Coupons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCouponUsages_UserOrders_OrderId",
                table: "UserCouponUsages",
                column: "OrderId",
                principalTable: "UserOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProductViews_AspNetUsers_UserId",
                table: "UserProductViews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockAlerts_AspNetUsers_UserId",
                table: "StockAlerts");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAlerts_ProductVariants_ProductVariantId",
                table: "StockAlerts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCouponUsages_AspNetUsers_UserId",
                table: "UserCouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCouponUsages_Coupons_CouponId",
                table: "UserCouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCouponUsages_Coupons_CouponId1",
                table: "UserCouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCouponUsages_UserOrders_OrderId",
                table: "UserCouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProductViews_AspNetUsers_UserId",
                table: "UserProductViews");

            migrationBuilder.DropIndex(
                name: "IX_UserProductViews_UserId",
                table: "UserProductViews");

            migrationBuilder.DropIndex(
                name: "IX_UserProductViews_UserId_ProductId",
                table: "UserProductViews");

            migrationBuilder.DropIndex(
                name: "IX_UserProductViews_ViewedAt",
                table: "UserProductViews");

            migrationBuilder.DropIndex(
                name: "IX_UserCouponUsages_CouponId1",
                table: "UserCouponUsages");

            migrationBuilder.DropIndex(
                name: "IX_UserCouponUsages_UsedAt",
                table: "UserCouponUsages");

            migrationBuilder.DropIndex(
                name: "IX_UserCouponUsages_UserId",
                table: "UserCouponUsages");

            migrationBuilder.DropIndex(
                name: "IX_StockAlerts_Notified",
                table: "StockAlerts");

            migrationBuilder.DropIndex(
                name: "IX_StockAlerts_ProductId_UserId",
                table: "StockAlerts");

            migrationBuilder.DropIndex(
                name: "IX_StockAlerts_UserId",
                table: "StockAlerts");

            migrationBuilder.DropIndex(
                name: "IX_ProductRelations_IsActive",
                table: "ProductRelations");

            migrationBuilder.DropIndex(
                name: "IX_ProductRelations_ProductId_RelatedProductId",
                table: "ProductRelations");

            migrationBuilder.DropIndex(
                name: "IX_ProductRelations_RelationType",
                table: "ProductRelations");

            migrationBuilder.DropIndex(
                name: "IX_OrderStatusHistories_ChangedAt",
                table: "OrderStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_OrderStatusHistories_Status",
                table: "OrderStatusHistories");

            migrationBuilder.DropColumn(
                name: "CouponId1",
                table: "UserCouponUsages");

            migrationBuilder.AlterColumn<int>(
                name: "ViewDuration",
                table: "UserProductViews",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserProductViews",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "UserProductViews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "UserProductViews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReferrerUrl",
                table: "UserProductViews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OperatingSystem",
                table: "UserProductViews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsReturningView",
                table: "UserProductViews",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "UserProductViews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceType",
                table: "UserProductViews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "UserProductViews",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Browser",
                table: "UserProductViews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "UserProductViews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserCouponUsages",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<decimal>(
                name: "OrderTotal",
                table: "UserCouponUsages",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "UserCouponUsages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "UserCouponUsages",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "UserCouponUsages",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "UserCouponUsages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "StockAlerts",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "StockAlerts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Notified",
                table: "StockAlerts",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "NotificationMethod",
                table: "StockAlerts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "StockAlerts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "StockAlerts",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "StockAlerts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "ProductSeasons",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Weight",
                table: "ProductRelations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "RelationType",
                table: "ProductRelations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ProductRelations",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "ProductRelations",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "ProductMaterials",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ProductComparisons",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "OrderStatusHistories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "OrderStatusHistories",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ChangedBy",
                table: "OrderStatusHistories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProductViews_UserId1",
                table: "UserProductViews",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserCouponUsages_UserId1",
                table: "UserCouponUsages",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_UserId1",
                table: "StockAlerts",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_StockAlerts_AspNetUsers_UserId1",
                table: "StockAlerts",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAlerts_ProductVariants_ProductVariantId",
                table: "StockAlerts",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCouponUsages_AspNetUsers_UserId1",
                table: "UserCouponUsages",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCouponUsages_Coupons_CouponId",
                table: "UserCouponUsages",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCouponUsages_UserOrders_OrderId",
                table: "UserCouponUsages",
                column: "OrderId",
                principalTable: "UserOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProductViews_AspNetUsers_UserId1",
                table: "UserProductViews",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
