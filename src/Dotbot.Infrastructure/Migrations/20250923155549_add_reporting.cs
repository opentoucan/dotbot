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
                name: "PK_Guilds",
                schema: "dotbot",
                table: "Guilds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_custom_commands",
                schema: "dotbot",
                table: "custom_commands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Xkcds",
                schema: "dotbot",
                table: "Xkcds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_command_attachments",
                schema: "dotbot",
                table: "command_attachments");

            migrationBuilder.RenameTable(
                name: "Guilds",
                schema: "dotbot",
                newName: "guilds",
                newSchema: "dotbot");

            migrationBuilder.RenameTable(
                name: "Xkcds",
                schema: "dotbot",
                newName: "xkcd",
                newSchema: "dotbot");

            migrationBuilder.RenameTable(
                name: "command_attachments",
                schema: "dotbot",
                newName: "attachments",
                newSchema: "dotbot");

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
                name: "Posted",
                schema: "dotbot",
                table: "xkcd",
                newName: "posted");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dotbot",
                table: "xkcd",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ComicNumber",
                schema: "dotbot",
                table: "xkcd",
                newName: "comic_number");

            migrationBuilder.RenameColumn(
                name: "Url",
                schema: "dotbot",
                table: "attachments",
                newName: "url");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "dotbot",
                table: "attachments",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dotbot",
                table: "attachments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "FileType",
                schema: "dotbot",
                table: "attachments",
                newName: "file_type");

            migrationBuilder.RenameColumn(
                name: "CustomCommandId",
                schema: "dotbot",
                table: "attachments",
                newName: "custom_command_id");

            migrationBuilder.RenameIndex(
                name: "IX_command_attachments_CustomCommandId",
                schema: "dotbot",
                table: "attachments",
                newName: "ix_attachments_custom_command_id");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:dotbot.fuel_type", "diesel,electric,petrol,unknown")
                .Annotation("Npgsql:Enum:dotbot.mot_defect_category", "advisory,dangerous,fail,major,minor,nonspecific,prs,systemgenerated,userentered")
                .Annotation("Npgsql:Enum:dotbot.odometer_result", "no_odometer,read,unreadable")
                .Annotation("Npgsql:Enum:dotbot.test_result", "failed,passed");

            migrationBuilder.AlterColumn<Guid>(
                name: "custom_command_id",
                schema: "dotbot",
                table: "attachments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

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
                name: "pk_xkcd",
                schema: "dotbot",
                table: "xkcd",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_attachments",
                schema: "dotbot",
                table: "attachments",
                column: "id");

            migrationBuilder.CreateTable(
                name: "discord_command_logs",
                schema: "dotbot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    command_name = table.Column<string>(type: "text", nullable: false),
                    identifier = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    guild_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discord_command_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mot_inspection_defect_definitions",
                schema: "dotbot",
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
                    table.PrimaryKey("pk_mot_inspection_defect_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_information",
                schema: "dotbot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration = table.Column<string>(type: "text", nullable: false),
                    potentially_scrapped = table.Column<bool>(type: "boolean", nullable: false),
                    make = table.Column<string>(type: "text", nullable: true),
                    model = table.Column<string>(type: "text", nullable: true),
                    colour = table.Column<string>(type: "text", nullable: true),
                    fuel_type = table.Column<FuelType>(type: "dotbot.fuel_type", nullable: false),
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
                name: "vehicle_mot_test",
                schema: "dotbot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    result = table.Column<TestResult>(type: "dotbot.test_result", nullable: false),
                    completed_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    odometer_reading_in_miles = table.Column<int>(type: "integer", nullable: true),
                    odometer_read_result = table.Column<OdometerResult>(type: "dotbot.odometer_result", nullable: false),
                    test_number = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    vehicle_information_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_mot_test", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_mot_test_vehicle_information_vehicle_information_id",
                        column: x => x.vehicle_information_id,
                        principalSchema: "dotbot",
                        principalTable: "vehicle_information",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "vehicle_mot_test_defect",
                schema: "dotbot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<MotDefectCategory>(type: "dotbot.mot_defect_category", nullable: false),
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
                        principalSchema: "dotbot",
                        principalTable: "mot_inspection_defect_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vehicle_mot_test_defect_vehicle_mot_test_vehicle_mot_test_id",
                        column: x => x.vehicle_mot_test_id,
                        principalSchema: "dotbot",
                        principalTable: "vehicle_mot_test",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_information_registration",
                schema: "dotbot",
                table: "vehicle_information",
                column: "registration",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_mot_test_test_number",
                schema: "dotbot",
                table: "vehicle_mot_test",
                column: "test_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_mot_test_vehicle_information_id",
                schema: "dotbot",
                table: "vehicle_mot_test",
                column: "vehicle_information_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_mot_test_defect_defect_definition_id",
                schema: "dotbot",
                table: "vehicle_mot_test_defect",
                column: "defect_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_mot_test_defect_vehicle_mot_test_id",
                schema: "dotbot",
                table: "vehicle_mot_test_defect",
                column: "vehicle_mot_test_id");

            migrationBuilder.AddForeignKey(
                name: "fk_attachments_custom_commands_custom_command_id",
                schema: "dotbot",
                table: "attachments",
                column: "custom_command_id",
                principalSchema: "dotbot",
                principalTable: "custom_commands",
                principalColumn: "id");

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
                name: "fk_attachments_custom_commands_custom_command_id",
                schema: "dotbot",
                table: "attachments");

            migrationBuilder.DropForeignKey(
                name: "fk_custom_commands_guilds_guild_id",
                schema: "dotbot",
                table: "custom_commands");

            migrationBuilder.DropTable(
                name: "discord_command_logs",
                schema: "dotbot");

            migrationBuilder.DropTable(
                name: "vehicle_mot_test_defect",
                schema: "dotbot");

            migrationBuilder.DropTable(
                name: "mot_inspection_defect_definitions",
                schema: "dotbot");

            migrationBuilder.DropTable(
                name: "vehicle_mot_test",
                schema: "dotbot");

            migrationBuilder.DropTable(
                name: "vehicle_information",
                schema: "dotbot");

            migrationBuilder.DropPrimaryKey(
                name: "pk_guilds",
                schema: "dotbot",
                table: "guilds");

            migrationBuilder.DropPrimaryKey(
                name: "pk_custom_commands",
                schema: "dotbot",
                table: "custom_commands");

            migrationBuilder.DropPrimaryKey(
                name: "pk_xkcd",
                schema: "dotbot",
                table: "xkcd");

            migrationBuilder.DropPrimaryKey(
                name: "pk_attachments",
                schema: "dotbot",
                table: "attachments");

            migrationBuilder.RenameTable(
                name: "guilds",
                schema: "dotbot",
                newName: "Guilds",
                newSchema: "dotbot");

            migrationBuilder.RenameTable(
                name: "xkcd",
                schema: "dotbot",
                newName: "Xkcds",
                newSchema: "dotbot");

            migrationBuilder.RenameTable(
                name: "attachments",
                schema: "dotbot",
                newName: "command_attachments",
                newSchema: "dotbot");

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
                name: "ix_attachments_custom_command_id",
                schema: "dotbot",
                table: "command_attachments",
                newName: "IX_command_attachments_CustomCommandId");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:dotbot.fuel_type", "diesel,electric,petrol,unknown")
                .OldAnnotation("Npgsql:Enum:dotbot.mot_defect_category", "advisory,dangerous,fail,major,minor,nonspecific,prs,systemgenerated,userentered")
                .OldAnnotation("Npgsql:Enum:dotbot.odometer_result", "no_odometer,read,unreadable")
                .OldAnnotation("Npgsql:Enum:dotbot.test_result", "failed,passed");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomCommandId",
                schema: "dotbot",
                table: "command_attachments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

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
                name: "PK_Xkcds",
                schema: "dotbot",
                table: "Xkcds",
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