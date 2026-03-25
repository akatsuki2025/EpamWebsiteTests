using Microsoft.Extensions.Configuration;
using Serilog;

namespace EpamWebsite.Core
{
    public static class Logger
    {
        public static void InitLogger()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var minLevel = configuration["Logging:MinimumLevel"] ?? "Information";

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var logFileName = $"Logs/log-{timestamp}.txt";

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(Enum.TryParse(minLevel, out Serilog.Events.LogEventLevel level) ? level : Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.File(
                    logFileName,
                    rollingInterval: RollingInterval.Infinite,
                    shared: false);

            Log.Logger = loggerConfig.CreateLogger();
        }

        public static void CloseAndFlush() => Log.CloseAndFlush();
    }
}