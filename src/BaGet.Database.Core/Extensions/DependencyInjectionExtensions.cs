using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BaGet.Core.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddBaGetDbContextProvider<TContext>(
            this IServiceCollection services,
            string databaseType,
            Action<IServiceProvider, DbContextOptionsBuilder> configureContext)
            where TContext : DbContext, IContext
        {

            services.AddDbContext<TContext>(configureContext);
            services.AddBaGetGenericContextProvider<TContext>(databaseType);
            services.TryAddTransient<DatabaseSearchService>();
            services.TryAddTransient<ISearchService>(provider => provider.GetRequiredService<DatabaseSearchService>());

            services.AddProvider<ISearchService>((provider, config) =>
            {
                if (!config.HasSearchType("Database")) return null;
                if (!config.HasDatabaseType(databaseType)) return null;

                return provider.GetRequiredService<DatabaseSearchService>();
            });
            return services;
        }
    }
}
