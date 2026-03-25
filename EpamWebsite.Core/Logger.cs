using Microsoft.Extensions.Configuration;
using Serilog;

namespace EpamWebsite.Core
{
    public static class Logger
    {
        private static bool _initialized = false;

        public static void InitLogger()
        {
            if (_initialized)
                return;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var logFileName = $"Logs/log-{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.txt";

            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Debug(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    logFileName,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                );

            Log.Logger = loggerConfig.CreateLogger();

            _initialized = true;
        }

        public static void CloseAndFlush() => Log.CloseAndFlush();
    }
}