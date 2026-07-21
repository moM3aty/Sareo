using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sareoo.Migrations
{
    /// <inheritdoc />
    public partial class AddFoldersLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "StudentCourses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionName",
                table: "Lessons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "StudentCourses");

            migrationBuilder.DropColumn(
                name: "SectionName",
                table: "Lessons");
        }
    }
}
