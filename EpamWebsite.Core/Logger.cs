using Microsoft.Extensions.Configuration;
using Serilog;

namespace EpamWebsite.Core
{
    public static class Logger
    {
        private static readonly object _lock = new();
        private static bool _initialized = false;

        public static void InitLogger(IConfigurationRoot configuration)
        {
            if (_initialized)
            {
                return;
            }

            lock (_lock)
            {
                if (_initialized)
                {
                    return;
                }
                var logFileName = $"Logs/log-{DateTime.Now:yyyyMMdd_HHmmss_fff}.txt";
                var minLevel = configuration.GetSection("Serilog:MinimumLevel:Default").Value ?? "Information";
                var parsedLevel = (Serilog.Events.LogEventLevel)Enum.Parse(typeof(Serilog.Events.LogEventLevel), minLevel, true);
                var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Scenario}] {Message:lj}{NewLine}{Exception}";

                var loggerConfig = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate: outputTemplate,
                        restrictedToMinimumLevel: parsedLevel)
                    .WriteTo.File(
                        logFileName,
                        outputTemplate: outputTemplate,
                        restrictedToMinimumLevel: parsedLevel
                    );

                Log.Logger = loggerConfig.CreateLogger();

                _initialized = true;
            }
        }

        public static void CloseAndFlush() => Log.CloseAndFlush();
    }
}