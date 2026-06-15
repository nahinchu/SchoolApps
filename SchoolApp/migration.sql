IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410030843_InitDB'
)
BEGIN
    CREATE TABLE [Courses] (
        [CourseId] int NOT NULL IDENTITY,
        [CourseName] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [Credits] int NOT NULL,
        [Fee] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([CourseId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410030843_InitDB'
)
BEGIN
    CREATE TABLE [Students] (
        [StudentId] int NOT NULL IDENTITY,
        [FullName] nvarchar(100) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [Phone] nvarchar(15) NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [Address] nvarchar(300) NOT NULL,
        [RegisteredDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([StudentId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410030843_InitDB'
)
BEGIN
    CREATE TABLE [Enrollments] (
        [EnrollmentId] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [CourseId] int NOT NULL,
        [EnrollDate] datetime2 NOT NULL,
        [Grade] decimal(4,2) NULL,
        [Notes] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY ([EnrollmentId]),
        CONSTRAINT [FK_Enrollments_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([CourseId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Enrollments_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([StudentId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410030843_InitDB'
)
BEGIN
    CREATE INDEX [IX_Enrollments_CourseId] ON [Enrollments] ([CourseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410030843_InitDB'
)
BEGIN
    CREATE INDEX [IX_Enrollments_StudentId] ON [Enrollments] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410030843_InitDB'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410030843_InitDB', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410034650_AddPasswordForStudent'
)
BEGIN
    ALTER TABLE [Students] ADD [Password] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410034650_AddPasswordForStudent'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410034650_AddPasswordForStudent', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413070637_MakeNotesNullable'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Enrollments]') AND [c].[name] = N'Notes');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Enrollments] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Enrollments] ALTER COLUMN [Notes] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413070637_MakeNotesNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260413070637_MakeNotesNullable', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506035438_AddRoleToStudent'
)
BEGIN
    ALTER TABLE [Students] ADD [Role] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506035438_AddRoleToStudent'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260506035438_AddRoleToStudent', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506073138_AllowNull'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'Address');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Students] ALTER COLUMN [Address] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506073138_AllowNull'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Courses]') AND [c].[name] = N'Description');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Courses] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Courses] ALTER COLUMN [Description] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506073138_AllowNull'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260506073138_AllowNull', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE TABLE [Modules] (
        [ModuleId] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [OrderIndex] int NOT NULL,
        [IsPublished] bit NOT NULL,
        [CourseId] int NOT NULL,
        CONSTRAINT [PK_Modules] PRIMARY KEY ([ModuleId]),
        CONSTRAINT [FK_Modules_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([CourseId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE TABLE [Lessons] (
        [LessonId] int NOT NULL IDENTITY,
        [Title] nvarchar(300) NOT NULL,
        [Type] int NOT NULL,
        [VideoUrl] nvarchar(500) NULL,
        [HtmlContent] nvarchar(max) NULL,
        [AttachmentPath] nvarchar(500) NULL,
        [DurationMinutes] int NOT NULL,
        [OrderIndex] int NOT NULL,
        [IsPublished] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ModuleId] int NOT NULL,
        CONSTRAINT [PK_Lessons] PRIMARY KEY ([LessonId]),
        CONSTRAINT [FK_Lessons_Modules_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [Modules] ([ModuleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE TABLE [LessonProgresses] (
        [LessonProgressId] int NOT NULL IDENTITY,
        [Status] int NOT NULL,
        [ProgressPercent] int NOT NULL,
        [CompletedAt] datetime2 NULL,
        [LastAccessedAt] datetime2 NOT NULL,
        [StudentId] int NOT NULL,
        [LessonId] int NOT NULL,
        CONSTRAINT [PK_LessonProgresses] PRIMARY KEY ([LessonProgressId]),
        CONSTRAINT [FK_LessonProgresses_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([LessonId]),
        CONSTRAINT [FK_LessonProgresses_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([StudentId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE TABLE [Quizzes] (
        [QuizId] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NULL,
        [PassingScore] int NOT NULL,
        [TimeLimitMinutes] int NOT NULL,
        [AllowRetry] bit NOT NULL,
        [MaxAttempts] int NOT NULL,
        [ShowAnswersAfterSubmit] bit NOT NULL,
        [IsPublished] bit NOT NULL,
        [LessonId] int NOT NULL,
        CONSTRAINT [PK_Quizzes] PRIMARY KEY ([QuizId]),
        CONSTRAINT [FK_Quizzes_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([LessonId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE TABLE [Questions] (
        [QuestionId] int NOT NULL IDENTITY,
        [Content] nvarchar(2000) NOT NULL,
        [Explanation] nvarchar(1000) NULL,
        [Type] int NOT NULL,
        [Points] int NOT NULL,
        [OrderIndex] int NOT NULL,
        [QuizId] int NOT NULL,
        CONSTRAINT [PK_Questions] PRIMARY KEY ([QuestionId]),
        CONSTRAINT [FK_Questions_Quizzes_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [Quizzes] ([QuizId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE TABLE [QuizAttempts] (
        [QuizAttemptId] int NOT NULL IDENTITY,
        [StartedAt] datetime2 NOT NULL,
        [FinishedAt] datetime2 NULL,
        [Score] int NOT NULL,
        [MaxScore] int NOT NULL,
        [Passed] bit NOT NULL,
        [AttemptNumber] int NOT NULL,
        [StudentId] int NOT NULL,
        [QuizId] int NOT NULL,
        CONSTRAINT [PK_QuizAttempts] PRIMARY KEY ([QuizAttemptId]),
        CONSTRAINT [FK_QuizAttempts_Quizzes_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [Quizzes] ([QuizId]),
        CONSTRAINT [FK_QuizAttempts_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([StudentId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE TABLE [AnswerOptions] (
        [AnswerOptionId] int NOT NULL IDENTITY,
        [Content] nvarchar(1000) NOT NULL,
        [IsCorrect] bit NOT NULL,
        [OrderIndex] int NOT NULL,
        [QuestionId] int NOT NULL,
        CONSTRAINT [PK_AnswerOptions] PRIMARY KEY ([AnswerOptionId]),
        CONSTRAINT [FK_AnswerOptions_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([QuestionId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE TABLE [QuizAnswers] (
        [QuizAnswerId] int NOT NULL IDENTITY,
        [IsCorrect] bit NOT NULL,
        [QuizAttemptId] int NOT NULL,
        [QuestionId] int NOT NULL,
        [SelectedOptionId] int NOT NULL,
        CONSTRAINT [PK_QuizAnswers] PRIMARY KEY ([QuizAnswerId]),
        CONSTRAINT [FK_QuizAnswers_AnswerOptions_SelectedOptionId] FOREIGN KEY ([SelectedOptionId]) REFERENCES [AnswerOptions] ([AnswerOptionId]),
        CONSTRAINT [FK_QuizAnswers_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([QuestionId]),
        CONSTRAINT [FK_QuizAnswers_QuizAttempts_QuizAttemptId] FOREIGN KEY ([QuizAttemptId]) REFERENCES [QuizAttempts] ([QuizAttemptId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE INDEX [IX_AnswerOption_Question_Order] ON [AnswerOptions] ([QuestionId], [OrderIndex]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LessonProgress_Student_Lesson] ON [LessonProgresses] ([StudentId], [LessonId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE INDEX [IX_LessonProgresses_LessonId] ON [LessonProgresses] ([LessonId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE INDEX [IX_Lesson_Module_Order] ON [Lessons] ([ModuleId], [OrderIndex]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE INDEX [IX_Module_Course_Order] ON [Modules] ([CourseId], [OrderIndex]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE INDEX [IX_Question_Quiz_Order] ON [Questions] ([QuizId], [OrderIndex]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE UNIQUE INDEX [IX_QuizAnswer_Attempt_Question] ON [QuizAnswers] ([QuizAttemptId], [QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE INDEX [IX_QuizAnswers_QuestionId] ON [QuizAnswers] ([QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE INDEX [IX_QuizAnswers_SelectedOptionId] ON [QuizAnswers] ([SelectedOptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE INDEX [IX_QuizAttempt_Student_Quiz] ON [QuizAttempts] ([StudentId], [QuizId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE INDEX [IX_QuizAttempts_QuizId] ON [QuizAttempts] ([QuizId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Quiz_Lesson_Unique] ON [Quizzes] ([LessonId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506095857_ScaleUp'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260506095857_ScaleUp', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508031415_DropQuizAnswerUniqueIndex'
)
BEGIN
    DROP INDEX [IX_QuizAnswer_Attempt_Question] ON [QuizAnswers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508031415_DropQuizAnswerUniqueIndex'
)
BEGIN
    CREATE INDEX [IX_QuizAnswer_Attempt_Question] ON [QuizAnswers] ([QuizAttemptId], [QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508031415_DropQuizAnswerUniqueIndex'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260508031415_DropQuizAnswerUniqueIndex', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508074250_AllowNullableQuizId'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Questions]') AND [c].[name] = N'QuizId');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Questions] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Questions] ALTER COLUMN [QuizId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508074250_AllowNullableQuizId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260508074250_AllowNullableQuizId', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508074848_AddTagToQuestion'
)
BEGIN
    ALTER TABLE [Questions] ADD [Tag] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508074848_AddTagToQuestion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260508074848_AddTagToQuestion', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512063248_AddPaymentModel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260512063248_AddPaymentModel', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527040010_AddCourseCompletionDeleteGrade'
)
BEGIN

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
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527040010_AddCourseCompletionDeleteGrade'
)
BEGIN

                    IF NOT EXISTS (
                        SELECT 1 FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'CompletedAt'
                    )
                        ALTER TABLE [Enrollments] ADD [CompletedAt] datetime2 NULL;
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527040010_AddCourseCompletionDeleteGrade'
)
BEGIN

                    IF NOT EXISTS (
                        SELECT 1 FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'IsCompleted'
                    )
                        ALTER TABLE [Enrollments] ADD [IsCompleted] bit NOT NULL DEFAULT CAST(0 AS bit);
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527040010_AddCourseCompletionDeleteGrade'
)
BEGIN

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
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527040010_AddCourseCompletionDeleteGrade'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260527040010_AddCourseCompletionDeleteGrade', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529030054_RefactorCourse'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM sys.columns
                                   WHERE object_id = OBJECT_ID(N'[Courses]') AND name = 'Level')
                        ALTER TABLE [Courses] ADD [Level] int NOT NULL DEFAULT 0;
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529030054_RefactorCourse'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM sys.columns
                                   WHERE object_id = OBJECT_ID(N'[Courses]') AND name = 'ThumbnailUrl')
                        ALTER TABLE [Courses] ADD [ThumbnailUrl] nvarchar(500) NULL;
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529030054_RefactorCourse'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM sys.columns
                                   WHERE object_id = OBJECT_ID(N'[Courses]') AND name = 'UpdatedDate')
                        ALTER TABLE [Courses] ADD [UpdatedDate] datetime2 NULL;
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529030054_RefactorCourse'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM sys.indexes
                                   WHERE object_id = OBJECT_ID(N'[Courses]') AND name = 'IX_Course_Name_Unique')
                        CREATE UNIQUE INDEX [IX_Course_Name_Unique] ON [Courses] ([CourseName]);
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529030054_RefactorCourse'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529030054_RefactorCourse', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601023139_AddCertificate'
)
BEGIN
    CREATE TABLE [Certificates] (
        [CertificateId] int NOT NULL IDENTITY,
        [EnrollmentId] int NOT NULL,
        [StudentId] int NOT NULL,
        [CourseId] int NOT NULL,
        [CertificateCode] nvarchar(36) NOT NULL,
        [IssuedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Certificates] PRIMARY KEY ([CertificateId]),
        CONSTRAINT [FK_Certificates_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([CourseId]),
        CONSTRAINT [FK_Certificates_Enrollments_EnrollmentId] FOREIGN KEY ([EnrollmentId]) REFERENCES [Enrollments] ([EnrollmentId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Certificates_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([StudentId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601023139_AddCertificate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Certificate_Code_Unique] ON [Certificates] ([CertificateCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601023139_AddCertificate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Certificate_Enrollment_Unique] ON [Certificates] ([EnrollmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601023139_AddCertificate'
)
BEGIN
    CREATE INDEX [IX_Certificates_CourseId] ON [Certificates] ([CourseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601023139_AddCertificate'
)
BEGIN
    CREATE INDEX [IX_Certificates_StudentId] ON [Certificates] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601023139_AddCertificate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260601023139_AddCertificate', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601072747_AddReviewsAndNotifications'
)
BEGIN
    CREATE TABLE [Certificates] (
        [CertificateId] int NOT NULL IDENTITY,
        [EnrollmentId] int NOT NULL,
        [StudentId] int NOT NULL,
        [CourseId] int NOT NULL,
        [CertificateCode] nvarchar(36) NOT NULL,
        [IssuedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Certificates] PRIMARY KEY ([CertificateId]),
        CONSTRAINT [FK_Certificates_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([CourseId]),
        CONSTRAINT [FK_Certificates_Enrollments_EnrollmentId] FOREIGN KEY ([EnrollmentId]) REFERENCES [Enrollments] ([EnrollmentId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Certificates_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([StudentId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601072747_AddReviewsAndNotifications'
)
BEGIN
    CREATE TABLE [CourseReviews] (
        [ReviewId] int NOT NULL IDENTITY,
        [CourseId] int NOT NULL,
        [StudentId] int NOT NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CourseReviews] PRIMARY KEY ([ReviewId]),
        CONSTRAINT [FK_CourseReviews_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([CourseId]) ON DELETE CASCADE,
        CONSTRAINT [FK_CourseReviews_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([StudentId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601072747_AddReviewsAndNotifications'
)
BEGIN
    CREATE TABLE [Notifications] (
        [NotificationId] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(500) NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [Link] nvarchar(300) NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationId]),
        CONSTRAINT [FK_Notifications_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([StudentId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601072747_AddReviewsAndNotifications'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Certificate_Code_Unique] ON [Certificates] ([CertificateCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601072747_AddReviewsAndNotifications'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Certificate_Enrollment_Unique] ON [Certificates] ([EnrollmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601072747_AddReviewsAndNotifications'
)
BEGIN
    CREATE INDEX [IX_Certificates_CourseId] ON [Certificates] ([CourseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601072747_AddReviewsAndNotifications'
)
BEGIN
    CREATE INDEX [IX_Certificates_StudentId] ON [Certificates] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601072747_AddReviewsAndNotifications'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CourseReview_Course_Student] ON [CourseReviews] ([CourseId], [StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601072747_AddReviewsAndNotifications'
)
BEGIN
    CREATE INDEX [IX_CourseReviews_StudentId] ON [CourseReviews] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601072747_AddReviewsAndNotifications'
)
BEGIN
    CREATE INDEX [IX_Notification_Student_Read] ON [Notifications] ([StudentId], [IsRead]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601072747_AddReviewsAndNotifications'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260601072747_AddReviewsAndNotifications', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609022421_AddGoogleIdToStudent'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'Password');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Students] ALTER COLUMN [Password] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609022421_AddGoogleIdToStudent'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260609022421_AddGoogleIdToStudent', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609065731_AddResetPasswordToken'
)
BEGIN
    ALTER TABLE [Students] ADD [ResetPasswordExpiry] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609065731_AddResetPasswordToken'
)
BEGIN
    ALTER TABLE [Students] ADD [ResetPasswordToken] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609065731_AddResetPasswordToken'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260609065731_AddResetPasswordToken', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [Certificates] DROP CONSTRAINT [FK_Certificates_Students_StudentId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [CourseReviews] DROP CONSTRAINT [FK_CourseReviews_Students_StudentId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [Enrollments] DROP CONSTRAINT [FK_Enrollments_Students_StudentId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [LessonProgresses] DROP CONSTRAINT [FK_LessonProgresses_Students_StudentId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [Notifications] DROP CONSTRAINT [FK_Notifications_Students_StudentId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [Payments] DROP CONSTRAINT [FK_Payments_Students_StudentId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [QuizAttempts] DROP CONSTRAINT [FK_QuizAttempts_Students_StudentId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'ResetPasswordToken');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Students] DROP COLUMN [ResetPasswordToken];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'ResetPasswordExpiry');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Students] DROP COLUMN [ResetPasswordExpiry];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[Students]', N'Users';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[Users].[StudentId]', N'UserId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [Users] ADD [IsEmailVerified] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'FullName');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Users] ALTER COLUMN [FullName] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[QuizAttempts].[StudentId]', N'UserId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[QuizAttempts].[IX_QuizAttempt_Student_Quiz]', N'IX_QuizAttempt_User_Quiz', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[Payments].[StudentId]', N'UserId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[Payments].[IX_Payments_StudentId]', N'IX_Payments_UserId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[Notifications].[StudentId]', N'UserId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[Notifications].[IX_Notification_Student_Read]', N'IX_Notification_User_Read', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[LessonProgresses].[StudentId]', N'UserId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[LessonProgresses].[IX_LessonProgress_Student_Lesson]', N'IX_LessonProgress_User_Lesson', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[Enrollments].[StudentId]', N'UserId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[Enrollments].[IX_Enrollments_StudentId]', N'IX_Enrollments_UserId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[CourseReviews].[StudentId]', N'UserId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[CourseReviews].[IX_CourseReviews_StudentId]', N'IX_CourseReviews_UserId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[CourseReviews].[IX_CourseReview_Course_Student]', N'IX_CourseReview_Course_User', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[Certificates].[StudentId]', N'UserId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    EXEC sp_rename N'[Certificates].[IX_Certificates_StudentId]', N'IX_Certificates_UserId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [Certificates] ADD CONSTRAINT [FK_Certificates_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [CourseReviews] ADD CONSTRAINT [FK_CourseReviews_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [Enrollments] ADD CONSTRAINT [FK_Enrollments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [LessonProgresses] ADD CONSTRAINT [FK_LessonProgresses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [Notifications] ADD CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    ALTER TABLE [QuizAttempts] ADD CONSTRAINT [FK_QuizAttempts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609085429_RenameStudentToUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260609085429_RenameStudentToUser', N'8.0.25');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609091951_AddAvatarUrlToUser'
)
BEGIN
    ALTER TABLE [Users] ADD [AvatarUrl] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609091951_AddAvatarUrlToUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260609091951_AddAvatarUrlToUser', N'8.0.25');
END;
GO

COMMIT;
GO

