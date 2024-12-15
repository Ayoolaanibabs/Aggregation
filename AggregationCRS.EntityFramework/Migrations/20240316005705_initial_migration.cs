using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class initial_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityAggregationTable",
                columns: table => new
                {
                    EntityKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DailyActivitySummary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityAggregationTable", x => x.EntityKey);
                });

            migrationBuilder.CreateTable(
                name: "StreamAggregationTable",
                columns: table => new
                {
                    EntityKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamCurrency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DailyStreamSummary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamAggregationTable", x => x.EntityKey);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityAggregationTable");

            migrationBuilder.DropTable(
                name: "StreamAggregationTable");
        }
    }
}
