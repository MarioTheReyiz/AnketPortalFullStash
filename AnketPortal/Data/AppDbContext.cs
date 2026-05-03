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

        public DbSet<SystemLog> SystemLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Soru silindiğinde cevaplar silinsin (Şelale - Cascade)
            builder.Entity<SurveyAnswer>()
                .HasOne(sa => sa.Question)
                .WithMany()
                .HasForeignKey(sa => sa.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. KÖRDÜĞÜMÜ ÇÖZEN KOD: Kullanıcı ile Cevaplar arasındaki şelaleyi iptal et! (Döngüyü kırar)
            builder.Entity<SurveyAnswer>()
                .HasOne(sa => sa.AppUser)
                .WithMany()
                .HasForeignKey(sa => sa.AppUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}