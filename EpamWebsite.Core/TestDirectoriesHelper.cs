namespace EpamWebsite.Core;

public static class TestDirectoriesHelper
{
    public static string GetDownloadDirectory()
    {
        var runTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        return Path.Combine(
            Directory.GetCurrentDirectory(),
            "EpamDownloads",
            runTimestamp);
    }

    public static string GetScreenshotDirectory()
    {
        var runTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        return Path.Combine(
            Directory.GetCurrentDirectory(),
            "EpamScreenshots",
            runTimestamp);
    }

    public static void DeleteDirectoryIfExists(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
