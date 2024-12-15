using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Removed_The_Test_fields_on_Activity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthAverage",
                table: "ActivityAggregationTable");

            migrationBuilder.DropColumn(
                name: "MyProperty",
                table: "ActivityAggregationTable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MonthAverage",
                table: "ActivityAggregationTable",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "MyProperty",
                table: "ActivityAggregationTable",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
