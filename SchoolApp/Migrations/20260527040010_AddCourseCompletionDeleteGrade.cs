using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseCompletionDeleteGrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop Grade if it still exists
            migrationBuilder.Sql(@"
                DECLARE @v sysname;
                SELECT @v = [d].[name]
                FROM [sys].[default_constraints] [d]
                INNER JOIN [sys].[columns] [c]
                    ON [d].[parent_column_id] = [c].[column_id]
                    AND [d].[parent_object_id] = [c].[object_id]
                WHERE [d].[parent_object_id] = OBJECT_ID(N'[Enrollments]')
                  AND [c].[name] = N'Grade';
                IF @v IS NOT NULL EXEC(N'ALTER TABLE [Enrollments] DROP CONSTRAINT [' + @v + '];');
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'Grade'
                )
                    ALTER TABLE [Enrollments] DROP COLUMN [Grade];
            ");

            // Add CompletedAt if not present
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'CompletedAt'
                )
                    ALTER TABLE [Enrollments] ADD [CompletedAt] datetime2 NULL;
            ");

            // Add IsCompleted if not present
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'IsCompleted'
                )
                    ALTER TABLE [Enrollments] ADD [IsCompleted] bit NOT NULL DEFAULT CAST(0 AS bit);
            ");

            // Create Payments table if not present
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Payments]') AND type = 'U')
                BEGIN
                    CREATE TABLE [Payments] (
                        [PaymentId]  int          NOT NULL IDENTITY,
                        [StudentId]  int          NOT NULL,
                        [CourseId]   int          NOT NULL,
                        [OrderCode]  bigint       NOT NULL,
                        [Amount]     decimal(18,0) NOT NULL,
                        [Status]     int          NOT NULL,
                        [CreatedAt]  datetime2    NOT NULL,
                        [PaidAt]     datetime2    NULL,
                        CONSTRAINT [PK_Payments] PRIMARY KEY ([PaymentId]),
                        CONSTRAINT [FK_Payments_Courses_CourseId]
                            FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([CourseId]),
                        CONSTRAINT [FK_Payments_Students_StudentId]
                            FOREIGN KEY ([StudentId]) REFERENCES [Students] ([StudentId])
                    );
                    CREATE UNIQUE INDEX [IX_Payment_OrderCode]  ON [Payments] ([OrderCode]);
                    CREATE        INDEX [IX_Payments_CourseId]  ON [Payments] ([CourseId]);
                    CREATE        INDEX [IX_Payments_StudentId] ON [Payments] ([StudentId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Enrollments");

            migrationBuilder.AddColumn<decimal>(
                name: "Grade",
                table: "Enrollments",
                type: "decimal(4,2)",
                nullable: true);
        }
    }
}
