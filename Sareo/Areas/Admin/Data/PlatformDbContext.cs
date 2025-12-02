using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq; 

namespace Sareoo.Areas.Admin.Data
{
    public class PlatformDbContext : IdentityDbContext<ApplicationUser>
    {
        public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options) { }


        public DbSet<Stage> Stages { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LiveLesson> LiveLessons { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<EducationalMaterial> EducationalMaterials { get; set; }
        public DbSet<SalesOutlet> SalesOutlets { get; set; }
        public DbSet<TeacherRating> TeacherRatings { get; set; }
        public DbSet<LessonAttachment> LessonAttachments { get; set; }
        public DbSet<LiveLessonReminder> LiveLessonReminders { get; set; }
        public DbSet<LessonQuiz> LessonQuizzes { get; set; }
        public DbSet<LessonQuizQuestion> LessonQuizQuestions { get; set; }
        public DbSet<LessonQuizAttempt> LessonQuizAttempts { get; set; }
        public DbSet<LessonQuizAttemptAnswer> LessonQuizAttemptAnswers { get; set; }
        public DbSet<ExamAttempt> ExamAttempts { get; set; }
        public DbSet<ExamAttemptAnswer> ExamAttemptAnswers { get; set; }
        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            var decimalProps = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => (System.Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType) == typeof(decimal));

            foreach (var property in decimalProps)
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            modelBuilder.Entity<Teacher>()
                .HasMany(t => t.Subjects)
                .WithMany(s => s.Teachers)
                .UsingEntity(j => j.ToTable("TeacherSubjects"));

            modelBuilder.Entity<StudentCourse>()
                .HasKey(sc => new { sc.StudentId, sc.CourseId });

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.StudentCourses)
                .HasForeignKey(sc => sc.CourseId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.StudentCourses)
                .HasForeignKey(sc => sc.StudentId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Exam)
                .WithOne(e => e.Course)
                .HasForeignKey<Exam>(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Course)
                .WithMany(co => co.Certificates)
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Student)
                .WithMany(s => s.Certificates)
                .HasForeignKey(c => c.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherRating>()
                .HasOne(r => r.Teacher)
                .WithMany(t => t.Ratings)
                .HasForeignKey(r => r.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherRating>()
                .HasOne(r => r.Student)
                .WithMany(s => s.GivenRatings)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Student>()
                .HasOne(s => s.ApplicationUser)
                .WithOne()
                .HasForeignKey<Student>(s => s.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.ApplicationUser)
                .WithOne()
                .HasForeignKey<Teacher>(t => t.ApplicationUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Subject>()
                .HasMany(s => s.Grades)
                .WithMany(g => g.Subjects)
                .UsingEntity(j => j.ToTable("SubjectGrades"));

            modelBuilder.Entity<EducationalMaterial>()
                .HasOne(em => em.Grade)
                .WithMany(g => g.EducationalMaterials)
                .HasForeignKey(em => em.GradeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EducationalMaterial>()
                .HasOne(em => em.Subject)
                .WithMany(s => s.EducationalMaterials)
                .HasForeignKey(em => em.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EducationalMaterial>()
                .HasMany(em => em.SalesOutlets)
                .WithMany(so => so.AvailableMaterials)
                .UsingEntity(j => j.ToTable("MaterialSalesOutlets"));

            modelBuilder.Entity<LiveLessonReminder>()
                .HasOne(r => r.Student)
                .WithMany(s => s.LiveLessonReminders)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LiveLessonReminder>()
                .HasOne(r => r.LiveLesson)
                .WithMany(l => l.LiveLessonReminders)
                .HasForeignKey(r => r.LiveLessonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.LessonQuiz)
                .WithOne(q => q.Lesson)
                .HasForeignKey<LessonQuiz>(q => q.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LessonQuizAttempt>()
                .HasOne(a => a.Student)
                .WithMany(s => s.LessonQuizAttempts)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Book>()
                 .HasOne(b => b.Grade)
                 .WithMany()
                 .HasForeignKey(b => b.GradeId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Subject)
                .WithMany()
                .HasForeignKey(b => b.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Teacher)
                .WithMany()
                .HasForeignKey(b => b.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamAttempt>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
       
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamAttempt>()
                .HasOne(a => a.Exam)
                .WithMany()
                .HasForeignKey(a => a.ExamId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ExamAttemptAnswer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LessonQuizAttemptAnswer>()
                .HasOne(a => a.Question) 
                .WithMany()
                .HasForeignKey(a => a.QuestionId) 
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}