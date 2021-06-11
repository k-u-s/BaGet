using System;
using BaGet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BaGet.Database.RavenDb
{
    public static class RavenApplicationExtensions
    {
        public static BaGetApplication AddRavenDatabase(this BaGetApplication app)
        {
            app.Services.TryAddTransient<RavenSearchService>();
            app.Services.AddProvider<ISearchService>((provider, config) =>
            {
                if (!config.HasSearchType("Database")) return null;
                if (!config.HasDatabaseType("Raven")) return null;

                return provider.GetRequiredService<RavenSearchService>();
            });

            app.Services.AddScoped<RavenContext>();
            app.Services.AddBaGetGenericContextProvider<RavenContext>("RavenDb");
            return app;
        }

        public static BaGetApplication AddRavenDatabase(
            this BaGetApplication app,
            Action<DatabaseOptions> configure)
        {
            app.AddRavenDatabase();
            app.Services.Configure(configure);
            return app;
        }

        public static BaGetApplication AddRavenStorage(this BaGetApplication app)
        {
            app.Services.AddTransient<RavenStorage>();
            app.Services.TryAddTransient<IStorageService>(provider => provider.GetRequiredService<RavenStorage>());
            return app;
        }

        public static BaGetApplication AddRavenStorage(
            this BaGetApplication app,
            Action<FileSystemStorageOptions> configure)
        {
            app.AddRavenStorage();
            app.Services.Configure(configure);
            return app;
        }
    }
}
