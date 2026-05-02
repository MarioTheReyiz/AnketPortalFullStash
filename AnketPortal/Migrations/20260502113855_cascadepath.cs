using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnketPortal.Migrations
{
    /// <inheritdoc />
    public partial class cascadepath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyAnswers_AspNetUsers_AppUserId",
                table: "SurveyAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyAnswers_Questions_QuestionId",
                table: "SurveyAnswers");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId1",
                table: "SurveyAnswers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyAnswers_AppUserId1",
                table: "SurveyAnswers",
                column: "AppUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyAnswers_AspNetUsers_AppUserId",
                table: "SurveyAnswers",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyAnswers_AspNetUsers_AppUserId1",
                table: "SurveyAnswers",
                column: "AppUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyAnswers_Questions_QuestionId",
                table: "SurveyAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyAnswers_AspNetUsers_AppUserId",
                table: "SurveyAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyAnswers_AspNetUsers_AppUserId1",
                table: "SurveyAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyAnswers_Questions_QuestionId",
                table: "SurveyAnswers");

            migrationBuilder.DropIndex(
                name: "IX_SurveyAnswers_AppUserId1",
                table: "SurveyAnswers");

            migrationBuilder.DropColumn(
                name: "AppUserId1",
                table: "SurveyAnswers");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyAnswers_AspNetUsers_AppUserId",
                table: "SurveyAnswers",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyAnswers_Questions_QuestionId",
                table: "SurveyAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id");
        }
    }
}
