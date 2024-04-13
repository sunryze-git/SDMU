using SDMU.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SDMU.NewFramework;

internal class Package
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("binary")]
    public string? Binary { get; set; }

    [JsonPropertyName("updated")]
    public string? Updated { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("license")]
    public string? License { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("changelog")]
    public string? Changelog { get; set; }

    [JsonPropertyName("screens")]
    public int Screens { get; set; }

    [JsonPropertyName("extracted")]
    public int Extracted { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("filesize")]
    public int Filesize { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }

    [JsonPropertyName("app_dls")]
    public int AppDls { get; set; }

    [JsonPropertyName("md5")]
    public string? Md5 { get; set; }
}

internal class RootObject
{
    [JsonPropertyName("packages")]
    public IEnumerable<Package>? Packages { get; set; }
}


internal class Downloader
{
    private readonly static string _repo = "https://wiiu.cdn.fortheusers.org/repo.json";
    private readonly static string _dlRepo = "https://wiiu.cdn.fortheusers.org/zips/";

    private static string _downloadPath = Path.GetTempPath();

    private static void WriteMetadata(Package package)
    {
        var json = JsonSerializer.Serialize(package);
        var targetDrive = MediaDevice.Device?.Name;

        if (targetDrive is null)
        {
            throw new Exception("No target drive found!");
        }

        var metadataPath = $"{targetDrive}\\metadata";

        // Create path if not exist
        if (!Directory.Exists(metadataPath))
        {
            Directory.CreateDirectory(metadataPath);
        }

        File.WriteAllText($"{metadataPath}\\{package.Name}.json", json);
    }

    internal static async Task<Package[]> GetPackages(string? category = null)
    {
        var client = new HttpClient();
        var json = await client.GetStringAsync(_repo);

        RootObject? root = JsonSerializer.Deserialize<RootObject>(json);

        if (root is null)
        {
            throw new Exception("Failed to get packages!");
        }

        if (root.Packages is null)
        {
            throw new Exception("No packages found!");
        }
        if (category is not null)
        {
            return root.Packages.Where(x => x.Category?.ToLower() == category.ToLower()).ToArray();
        }
        return root.Packages.ToArray();
    }

    internal static async Task DownloadPackage(string packageName)
    {
        if (MediaDevice.Device is null)
        {
            throw new Exception("No target drive found!");
        }

        var packages = await GetPackages();
        var package = packages.FirstOrDefault(x => x.Name?.ToLower() == packageName.ToLower());

        if (package is null)
        {
            throw new Exception("Package not found!");
        }

        var client = new HttpClient();
        var zip = await client.GetByteArrayAsync($"{_dlRepo}{package.Name}.zip");
        File.WriteAllBytes($"{package.Name}.zip", zip);

        FileManager.ExtractZip($"{package.Name}.zip", MediaDevice.Device.Name);

        WriteMetadata(package);
    }

    internal static async Task DownloadAroma()
    {

        var aromaPaylodsURL = "https://aroma.foryour.cafe/api/download?packages=environmentloader,wiiu-nanddumper-payload";
        var aromaBaseURL = "https://github.com/wiiu-env/Aroma/releases/download/beta-16/aroma-beta-16.zip";

        if (MediaDevice.Device is null)
        {
            throw new Exception("No target drive found!");
        }

        byte[] aromaBase;
        byte[] aromaPayloads;

        using (HttpClient client = new())
        {
            aromaBase = await client.GetByteArrayAsync(aromaBaseURL);
            aromaPayloads = await client.GetByteArrayAsync(aromaPaylodsURL);
        }

        var aromaBasePath = Path.Join(_downloadPath, "aroma.zip");
        var aromaPayloadsPath = Path.Join(_downloadPath, "aroma_payloads.zip");

        File.WriteAllBytes(aromaBasePath, aromaBase);
        File.WriteAllBytes(aromaPayloadsPath, aromaPayloads);

        FileManager.ExtractZip(aromaBasePath, MediaDevice.Device.Name);
        FileManager.ExtractZip(aromaPayloadsPath, MediaDevice.Device.Name);

        File.Delete(aromaBasePath);
        File.Delete(aromaPayloadsPath);
    }

    internal static async Task DownloadTiramisu()
    {
        var tiramisuDL = "https://github.com/wiiu-env/Tiramisu/releases/download/v0.1.2/environmentloader-28332a7+wiiu-nanddumper-payload-5c5ec09+fw_img_loader-c2da326.zip";

        if (MediaDevice.Device is null)
        {
            throw new Exception("No target drive found!");
        }

        byte[] tiramisu;
        using (HttpClient client = new())
        {
            tiramisu = await client.GetByteArrayAsync(tiramisuDL);
        }

        var tiramisuPath = Path.Join(_downloadPath, "tiramisu.zip");
        File.WriteAllBytes(tiramisuPath, tiramisu);

        FileManager.ExtractZip(tiramisuPath, MediaDevice.Device.Name);

        File.Delete(tiramisuPath);
    }
}