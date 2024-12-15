using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Added_Customer_activity_aggregation_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerActivityAggregationTable",
                columns: table => new
                {
                    EntityKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActivityId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActivityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GlobalActivityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AggregationMonth = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerActivityAggregationTable", x => x.EntityKey);
                });

            migrationBuilder.CreateTable(
                name: "CustomerActivityShortTermMemoryTable",
                columns: table => new
                {
                    Ref = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerActivityShortTermMemoryTable", x => x.Ref);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerActivityAggregationTable_ActivityId",
                table: "CustomerActivityAggregationTable",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerActivityAggregationTable_AggregationMonth",
                table: "CustomerActivityAggregationTable",
                column: "AggregationMonth");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerActivityAggregationTable_Currency",
                table: "CustomerActivityAggregationTable",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerActivityAggregationTable_CustomerNumber",
                table: "CustomerActivityAggregationTable",
                column: "CustomerNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerActivityShortTermMemoryTable_ActDate",
                table: "CustomerActivityShortTermMemoryTable",
                column: "ActDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerActivityAggregationTable");

            migrationBuilder.DropTable(
                name: "CustomerActivityShortTermMemoryTable");
        }
    }
}
