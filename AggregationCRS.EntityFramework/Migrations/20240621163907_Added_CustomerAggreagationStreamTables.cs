using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Added_CustomerAggreagationStreamTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CustomerNumber",
                table: "ActivityAggregationTable",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "CustomerStreamAggregationTable",
                columns: table => new
                {
                    EntityKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StreamType = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StreamCurrency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerStreamAggregationTable", x => x.EntityKey);
                });

            migrationBuilder.CreateTable(
                name: "CustomerStreamShortTermMemoryTable",
                columns: table => new
                {
                    Ref = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerStreamShortTermMemoryTable", x => x.Ref);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreamShortTermMemoryTable_ActDate",
                table: "StreamShortTermMemoryTable",
                column: "ActDate");

            migrationBuilder.CreateIndex(
                name: "IX_DailyGLSummaryShortTermMemoryTable_ActDate",
                table: "DailyGLSummaryShortTermMemoryTable",
                column: "ActDate");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityShortTermMemoryTable_ActDate",
                table: "ActivityShortTermMemoryTable",
                column: "ActDate");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityAggregationTable_CustomerNumber",
                table: "ActivityAggregationTable",
                column: "CustomerNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerStreamShortTermMemoryTable_ActDate",
                table: "CustomerStreamShortTermMemoryTable",
                column: "ActDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerStreamAggregationTable");

            migrationBuilder.DropTable(
                name: "CustomerStreamShortTermMemoryTable");

            migrationBuilder.DropIndex(
                name: "IX_StreamShortTermMemoryTable_ActDate",
                table: "StreamShortTermMemoryTable");

            migrationBuilder.DropIndex(
                name: "IX_DailyGLSummaryShortTermMemoryTable_ActDate",
                table: "DailyGLSummaryShortTermMemoryTable");

            migrationBuilder.DropIndex(
                name: "IX_ActivityShortTermMemoryTable_ActDate",
                table: "ActivityShortTermMemoryTable");

            migrationBuilder.DropIndex(
                name: "IX_ActivityAggregationTable_CustomerNumber",
                table: "ActivityAggregationTable");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerNumber",
                table: "ActivityAggregationTable",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
