using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnketPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddSkipLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NextQuestionId",
                table: "QuestionOptions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextQuestionId",
                table: "QuestionOptions");
        }
    }
}
