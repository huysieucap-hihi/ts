using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteQL_Testcase.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestExecutions_TestCases_TestCaseId",
                table: "TestExecutions");

            migrationBuilder.AddForeignKey(
                name: "FK_TestExecutions_TestCases_TestCaseId",
                table: "TestExecutions",
                column: "TestCaseId",
                principalTable: "TestCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestExecutions_TestCases_TestCaseId",
                table: "TestExecutions");

            migrationBuilder.AddForeignKey(
                name: "FK_TestExecutions_TestCases_TestCaseId",
                table: "TestExecutions",
                column: "TestCaseId",
                principalTable: "TestCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
