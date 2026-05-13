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

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {

                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var expiredSurveys = dbContext.Surveys
                        .Where(s => s.IsActive == true && s.EndDate != DateTime.MinValue && s.EndDate < DateTime.Now)
                        .ToList();

                    if (expiredSurveys.Any())
                    {
                        foreach (var survey in expiredSurveys)
                        {
                            survey.IsActive = false; 
                        }


                        await dbContext.SaveChangesAsync(stoppingToken);

                        Console.WriteLine($"{expiredSurveys.Count} adet anket pasife alındı.");
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}