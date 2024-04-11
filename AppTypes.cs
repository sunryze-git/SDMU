namespace SDMU;

internal class AppTypes
{
    public static Dictionary<string, Func<Task>> BaseApps { get; } = new()
    {
        ["Aroma"] = Downloader.DownloadAroma,
        ["Tiramisu"] = Downloader.DownloadTiramisu
    };

    public static List<string> AromaPlugins { get; } = new()
    {
        "Padcon",
        "sdcafiine",
        "SwipSwapMe",
        "Inkay",
        "HaltFix",
        "Screenshot"
    };

    public static List<string> ExtraApps { get; } = new()
    {
        "SaveMii",
        "Bloopair",
        "Dumpling",
        "NUSspli",
        "WUDD",
        "WiiUIdent",
        "envSwap",
        "WiiUReboot",
        "WiiUScreenshotManager"
    };

}
