using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotbot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fix_vehicle_reporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_mot_inspection_defect_definitions_top_level_category_catego",
                schema: "dotbot",
                table: "mot_inspection_defect_definitions",
                columns: new[] { "top_level_category", "category_area", "sub_category_name", "defect_name", "defect_reference_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_mot_inspection_defect_definitions_top_level_category_catego",
                schema: "dotbot",
                table: "mot_inspection_defect_definitions");
        }
    }
}
