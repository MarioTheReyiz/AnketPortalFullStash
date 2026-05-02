using AnketPortal.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AnketPortal.API.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Survey> Surveys { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<SurveyAnswer> SurveyAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            base.OnModelCreating(builder);

            // Anket silindiğinde ona bağlı cevapları otomatik silme
            builder.Entity<SurveyAnswer>()
                .HasOne(sa => sa.Survey)
                .WithMany(s => s.Answers)
                .HasForeignKey(sa => sa.SurveyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Soru silindiğinde ona bağlı cevapları otomatik silme
            builder.Entity<SurveyAnswer>()
                .HasOne(sa => sa.Question)
                .WithMany()
                .HasForeignKey(sa => sa.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}