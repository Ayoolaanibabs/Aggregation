using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Added_shorttermmemoryforglSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyGLSummaryShortTermMemoryTable",
                columns: table => new
                {
                    Ref = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyGLSummaryShortTermMemoryTable", x => x.Ref);
                });

            migrationBuilder.CreateTable(
                name: "GLSummaryShortTermMemoryTable",
                columns: table => new
                {
                    Ref = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GLSummaryShortTermMemoryTable", x => x.Ref);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyGLSummaryShortTermMemoryTable");

            migrationBuilder.DropTable(
                name: "GLSummaryShortTermMemoryTable");
        }
    }
}
