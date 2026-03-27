namespace EpamWebsite.Core;

public static class TestDirectoriesHelper
{
    public static string GetDownloadDirectory()
    {
        var runTimestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
        return Path.Combine(
            Directory.GetCurrentDirectory(),
            "EpamDownloads",
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
