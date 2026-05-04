using AnketPortal.API.Data;   // AppDbContext burada!
using AnketPortal.API.Models; // Modellerin (Survey vb.) burada!
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnketPortal.Services
{
    // BackgroundService, .NET'in arka planda sürekli çalışan servisleri için kullanılır
    public class SurveyStatusUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public SurveyStatusUpdateService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Servis durdurulana kadar sonsuz bir döngüde çalışır
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Arka planda çalıştığımız için DbContext'i özel olarak scope içinden çağırıyoruz
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // SİHİRLİ SORGUMUZ: 
                    // 1. Anket Aktif mi?
                    // 2. Bitiş tarihi (EndDate) var mı?
                    // 3. Bitiş tarihi şu anki zamandan (DateTime.Now) daha mı eski?
                    var expiredSurveys = dbContext.Surveys
                        .Where(s => s.IsActive == true && s.EndDate != DateTime.MinValue && s.EndDate < DateTime.Now)
                        .ToList();

                    // Eğer süresi geçen anket bulduysa
                    if (expiredSurveys.Any())
                    {
                        foreach (var survey in expiredSurveys)
                        {
                            survey.IsActive = false; // Acımadan pasife çekiyoruz!
                        }

                        // Değişiklikleri veritabanına kaydet
                        await dbContext.SaveChangesAsync(stoppingToken);

                        // Konsolda görmek istersen:
                        Console.WriteLine($"{expiredSurveys.Count} adet anket pasife alındı.");
                    }
                }

                // BEKLEME SÜRESİ: 10 Saniyede bir kontrol et
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}