namespace EpamWebsite.Core;

public static class TestDirectories
{
    public static string GetDownloadDirectory()
    {
        var runTimestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
        return Path.Combine(
            Directory.GetCurrentDirectory(),
            "EpamDownloads",
            runTimestamp);
    }
}
