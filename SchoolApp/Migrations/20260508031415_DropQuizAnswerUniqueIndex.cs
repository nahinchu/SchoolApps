using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolApp.Migrations
{
    /// <inheritdoc />
    public partial class DropQuizAnswerUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizAnswer_Attempt_Question",
                table: "QuizAnswers");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswer_Attempt_Question",
                table: "QuizAnswers",
                columns: new[] { "QuizAttemptId", "QuestionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizAnswer_Attempt_Question",
                table: "QuizAnswers");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswer_Attempt_Question",
                table: "QuizAnswers",
                columns: new[] { "QuizAttemptId", "QuestionId" },
                unique: true);
        }
    }
}
