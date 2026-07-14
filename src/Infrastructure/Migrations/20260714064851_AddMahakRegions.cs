using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMahakRegions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MahakRegions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    CityName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProvinceId = table.Column<int>(type: "integer", nullable: false),
                    ProvinceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MapCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MahakRowVersion = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    SyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MahakId = table.Column<int>(type: "integer", nullable: true),
                    MahakClientId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MahakRegions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MahakRegions_CityId",
                table: "MahakRegions",
                column: "CityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MahakRegions_ProvinceId",
                table: "MahakRegions",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_MahakRegions_ProvinceName",
                table: "MahakRegions",
                column: "ProvinceName");

            migrationBuilder.CreateIndex(
                name: "IX_MahakRegions_ProvinceName_CityName",
                table: "MahakRegions",
                columns: new[] { "ProvinceName", "CityName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MahakRegions");
        }
    }
}
