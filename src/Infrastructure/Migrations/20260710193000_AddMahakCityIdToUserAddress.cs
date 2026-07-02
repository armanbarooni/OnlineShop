using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OnlineShop.Infrastructure.Persistence;

#nullable disable

namespace OnlineShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260710193000_AddMahakCityIdToUserAddress")]
    public partial class AddMahakCityIdToUserAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MahakCityId",
                table: "UserAddresses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_MahakCityId",
                table: "UserAddresses",
                column: "MahakCityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_MahakCityId",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "MahakCityId",
                table: "UserAddresses");
        }
    }
}
