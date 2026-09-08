using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrelandFinanceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyBudgetLimitToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyBudgetLimit",
                table: "Categories",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyBudgetLimit",
                table: "Categories");
        }
    }
}
