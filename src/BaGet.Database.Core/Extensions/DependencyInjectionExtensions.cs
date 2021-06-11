using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
            return services;
        }
    }
}
