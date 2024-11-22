using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotbot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class guild_as_aggregate_root : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_commands_CustomCommandId",
                schema: "dotbot",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "IsServer",
                schema: "dotbot",
                table: "Guilds");

            migrationBuilder.RenameColumn(
                name: "CustomCommandId",
                schema: "dotbot",
                table: "Guilds",
                newName: "Id");

            migrationBuilder.AddColumn<Guid>(
                name: "GuildId",
                schema: "dotbot",
                table: "commands",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "dotbot",
                table: "Guilds",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_commands_GuildId",
                schema: "dotbot",
                table: "commands",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_commands_Guilds_GuildId",
                schema: "dotbot",
                table: "commands",
                column: "GuildId",
                principalSchema: "dotbot",
                principalTable: "Guilds",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_commands_Guilds_GuildId",
                schema: "dotbot",
                table: "commands");

            migrationBuilder.DropIndex(
                name: "IX_commands_GuildId",
                schema: "dotbot",
                table: "commands");

            migrationBuilder.DropColumn(
                name: "GuildId",
                schema: "dotbot",
                table: "commands");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "dotbot",
                table: "Guilds");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dotbot",
                table: "Guilds",
                newName: "CustomCommandId");

            migrationBuilder.AddColumn<bool>(
                name: "IsServer",
                schema: "dotbot",
                table: "Guilds",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_commands_CustomCommandId",
                schema: "dotbot",
                table: "Guilds",
                column: "CustomCommandId",
                principalSchema: "dotbot",
                principalTable: "commands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
