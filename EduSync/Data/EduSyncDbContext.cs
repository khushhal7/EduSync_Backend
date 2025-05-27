// In Data/EduSyncDbContext.cs
using Microsoft.EntityFrameworkCore;
using EduSync.Models; // Your models namespace

namespace EduSync.Data // Your data access namespace
{
    public class EduSyncDbContext : DbContext
    {
        public EduSyncDbContext(DbContextOptions<EduSyncDbContext> options) : base(options)
        {
        }

        // DbSet properties for each of your models
        // These will be translated into tables in the database
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Here you can add any Fluent API configurations if needed.
            // For example, defining relationships, constraints, or default values
            // that are not easily configured using data annotations in the models.

            // Example: Configure the relationship between User (Instructor) and Course
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany() // If User model had a 'CoursesAuthored' collection: .WithMany(u => u.CoursesAuthored)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict); // Or .Cascade, .SetNull depending on your requirements

            // Example: Configure the relationship between Course and Assessment
            modelBuilder.Entity<Assessment>()
                .HasOne(a => a.Course)
                .WithMany() // If Course model had an 'Assessments' collection: .WithMany(c => c.Assessments)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Often assessments are deleted if the course is deleted

            // Example: Configure the relationship between Assessment and Result
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Assessment)
                .WithMany() // If Assessment model had a 'Results' collection: .WithMany(a => a.Results)
                .HasForeignKey(r => r.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Example: Configure the relationship between User (Student) and Result
            modelBuilder.Entity<Result>()
                .HasOne(r => r.User)
                .WithMany() // If User model had a 'Results' collection: .WithMany(u => u.Results)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Or .Cascade, depending on requirements

            // For the 'Questions' field in Assessment which is a JSON string:
            // By default, EF Core will map it to a string column (e.g., nvarchar(max)).
            // If you needed more specific JSON handling or wanted to map it to a dedicated JSON type
            // in databases that support it (like PostgreSQL jsonb or SQL Server 2016+ JSON support),
            // you might add configurations here, or use value converters.
            // For now, treating it as a simple string that your application code will parse is fine.
        }
    }
}