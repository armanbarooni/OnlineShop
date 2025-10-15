using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceUserProductViewAndAddNotificationService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Browser",
                table: "UserProductViews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceType",
                table: "UserProductViews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReturningView",
                table: "UserProductViews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperatingSystem",
                table: "UserProductViews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferrerUrl",
                table: "UserProductViews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewDuration",
                table: "UserProductViews",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Browser",
                table: "UserProductViews");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "UserProductViews");

            migrationBuilder.DropColumn(
                name: "IsReturningView",
                table: "UserProductViews");

            migrationBuilder.DropColumn(
                name: "OperatingSystem",
                table: "UserProductViews");

            migrationBuilder.DropColumn(
                name: "ReferrerUrl",
                table: "UserProductViews");

            migrationBuilder.DropColumn(
                name: "ViewDuration",
                table: "UserProductViews");
        }
    }
}
