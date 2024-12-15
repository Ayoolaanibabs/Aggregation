using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Concession_Utilisation2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CONCESSION_UTILIZATION_HISTORY",
                columns: table => new
                {
                    EntityKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GLAccount = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UtilizationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UtilizedAmount = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONCESSION_UTILIZATION_HISTORY", x => x.EntityKey);
                });

            migrationBuilder.CreateTable(
                name: "ConcessionUtilizationSummary",
                columns: table => new
                {
                    EntityKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GLAccount = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthYear = table.Column<DateOnly>(type: "date", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UtilizedAmount = table.Column<double>(type: "float", nullable: false),
                    ShortMemory = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcessionUtilizationSummary", x => x.EntityKey);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONCESSION_UTILIZATION_HISTORY_AccountNumber",
                table: "CONCESSION_UTILIZATION_HISTORY",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CONCESSION_UTILIZATION_HISTORY_CustomerNumber",
                table: "CONCESSION_UTILIZATION_HISTORY",
                column: "CustomerNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CONCESSION_UTILIZATION_HISTORY_GLAccount",
                table: "CONCESSION_UTILIZATION_HISTORY",
                column: "GLAccount");

            migrationBuilder.CreateIndex(
                name: "IX_ConcessionUtilizationSummary_AccountNumber",
                table: "ConcessionUtilizationSummary",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ConcessionUtilizationSummary_CustomerNumber",
                table: "ConcessionUtilizationSummary",
                column: "CustomerNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ConcessionUtilizationSummary_GLAccount",
                table: "ConcessionUtilizationSummary",
                column: "GLAccount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONCESSION_UTILIZATION_HISTORY");

            migrationBuilder.DropTable(
                name: "ConcessionUtilizationSummary");
        }
    }
}
