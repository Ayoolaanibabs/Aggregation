using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Added_shorttermmemoryandDailyGLComputationSummarytables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityShortTermMemoryTable",
                columns: table => new
                {
                    Ref = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityShortTermMemoryTable", x => x.Ref);
                });

            migrationBuilder.CreateTable(
                name: "DailyGLComputationSummaryTable",
                columns: table => new
                {
                    EntityKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GLAccount = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActualFlow = table.Column<double>(type: "float", nullable: false),
                    ExpectedFlow = table.Column<double>(type: "float", nullable: false),
                    Difference = table.Column<double>(type: "float", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SummaryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyGLComputationSummaryTable", x => x.EntityKey);
                });

            migrationBuilder.CreateTable(
                name: "StreamShortTermMemoryTable",
                columns: table => new
                {
                    Ref = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamShortTermMemoryTable", x => x.Ref);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyGLComputationSummaryTable_GLAccount",
                table: "DailyGLComputationSummaryTable",
                column: "GLAccount");

            migrationBuilder.CreateIndex(
                name: "IX_DailyGLComputationSummaryTable_SummaryDate",
                table: "DailyGLComputationSummaryTable",
                column: "SummaryDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityShortTermMemoryTable");

            migrationBuilder.DropTable(
                name: "DailyGLComputationSummaryTable");

            migrationBuilder.DropTable(
                name: "StreamShortTermMemoryTable");
        }
    }
}
