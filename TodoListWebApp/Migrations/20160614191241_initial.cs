using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TodoListWebApp.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ObjectID = table.Column<string>(nullable: false),
                    UPN = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ObjectID);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    AdminConsented = table.Column<bool>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    IssValue = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Todoes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Description = table.Column<string>(nullable: true),
                    Owner = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Todoes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PerUserCacheList",
                columns: table => new
                {
                    EntryId = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    LastWrite = table.Column<DateTime>(nullable: false),
                    cacheBits = table.Column<byte[]>(nullable: true),
                    webUserUniqueId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerUserCacheList", x => x.EntryId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Todoes");

            migrationBuilder.DropTable(
                name: "PerUserCacheList");
        }
    }
}
