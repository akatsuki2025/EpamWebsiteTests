namespace EpamWebsite.Core;

public static class TestDirectoriesHelper
{
    public static string GetDownloadDirectory()
    {
        var runTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        var uniqueId = Guid.NewGuid().ToString("N");
        return Path.Combine(
            Directory.GetCurrentDirectory(),
            "EpamDownloads",
            $"{runTimestamp}_{uniqueId}");
    }

    public static string GetScreenshotDirectory()
    {
        var runTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        return Path.Combine(projectRoot, "EpamScreenshots", runTimestamp);
    }

    public static void DeleteDirectoryIfExists(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
