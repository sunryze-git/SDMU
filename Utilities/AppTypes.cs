namespace SDMU.Utilities;

internal class AppTypes
{
    public static Dictionary<string, Func<Task>> BaseApps { get; } = new()
    {
        ["Aroma"] = Downloader.DownloadAroma,
        ["Tiramisu"] = Downloader.DownloadTiramisu
    };

    public static List<string> ExtraApps { get; } = Downloader.GetPackagesByCategory("aroma").Result.Select(x => x.Name).ToList();

}
