using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Added_AggrgationMonthToCustomerStreamAggregation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "CustomerStreamAggregationTable",
                newName: "TotalAmount");

            migrationBuilder.AlterColumn<string>(
                name: "StreamId",
                table: "CustomerStreamAggregationTable",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "StreamCurrency",
                table: "CustomerStreamAggregationTable",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerNumber",
                table: "CustomerStreamAggregationTable",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateOnly>(
                name: "AggregationMonth",
                table: "CustomerStreamAggregationTable",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_CustomerStreamAggregationTable_AggregationMonth",
                table: "CustomerStreamAggregationTable",
                column: "AggregationMonth");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerStreamAggregationTable_CustomerNumber",
                table: "CustomerStreamAggregationTable",
                column: "CustomerNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerStreamAggregationTable_StreamCurrency",
                table: "CustomerStreamAggregationTable",
                column: "StreamCurrency");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerStreamAggregationTable_StreamId",
                table: "CustomerStreamAggregationTable",
                column: "StreamId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerStreamAggregationTable_StreamType",
                table: "CustomerStreamAggregationTable",
                column: "StreamType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerStreamAggregationTable_AggregationMonth",
                table: "CustomerStreamAggregationTable");

            migrationBuilder.DropIndex(
                name: "IX_CustomerStreamAggregationTable_CustomerNumber",
                table: "CustomerStreamAggregationTable");

            migrationBuilder.DropIndex(
                name: "IX_CustomerStreamAggregationTable_StreamCurrency",
                table: "CustomerStreamAggregationTable");

            migrationBuilder.DropIndex(
                name: "IX_CustomerStreamAggregationTable_StreamId",
                table: "CustomerStreamAggregationTable");

            migrationBuilder.DropIndex(
                name: "IX_CustomerStreamAggregationTable_StreamType",
                table: "CustomerStreamAggregationTable");

            migrationBuilder.DropColumn(
                name: "AggregationMonth",
                table: "CustomerStreamAggregationTable");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "CustomerStreamAggregationTable",
                newName: "Amount");

            migrationBuilder.AlterColumn<string>(
                name: "StreamId",
                table: "CustomerStreamAggregationTable",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "StreamCurrency",
                table: "CustomerStreamAggregationTable",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerNumber",
                table: "CustomerStreamAggregationTable",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
