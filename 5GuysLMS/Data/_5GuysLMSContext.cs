using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using _5GuysLMS.Models;
using _5GuysLMS.Data.Configurations;

namespace _5GuysLMS.Data
{
    public class _5GuysLMSContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<ProfilePicture> ProfilePictures { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<TextSubmission> TextSubmissions { get; set; }
        public DbSet<FileSubmission> FileSubmissions { get; set; }
        public DbSet<SecurityQuestion> SecurityQuestions { get; set; }
        public DbSet<SecurityQuestionAnswer> SecurityQuestionsAnswers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CreatedNotification> CreatedNotifications { get; set; }
        public DbSet<GradedNotification> GradedNotifications { get; set; }
        public _5GuysLMSContext (DbContextOptions<_5GuysLMSContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            modelBuilder.ApplyConfiguration(new ProfilePictureConfiguration());
            
        }
       
    }
}
