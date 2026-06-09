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
        public DbSet<User> Users { get; set; }
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
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasIndex(c => c.CourseName)
                      .IsUnique()
                      .HasDatabaseName("IX_Course_Name_Unique");
            });

            modelBuilder.Entity<Module>(entity =>
            {
                entity.HasIndex(m => new { m.CourseId, m.OrderIndex })
                      .HasDatabaseName("IX_Module_Course_Order");

                entity.HasOne(m => m.Course)
                      .WithMany(c => c.Modules)
                      .HasForeignKey(m => m.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasIndex(l => new { l.ModuleId, l.OrderIndex })
                      .HasDatabaseName("IX_Lesson_Module_Order");

                entity.HasOne(l => l.Module)
                      .WithMany(m => m.Lessons)
                      .HasForeignKey(l => l.ModuleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<Quiz>(entity =>
            {
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
                entity.HasIndex(p => new { p.UserId, p.LessonId })
                      .IsUnique()
                      .HasDatabaseName("IX_LessonProgress_User_Lesson");

                entity.HasOne(p => p.User)
                      .WithMany(u => u.LessonProgresses)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Lesson)
                      .WithMany(l => l.Progresses)
                      .HasForeignKey(p => p.LessonId)
                      .OnDelete(DeleteBehavior.NoAction);
            });


            modelBuilder.Entity<QuizAttempt>(entity =>
            {
                entity.HasIndex(a => new { a.UserId, a.QuizId })
                      .HasDatabaseName("IX_QuizAttempt_User_Quiz");

                entity.HasOne(a => a.User)
                      .WithMany(u => u.QuizAttempts)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Quiz)
                      .WithMany(q => q.Attempts)
                      .HasForeignKey(a => a.QuizId)
                      .OnDelete(DeleteBehavior.NoAction);
            });


            modelBuilder.Entity<QuizAnswer>(entity =>
            {
                entity.HasIndex(qa => new { qa.QuizAttemptId, qa.QuestionId })
                      .HasDatabaseName("IX_QuizAnswer_Attempt_Question");

                entity.HasOne(qa => qa.Attempt)
                      .WithMany(a => a.Answers)
                      .HasForeignKey(qa => qa.QuizAttemptId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(qa => qa.Question)
                      .WithMany()
                      .HasForeignKey(qa => qa.QuestionId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(qa => qa.SelectedOption)
                      .WithMany()
                      .HasForeignKey(qa => qa.SelectedOptionId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(p => p.OrderCode)
                      .IsUnique()
                      .HasDatabaseName("IX_Payment_OrderCode");

                entity.HasOne(p => p.User)
                      .WithMany()
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Course)
                      .WithMany()
                      .HasForeignKey(p => p.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Certificate>(entity =>
            {
                entity.HasIndex(c => c.CertificateCode)
                      .IsUnique()
                      .HasDatabaseName("IX_Certificate_Code_Unique");

                entity.HasIndex(c => c.EnrollmentId)
                      .IsUnique()
                      .HasDatabaseName("IX_Certificate_Enrollment_Unique");

                entity.HasOne(c => c.Enrollment)
                      .WithMany()
                      .HasForeignKey(c => c.EnrollmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.Course)
                      .WithMany()
                      .HasForeignKey(c => c.CourseId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<CourseReview>(entity =>
            {
                entity.HasIndex(r => new { r.CourseId, r.UserId })
                      .IsUnique()
                      .HasDatabaseName("IX_CourseReview_Course_User");

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Course)
                      .WithMany()
                      .HasForeignKey(r => r.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasIndex(n => new { n.UserId, n.IsRead })
                      .HasDatabaseName("IX_Notification_User_Read");

                entity.HasOne(n => n.User)
                      .WithMany()
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
