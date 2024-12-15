using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Included_GlobalActivityCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GlobalActivityCode",
                table: "ActivityAggregationTable",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GlobalActivityCode",
                table: "ActivityAggregationTable");
        }
    }
}
