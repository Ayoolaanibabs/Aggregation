using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Add_ShortTerm_To_Activity_Stream : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthAvg",
                table: "ActivityAggregationTable");

            migrationBuilder.DropColumn(
                name: "MonthCount",
                table: "ActivityAggregationTable");

            migrationBuilder.DropColumn(
                name: "MonthMax",
                table: "ActivityAggregationTable");

            migrationBuilder.DropColumn(
                name: "MonthMin",
                table: "ActivityAggregationTable");

            migrationBuilder.DropColumn(
                name: "MonthSum",
                table: "ActivityAggregationTable");

            migrationBuilder.DropColumn(
                name: "MonthYear",
                table: "ActivityAggregationTable");

            migrationBuilder.AddColumn<string>(
                name: "ShortTerm",
                table: "StreamAggregationTable",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortTerm",
                table: "ActivityAggregationTable",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortTerm",
                table: "StreamAggregationTable");

            migrationBuilder.DropColumn(
                name: "ShortTerm",
                table: "ActivityAggregationTable");

            migrationBuilder.AddColumn<double>(
                name: "MonthAvg",
                table: "ActivityAggregationTable",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MonthCount",
                table: "ActivityAggregationTable",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MonthMax",
                table: "ActivityAggregationTable",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MonthMin",
                table: "ActivityAggregationTable",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MonthSum",
                table: "ActivityAggregationTable",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "MonthYear",
                table: "ActivityAggregationTable",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }
    }
}
