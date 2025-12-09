using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMahakMappingsUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the incorrect unique index on (EntityType, LocalEntityId)
            // The correct index on (EntityType, MahakEntityId) already exists
            migrationBuilder.DropIndex(
                name: "IX_MahakMappings_EntityType_LocalEntityId",
                table: "MahakMappings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate the old index for rollback
            migrationBuilder.CreateIndex(
                name: "IX_MahakMappings_EntityType_LocalEntityId",
                table: "MahakMappings",
                columns: new[] { "EntityType", "LocalEntityId" },
                unique: true);
        }
    }
}
