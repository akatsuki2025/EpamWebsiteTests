using Microsoft.Extensions.Configuration;

namespace EpamWebsite.Core.Configurations;

public class Configuration
{
    public SerilogConfig Serilog { get; set; } = new();
    public WebDriverConfig WebDriver { get; set; } = new();
    public ApiConfig Api { get; set; } = new();

    public static IConfigurationRoot BuildConfiguration(string basePath)
    {
        return new ConfigurationBuilder()
            .SetBasePath(basePath ?? AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public static Configuration FromRoot(IConfigurationRoot configRoot)
    {
        var config = new Configuration();
        configRoot.Bind(config);
        return config;
    }
}