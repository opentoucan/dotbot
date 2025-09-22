using System;
using Dotbot.Infrastructure;
using Dotbot.Infrastructure.Entities.Reports;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotbot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_reporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_command_attachments_custom_commands_CustomCommandId",
                schema: "dotbot",
                table: "command_attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_custom_commands_Guilds_GuildId",
                schema: "dotbot",
                table: "custom_commands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Xkcds",
                schema: "dotbot",
                table: "Xkcds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Guilds",
                schema: "dotbot",
                table: "Guilds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_custom_commands",
                schema: "dotbot",
                table: "custom_commands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_command_attachments",
                schema: "dotbot",
                table: "command_attachments");

            migrationBuilder.EnsureSchema(
                name: "vehicle_reporting");

            migrationBuilder.RenameTable(
                name: "Xkcds",
                schema: "dotbot",
                newName: "xkcds",
                newSchema: "dotbot");

            migrationBuilder.RenameTable(
                name: "Guilds",
                schema: "dotbot",
                newName: "guilds",
                newSchema: "dotbot");

            migrationBuilder.RenameColumn(
                name: "Posted",
                schema: "dotbot",
                table: "xkcds",
                newName: "posted");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dotbot",
                table: "xkcds",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ComicNumber",
                schema: "dotbot",
                table: "xkcds",
                newName: "comic_number");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "dotbot",
                table: "guilds",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dotbot",
                table: "guilds",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ExternalId",
                schema: "dotbot",
                table: "guilds",
                newName: "external_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "dotbot",
                table: "custom_commands",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Created",
                schema: "dotbot",
                table: "custom_commands",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "Content",
                schema: "dotbot",
                table: "custom_commands",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dotbot",
                table: "custom_commands",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                schema: "dotbot",
                table: "custom_commands",
                newName: "guild_id");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                schema: "dotbot",
                table: "custom_commands",
                newName: "creator_id");

            migrationBuilder.RenameIndex(
                name: "IX_custom_commands_GuildId",
                schema: "dotbot",
                table: "custom_commands",
                newName: "ix_custom_commands_guild_id");

            migrationBuilder.RenameColumn(
                name: "Url",
                schema: "dotbot",
                table: "command_attachments",
                newName: "url");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "dotbot",
                table: "command_attachments",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dotbot",
                table: "command_attachments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "FileType",
                schema: "dotbot",
                table: "command_attachments",
                newName: "file_type");

            migrationBuilder.RenameColumn(
                name: "CustomCommandId",
                schema: "dotbot",
                table: "command_attachments",
                newName: "custom_command_id");

            migrationBuilder.RenameIndex(
                name: "IX_command_attachments_CustomCommandId",
                schema: "dotbot",
                table: "command_attachments",
                newName: "ix_command_attachments_custom_command_id");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:vehicle_reporting.fuel_type", "diesel,electric,petrol,unknown")
                .Annotation("Npgsql:Enum:vehicle_reporting.mot_defect_category", "advisory,dangerous,fail,major,minor,nonspecific,prs,systemgenerated,userentered")
                .Annotation("Npgsql:Enum:vehicle_reporting.odometer_result", "no_odometer,read,unreadable")
                .Annotation("Npgsql:Enum:vehicle_reporting.test_result", "failed,passed");

            migrationBuilder.AddPrimaryKey(
                name: "pk_xkcds",
                schema: "dotbot",
                table: "xkcds",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_guilds",
                schema: "dotbot",
                table: "guilds",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_custom_commands",
                schema: "dotbot",
                table: "custom_commands",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_command_attachments",
                schema: "dotbot",
                table: "command_attachments",
                column: "id");

            migrationBuilder.CreateTable(
                name: "vehicle_command_log",
                schema: "vehicle_reporting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_plate = table.Column<string>(type: "text", nullable: false),
                    request_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    guild_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_command_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_information",
                schema: "vehicle_reporting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration = table.Column<string>(type: "text", nullable: false),
                    potentially_scrapped = table.Column<bool>(type: "boolean", nullable: false),
                    make = table.Column<string>(type: "text", nullable: true),
                    model = table.Column<string>(type: "text", nullable: true),
                    colour = table.Column<string>(type: "text", nullable: true),
                    fuel_type = table.Column<FuelType>(type: "vehicle_reporting.fuel_type", nullable: false),
                    mot_status_is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    mot_status_valid_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    mot_status_is_exempt = table.Column<bool>(type: "boolean", nullable: false),
                    tax_status_dvla_tax_status_text = table.Column<string>(type: "text", nullable: true),
                    tax_status_is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    tax_status_is_exempt = table.Column<bool>(type: "boolean", nullable: false),
                    tax_status_tax_due_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    registration_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    engine_capacity_litres = table.Column<decimal>(type: "numeric", nullable: true),
                    weight_in_kg = table.Column<int>(type: "integer", nullable: true),
                    co2in_gram_per_kilometer = table.Column<int>(type: "integer", nullable: true),
                    last_issued_v5c_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_information", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_mot_inspection_defect_definitions",
                schema: "vehicle_reporting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    top_level_category = table.Column<string>(type: "text", nullable: false),
                    category_area = table.Column<string>(type: "text", nullable: true),
                    sub_category_name = table.Column<string>(type: "text", nullable: true),
                    defect_reference_code = table.Column<string>(type: "text", nullable: true),
                    defect_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_mot_inspection_defect_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_mot_test",
                schema: "vehicle_reporting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    result = table.Column<TestResult>(type: "vehicle_reporting.test_result", nullable: false),
                    completed_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    odometer_reading_in_miles = table.Column<int>(type: "integer", nullable: true),
                    odometer_read_result = table.Column<OdometerResult>(type: "vehicle_reporting.odometer_result", nullable: false),
                    test_number = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    vehicle_information_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_mot_test", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_mot_test_vehicle_information_vehicle_information_id",
                        column: x => x.vehicle_information_id,
                        principalSchema: "vehicle_reporting",
                        principalTable: "vehicle_information",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "vehicle_mot_test_defect",
                schema: "vehicle_reporting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<MotDefectCategory>(type: "vehicle_reporting.mot_defect_category", nullable: false),
                    defect_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_dangerous = table.Column<bool>(type: "boolean", nullable: false),
                    vehicle_mot_test_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_mot_test_defect", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_mot_test_defect_mot_inspection_defect_definitions_d",
                        column: x => x.defect_definition_id,
                        principalSchema: "vehicle_reporting",
                        principalTable: "vehicle_mot_inspection_defect_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vehicle_mot_test_defect_vehicle_mot_test_vehicle_mot_test_id",
                        column: x => x.vehicle_mot_test_id,
                        principalSchema: "vehicle_reporting",
                        principalTable: "vehicle_mot_test",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_information_registration",
                schema: "vehicle_reporting",
                table: "vehicle_information",
                column: "registration",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_mot_test_test_number",
                schema: "vehicle_reporting",
                table: "vehicle_mot_test",
                column: "test_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_mot_test_vehicle_information_id",
                schema: "vehicle_reporting",
                table: "vehicle_mot_test",
                column: "vehicle_information_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_mot_test_defect_defect_definition_id",
                schema: "vehicle_reporting",
                table: "vehicle_mot_test_defect",
                column: "defect_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_mot_test_defect_vehicle_mot_test_id",
                schema: "vehicle_reporting",
                table: "vehicle_mot_test_defect",
                column: "vehicle_mot_test_id");

            migrationBuilder.AddForeignKey(
                name: "fk_command_attachments_custom_commands_custom_command_id",
                schema: "dotbot",
                table: "command_attachments",
                column: "custom_command_id",
                principalSchema: "dotbot",
                principalTable: "custom_commands",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_custom_commands_guilds_guild_id",
                schema: "dotbot",
                table: "custom_commands",
                column: "guild_id",
                principalSchema: "dotbot",
                principalTable: "guilds",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_command_attachments_custom_commands_custom_command_id",
                schema: "dotbot",
                table: "command_attachments");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_commands_guilds_guild_id",
                schema: "dotbot",
                table: "custom_commands");

            migrationBuilder.DropTable(
                name: "vehicle_command_log",
                schema: "vehicle_reporting");

            migrationBuilder.DropTable(
                name: "vehicle_mot_test_defect",
                schema: "vehicle_reporting");

            migrationBuilder.DropTable(
                name: "vehicle_mot_inspection_defect_definitions",
                schema: "vehicle_reporting");

            migrationBuilder.DropTable(
                name: "vehicle_mot_test",
                schema: "vehicle_reporting");

            migrationBuilder.DropTable(
                name: "vehicle_information",
                schema: "vehicle_reporting");

            migrationBuilder.DropPrimaryKey(
                name: "pk_xkcds",
                schema: "dotbot",
                table: "xkcds");

            migrationBuilder.DropPrimaryKey(
                name: "pk_guilds",
                schema: "dotbot",
                table: "guilds");

            migrationBuilder.DropPrimaryKey(
                name: "pk_custom_commands",
                schema: "dotbot",
                table: "custom_commands");

            migrationBuilder.DropPrimaryKey(
                name: "pk_command_attachments",
                schema: "dotbot",
                table: "command_attachments");

            migrationBuilder.RenameTable(
                name: "xkcds",
                schema: "dotbot",
                newName: "Xkcds",
                newSchema: "dotbot");

            migrationBuilder.RenameTable(
                name: "guilds",
                schema: "dotbot",
                newName: "Guilds",
                newSchema: "dotbot");

            migrationBuilder.RenameColumn(
                name: "posted",
                schema: "dotbot",
                table: "Xkcds",
                newName: "Posted");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "dotbot",
                table: "Xkcds",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "comic_number",
                schema: "dotbot",
                table: "Xkcds",
                newName: "ComicNumber");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "dotbot",
                table: "Guilds",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "dotbot",
                table: "Guilds",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "external_id",
                schema: "dotbot",
                table: "Guilds",
                newName: "ExternalId");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "dotbot",
                table: "custom_commands",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "created",
                schema: "dotbot",
                table: "custom_commands",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "content",
                schema: "dotbot",
                table: "custom_commands",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "dotbot",
                table: "custom_commands",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                schema: "dotbot",
                table: "custom_commands",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                schema: "dotbot",
                table: "custom_commands",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "ix_custom_commands_guild_id",
                schema: "dotbot",
                table: "custom_commands",
                newName: "IX_custom_commands_GuildId");

            migrationBuilder.RenameColumn(
                name: "url",
                schema: "dotbot",
                table: "command_attachments",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "dotbot",
                table: "command_attachments",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "dotbot",
                table: "command_attachments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "file_type",
                schema: "dotbot",
                table: "command_attachments",
                newName: "FileType");

            migrationBuilder.RenameColumn(
                name: "custom_command_id",
                schema: "dotbot",
                table: "command_attachments",
                newName: "CustomCommandId");

            migrationBuilder.RenameIndex(
                name: "ix_command_attachments_custom_command_id",
                schema: "dotbot",
                table: "command_attachments",
                newName: "IX_command_attachments_CustomCommandId");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:vehicle_reporting.fuel_type", "diesel,electric,petrol,unknown")
                .OldAnnotation("Npgsql:Enum:vehicle_reporting.mot_defect_category", "advisory,dangerous,fail,major,minor,nonspecific,prs,systemgenerated,userentered")
                .OldAnnotation("Npgsql:Enum:vehicle_reporting.odometer_result", "no_odometer,read,unreadable")
                .OldAnnotation("Npgsql:Enum:vehicle_reporting.test_result", "failed,passed");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Xkcds",
                schema: "dotbot",
                table: "Xkcds",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Guilds",
                schema: "dotbot",
                table: "Guilds",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_custom_commands",
                schema: "dotbot",
                table: "custom_commands",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_command_attachments",
                schema: "dotbot",
                table: "command_attachments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_command_attachments_custom_commands_CustomCommandId",
                schema: "dotbot",
                table: "command_attachments",
                column: "CustomCommandId",
                principalSchema: "dotbot",
                principalTable: "custom_commands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_custom_commands_Guilds_GuildId",
                schema: "dotbot",
                table: "custom_commands",
                column: "GuildId",
                principalSchema: "dotbot",
                principalTable: "Guilds",
                principalColumn: "Id");
        }
    }
}
