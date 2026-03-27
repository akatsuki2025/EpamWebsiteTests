using Microsoft.Extensions.Configuration;

namespace EpamWebsite.Core;

public class Configuration
{
    public SerilogConfig Serilog { get; set; } = new();
    public WebDriverConfig WebDriver { get; set; } = new();

    public static IConfigurationRoot BuildConfiguration(string basePath)
    {
        return new ConfigurationBuilder()
            .SetBasePath(basePath ?? AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    public static Configuration FromRoot(IConfigurationRoot configRoot)
    {
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