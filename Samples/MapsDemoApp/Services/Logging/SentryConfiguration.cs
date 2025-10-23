using Microsoft.Extensions.Logging;
using Sentry.Extensions.Logging;
using Sentry.Maui;

namespace MapsDemoApp.Services.Logging
{
    public static class SentryConfiguration
    {
        public static void Configure(SentryLoggingOptions options)
        {
            options.InitializeSdk = true;
            options.Debug = false;
            options.Dsn = "https://6a656feef8c47b176c0faf814a79ff85@o4507458300280832.ingest.de.sentry.io/4510240104448080";
            options.MinimumEventLevel = LogLevel.Warning;
            options.MinimumBreadcrumbLevel = LogLevel.Debug;
        }
    }
}