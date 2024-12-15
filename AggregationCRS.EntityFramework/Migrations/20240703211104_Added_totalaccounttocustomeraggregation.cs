using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AggregationCRS.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Added_totalaccounttocustomeraggregation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalCount",
                table: "CustomerStreamAggregationTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalCount",
                table: "CustomerActivityAggregationTable",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalCount",
                table: "CustomerStreamAggregationTable");

            migrationBuilder.DropColumn(
                name: "TotalCount",
                table: "CustomerActivityAggregationTable");
        }
    }
}
