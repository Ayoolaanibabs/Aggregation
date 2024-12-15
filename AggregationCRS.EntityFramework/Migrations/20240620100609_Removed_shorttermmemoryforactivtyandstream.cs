using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Removed_shorttermmemoryforactivtyandstream : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortTerm",
                table: "StreamAggregationTable");

            migrationBuilder.DropColumn(
                name: "ShortTerm",
                table: "GLSummaryTable");

            migrationBuilder.DropColumn(
                name: "ShortTerm",
                table: "ActivityAggregationTable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortTerm",
                table: "StreamAggregationTable",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortTerm",
                table: "GLSummaryTable",
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
    }
}
