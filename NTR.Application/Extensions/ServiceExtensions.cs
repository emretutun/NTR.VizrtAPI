using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NTR.Application.Services;
using NTR.Core.Interfaces;
using NTR.Infrastructure.Repositories;
using NTR.Infrastructure.Vizrt;

namespace NTR.Application.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            string dataPath)
        {
            // Repository'ler
            services.AddSingleton<IHaberRepository>(new JsonHaberRepository(
                Path.Combine(dataPath, "haberler.json")));

            services.AddSingleton<IRundownRepository>(new JsonRundownRepository(
                Path.Combine(dataPath, "rundowns.json")));

            services.AddSingleton<IKjRepository>(new JsonKjRepository(
                Path.Combine(dataPath, "kj_listesi.json")));

            // Services
            services.AddSingleton<IVizrtService, VizrtService>();
            services.AddSingleton<HaberService>();
            services.AddSingleton<RundownService>();
            services.AddSingleton<KjService>();

            return services;
        }
    }
}