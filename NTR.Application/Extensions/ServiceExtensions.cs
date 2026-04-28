using Microsoft.Extensions.DependencyInjection;
using NTR.Application.Services;
using NTR.Core.Entities;
using NTR.Core.Interfaces;
using NTR.Infrastructure.Repositories;
using NTR.Infrastructure.Vizrt;

namespace NTR.Application.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            string dataPath,
            VizrtSettings vizrtSettings)
        {
            // Log klasörü
            string logPath = Path.Combine(dataPath, "Logs");
            Directory.CreateDirectory(logPath);

            // Repository'ler
            services.AddSingleton<ILogRepository>(new TxtLogRepository(logPath));
            services.AddSingleton<IHaberRepository>(new JsonHaberRepository(
                Path.Combine(dataPath, "haberler.json")));
            services.AddSingleton<IRundownRepository>(new JsonRundownRepository(
                Path.Combine(dataPath, "rundowns.json")));
            services.AddSingleton<IKjRepository>(new JsonKjRepository(
                Path.Combine(dataPath, "kj_listesi.json")));

            // Services
            services.AddSingleton<LogService>();
            services.AddSingleton<IVizrtService>(provider =>
                new VizrtService(vizrtSettings, provider.GetRequiredService<LogService>()));
            services.AddSingleton<HaberService>();
            services.AddSingleton<RundownService>();
            services.AddSingleton<KjService>();

            return services;
        }
    }
}