using Microsoft.Extensions.Configuration;

namespace EpamWebsite.Core;

public class Configuration
{
    public SerilogConfig Serilog { get; set; } = new();
    public WebDriverConfig WebDriver { get; set; } = new();

    public static Configuration Load(string basePath)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var configRoot = builder.Build();
        var config = new Configuration();
        configRoot.Bind(config);
        return config;
    }
}

public class SerilogConfig
{
    public MinimumLevelConfig MinimumLevel { get; set; } = new();
}

public class MinimumLevelConfig
{
    public string Default { get; set; } = "Information";
}

public class WebDriverConfig
{
    public string Browser { get; set; } = "Chrome";
}