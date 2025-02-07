using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotbot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class rename_commands_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_command_attachments_commands_CustomCommandId",
                schema: "dotbot",
                table: "command_attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_commands_Guilds_GuildId",
                schema: "dotbot",
                table: "commands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_commands",
                schema: "dotbot",
                table: "commands");

            migrationBuilder.RenameTable(
                name: "commands",
                schema: "dotbot",
                newName: "custom_commands",
                newSchema: "dotbot");

            migrationBuilder.RenameIndex(
                name: "IX_commands_GuildId",
                schema: "dotbot",
                table: "custom_commands",
                newName: "IX_custom_commands_GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_custom_commands",
                schema: "dotbot",
                table: "custom_commands",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "PK_custom_commands",
                schema: "dotbot",
                table: "custom_commands");

            migrationBuilder.RenameTable(
                name: "custom_commands",
                schema: "dotbot",
                newName: "commands",
                newSchema: "dotbot");

            migrationBuilder.RenameIndex(
                name: "IX_custom_commands_GuildId",
                schema: "dotbot",
                table: "commands",
                newName: "IX_commands_GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_commands",
                schema: "dotbot",
                table: "commands",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_command_attachments_commands_CustomCommandId",
                schema: "dotbot",
                table: "command_attachments",
                column: "CustomCommandId",
                principalSchema: "dotbot",
                principalTable: "commands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_commands_Guilds_GuildId",
                schema: "dotbot",
                table: "commands",
                column: "GuildId",
                principalSchema: "dotbot",
                principalTable: "Guilds",
                principalColumn: "Id");
        }
    }
}