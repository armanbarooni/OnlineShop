using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnhancedProductEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BrandId",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Products",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "ProductReviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "ProductReviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "ProductReviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ProductReviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "ProductImages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Main");

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Size = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SKU = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AdditionalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    MahakId = table.Column<int>(type: "integer", nullable: true),
                    MahakClientId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductMaterials_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductSeasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    MahakId = table.Column<int>(type: "integer", nullable: true),
                    MahakClientId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSeasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSeasons_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductSeasons_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Gender",
                table: "Products",
                column: "Gender");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_DisplayOrder",
                table: "Brands",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_IsActive",
                table: "Brands",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Name",
                table: "Brands",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_IsActive",
                table: "Materials",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Name",
                table: "Materials",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMaterials_MaterialId",
                table: "ProductMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMaterials_ProductId_MaterialId",
                table: "ProductMaterials",
                columns: new[] { "ProductId", "MaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSeasons_ProductId_SeasonId",
                table: "ProductSeasons",
                columns: new[] { "ProductId", "SeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSeasons_SeasonId",
                table: "ProductSeasons",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_Size_Color",
                table: "ProductVariants",
                columns: new[] { "ProductId", "Size", "Color" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_SKU",
                table: "ProductVariants",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_StockQuantity",
                table: "ProductVariants",
                column: "StockQuantity");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_Code",
                table: "Seasons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_IsActive",
                table: "Seasons",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_Name",
                table: "Seasons",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "ProductMaterials");

            migrationBuilder.DropTable(
                name: "ProductSeasons");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Gender",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "ProductImages");
        }
    }
}
