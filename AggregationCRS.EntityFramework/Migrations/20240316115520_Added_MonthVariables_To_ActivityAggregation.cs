using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Added_MonthVariables_To_ActivityAggregation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MonthAverage",
                table: "ActivityAggregationTable",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthAverage",
                table: "ActivityAggregationTable");

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
        }
    }
}
