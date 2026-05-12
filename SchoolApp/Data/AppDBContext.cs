using Microsoft.EntityFrameworkCore;
using SchoolApp.Models;

namespace SchoolApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }
        public DbSet<Payment> Payments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Module>(entity =>
            {
                // Sắp xếp Module theo Course + thứ tự
                entity.HasIndex(m => new { m.CourseId, m.OrderIndex })
                      .HasDatabaseName("IX_Module_Course_Order");

                // Quan hệ: Course 1-N Module (xóa Course → xóa hết Module)
                entity.HasOne(m => m.Course)
                      .WithMany(c => c.Modules)
                      .HasForeignKey(m => m.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasIndex(l => new { l.ModuleId, l.OrderIndex })
                      .HasDatabaseName("IX_Lesson_Module_Order");

                // Quan hệ: Module 1-N Lesson
                entity.HasOne(l => l.Module)
                      .WithMany(m => m.Lessons)
                      .HasForeignKey(l => l.ModuleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<Quiz>(entity =>
            {
                // Mỗi Lesson chỉ có tối đa 1 Quiz
                entity.HasIndex(q => q.LessonId)
                      .IsUnique()
                      .HasDatabaseName("IX_Quiz_Lesson_Unique");

                entity.HasOne(q => q.Lesson)
                      .WithOne(l => l.Quiz)
                      .HasForeignKey<Quiz>(q => q.LessonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasIndex(q => new { q.QuizId, q.OrderIndex })
                      .HasDatabaseName("IX_Question_Quiz_Order");

                entity.HasOne(q => q.Quiz)
                      .WithMany(qz => qz.Questions)
                      .HasForeignKey(q => q.QuizId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<AnswerOption>(entity =>
            {
                entity.HasIndex(a => new { a.QuestionId, a.OrderIndex })
                      .HasDatabaseName("IX_AnswerOption_Question_Order");

                entity.HasOne(a => a.Question)
                      .WithMany(q => q.Options)
                      .HasForeignKey(a => a.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<LessonProgress>(entity =>
            {
                // Đã có [Index] attribute trên class, nhưng thêm Fluent API cho chắc
                entity.HasIndex(p => new { p.StudentId, p.LessonId })
                      .IsUnique()
                      .HasDatabaseName("IX_LessonProgress_Student_Lesson");

                entity.HasOne(p => p.Student)
                      .WithMany(s => s.LessonProgresses)
                      .HasForeignKey(p => p.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Lesson)
                      .WithMany(l => l.Progresses)
                      .HasForeignKey(p => p.LessonId)
                      .OnDelete(DeleteBehavior.NoAction); // tránh cascade cycle
            });


            modelBuilder.Entity<QuizAttempt>(entity =>
            {
                // Index để query nhanh: lấy tất cả attempt của 1 student trong 1 quiz
                entity.HasIndex(a => new { a.StudentId, a.QuizId })
                      .HasDatabaseName("IX_QuizAttempt_Student_Quiz");

                entity.HasOne(a => a.Student)
                      .WithMany(s => s.QuizAttempts)
                      .HasForeignKey(a => a.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Quiz)
                      .WithMany(q => q.Attempts)
                      .HasForeignKey(a => a.QuizId)
                      .OnDelete(DeleteBehavior.NoAction); // tránh cascade cycle
            });


            modelBuilder.Entity<QuizAnswer>(entity =>
            {
                // 1 attempt + 1 question = 1 answer (tránh trùng)
                entity.HasIndex(qa => new { qa.QuizAttemptId, qa.QuestionId })
                      .HasDatabaseName("IX_QuizAnswer_Attempt_Question");

                entity.HasOne(qa => qa.Attempt)
                      .WithMany(a => a.Answers)
                      .HasForeignKey(qa => qa.QuizAttemptId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(qa => qa.Question)
                      .WithMany()
                      .HasForeignKey(qa => qa.QuestionId)
                      .OnDelete(DeleteBehavior.NoAction); // tránh cascade cycle

                entity.HasOne(qa => qa.SelectedOption)
                      .WithMany()
                      .HasForeignKey(qa => qa.SelectedOptionId)
                      .OnDelete(DeleteBehavior.NoAction); // tránh cascade cycle
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(p => p.OrderCode)
                      .IsUnique()
                      .HasDatabaseName("IX_Payment_OrderCode");

                entity.HasOne(p => p.Student)
                      .WithMany()
                      .HasForeignKey(p => p.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Course)
                      .WithMany()
                      .HasForeignKey(p => p.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}