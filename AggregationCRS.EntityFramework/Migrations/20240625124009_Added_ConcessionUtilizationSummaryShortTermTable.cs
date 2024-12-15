using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Added_ConcessionUtilizationSummaryShortTermTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortMemory",
                table: "ConcessionUtilizationSummary");

            migrationBuilder.AlterColumn<string>(
                name: "ActivityId",
                table: "CustomerStreamAggregationTable",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "ConcessionUtilizationSummaryShortTermTable",
                columns: table => new
                {
                    Ref = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcessionUtilizationSummaryShortTermTable", x => x.Ref);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerStreamAggregationTable_ActivityId",
                table: "CustomerStreamAggregationTable",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ConcessionUtilizationSummaryShortTermTable_ActDate",
                table: "ConcessionUtilizationSummaryShortTermTable",
                column: "ActDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConcessionUtilizationSummaryShortTermTable");

            migrationBuilder.DropIndex(
                name: "IX_CustomerStreamAggregationTable_ActivityId",
                table: "CustomerStreamAggregationTable");

            migrationBuilder.AlterColumn<string>(
                name: "ActivityId",
                table: "CustomerStreamAggregationTable",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ShortMemory",
                table: "ConcessionUtilizationSummary",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
