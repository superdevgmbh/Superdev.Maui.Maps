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
            //options.Dsn = "TBD";
            options.MinimumEventLevel = LogLevel.Warning;
            options.MinimumBreadcrumbLevel = LogLevel.Debug;
        }
    }
}