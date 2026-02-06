using SDMU.NewFramework;

namespace SDMU.Utilities;

internal class AppTypes(Downloader downloader)
{
    public Dictionary<string, Func<Task>> BaseApps { get; } = new()
    {
        ["Aroma"] = downloader.DownloadAroma,
        ["Tiramisu"] = downloader.DownloadTiramisu
    };

    public List<string> ExtraApps { get; } = 
        downloader.GetPackages("aroma").Result
            .Where(x => x.Name is not null)
            .Select(x => x.Name)
            .OfType<string>()
            .ToList();
}
