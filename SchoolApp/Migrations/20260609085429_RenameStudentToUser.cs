using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolApp.Migrations
{
    /// <inheritdoc />
    public partial class RenameStudentToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Drop all FK constraints referencing Students table ──────────
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Students_StudentId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseReviews_Students_StudentId",
                table: "CourseReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonProgresses_Students_StudentId",
                table: "LessonProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Students_StudentId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Students_StudentId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Students_StudentId",
                table: "QuizAttempts");

            // ── 2. Drop old columns from Students before rename ──────────────
            migrationBuilder.DropColumn(
                name: "ResetPasswordToken",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ResetPasswordExpiry",
                table: "Students");

            // ── 3. Rename Students table → Users ─────────────────────────────
            migrationBuilder.RenameTable(
                name: "Students",
                newName: "Users");

            // ── 4. Rename primary key column StudentId → UserId ──────────────
            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Users",
                newName: "UserId");

            // ── 5. Add new columns to Users ──────────────────────────────────
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // Allow FullName to be nullable
            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            // Allow Phone to be nullable (was required before)
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);

            // Allow DateOfBirth to be nullable
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            // ── 6. Rename FK columns in child tables ─────────────────────────
            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "QuizAttempts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_QuizAttempt_Student_Quiz",
                table: "QuizAttempts",
                newName: "IX_QuizAttempt_User_Quiz");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Payments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_StudentId",
                table: "Payments",
                newName: "IX_Payments_UserId");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Notifications",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_Student_Read",
                table: "Notifications",
                newName: "IX_Notification_User_Read");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "LessonProgresses",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_LessonProgress_Student_Lesson",
                table: "LessonProgresses",
                newName: "IX_LessonProgress_User_Lesson");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Enrollments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                newName: "IX_Enrollments_UserId");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "CourseReviews",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseReviews_StudentId",
                table: "CourseReviews",
                newName: "IX_CourseReviews_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseReview_Course_Student",
                table: "CourseReviews",
                newName: "IX_CourseReview_Course_User");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Certificates",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Certificates_StudentId",
                table: "Certificates",
                newName: "IX_Certificates_UserId");

            // ── 7. Re-add FK constraints pointing to Users ───────────────────
            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Users_UserId",
                table: "Certificates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseReviews_Users_UserId",
                table: "CourseReviews",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Users_UserId",
                table: "Enrollments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonProgresses_Users_UserId",
                table: "LessonProgresses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Users_UserId",
                table: "QuizAttempts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Certificates_Users_UserId", table: "Certificates");
            migrationBuilder.DropForeignKey(name: "FK_CourseReviews_Users_UserId", table: "CourseReviews");
            migrationBuilder.DropForeignKey(name: "FK_Enrollments_Users_UserId", table: "Enrollments");
            migrationBuilder.DropForeignKey(name: "FK_LessonProgresses_Users_UserId", table: "LessonProgresses");
            migrationBuilder.DropForeignKey(name: "FK_Notifications_Users_UserId", table: "Notifications");
            migrationBuilder.DropForeignKey(name: "FK_Payments_Users_UserId", table: "Payments");
            migrationBuilder.DropForeignKey(name: "FK_QuizAttempts_Users_UserId", table: "QuizAttempts");

            migrationBuilder.DropColumn(name: "IsEmailVerified", table: "Users");

            migrationBuilder.RenameColumn(name: "UserId", table: "QuizAttempts", newName: "StudentId");
            migrationBuilder.RenameIndex(name: "IX_QuizAttempt_User_Quiz", table: "QuizAttempts", newName: "IX_QuizAttempt_Student_Quiz");
            migrationBuilder.RenameColumn(name: "UserId", table: "Payments", newName: "StudentId");
            migrationBuilder.RenameIndex(name: "IX_Payments_UserId", table: "Payments", newName: "IX_Payments_StudentId");
            migrationBuilder.RenameColumn(name: "UserId", table: "Notifications", newName: "StudentId");
            migrationBuilder.RenameIndex(name: "IX_Notification_User_Read", table: "Notifications", newName: "IX_Notification_Student_Read");
            migrationBuilder.RenameColumn(name: "UserId", table: "LessonProgresses", newName: "StudentId");
            migrationBuilder.RenameIndex(name: "IX_LessonProgress_User_Lesson", table: "LessonProgresses", newName: "IX_LessonProgress_Student_Lesson");
            migrationBuilder.RenameColumn(name: "UserId", table: "Enrollments", newName: "StudentId");
            migrationBuilder.RenameIndex(name: "IX_Enrollments_UserId", table: "Enrollments", newName: "IX_Enrollments_StudentId");
            migrationBuilder.RenameColumn(name: "UserId", table: "CourseReviews", newName: "StudentId");
            migrationBuilder.RenameIndex(name: "IX_CourseReviews_UserId", table: "CourseReviews", newName: "IX_CourseReviews_StudentId");
            migrationBuilder.RenameIndex(name: "IX_CourseReview_Course_User", table: "CourseReviews", newName: "IX_CourseReview_Course_Student");
            migrationBuilder.RenameColumn(name: "UserId", table: "Certificates", newName: "StudentId");
            migrationBuilder.RenameIndex(name: "IX_Certificates_UserId", table: "Certificates", newName: "IX_Certificates_StudentId");

            migrationBuilder.RenameColumn(name: "UserId", table: "Users", newName: "StudentId");
            migrationBuilder.RenameTable(name: "Users", newName: "Students");

            migrationBuilder.AddColumn<string>(
                name: "ResetPasswordToken",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordExpiry",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(name: "FK_Certificates_Students_StudentId", table: "Certificates", column: "StudentId", principalTable: "Students", principalColumn: "StudentId");
            migrationBuilder.AddForeignKey(name: "FK_CourseReviews_Students_StudentId", table: "CourseReviews", column: "StudentId", principalTable: "Students", principalColumn: "StudentId", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_Enrollments_Students_StudentId", table: "Enrollments", column: "StudentId", principalTable: "Students", principalColumn: "StudentId", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_LessonProgresses_Students_StudentId", table: "LessonProgresses", column: "StudentId", principalTable: "Students", principalColumn: "StudentId", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_Notifications_Students_StudentId", table: "Notifications", column: "StudentId", principalTable: "Students", principalColumn: "StudentId", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_Payments_Students_StudentId", table: "Payments", column: "StudentId", principalTable: "Students", principalColumn: "StudentId", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_QuizAttempts_Students_StudentId", table: "QuizAttempts", column: "StudentId", principalTable: "Students", principalColumn: "StudentId", onDelete: ReferentialAction.Cascade);
        }
    }
}
