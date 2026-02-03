using SDMU.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SDMU.NewFramework;

internal class Package
{
    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("binary")]
    public string? Binary { get; init; }

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


internal abstract class Downloader(HttpClient client, MediaDevice mediaDevice)
{
    private const string Repo = "https://wiiu.cdn.fortheusers.org/repo.json";
    private const string DlRepo = "https://wiiu.cdn.fortheusers.org/zips/";
    private const string TiramisuDl = "https://github.com/wiiu-env/Tiramisu/releases/download/v0.1.2/environmentloader-28332a7+wiiu-nanddumper-payload-5c5ec09+fw_img_loader-c2da326.zip";
    private const string AromaPaylodsUrl = "https://aroma.foryour.cafe/api/download?packages=environmentloader,wiiu-nanddumper-payload";
    private const string AromaBaseUrl = "https://github.com/wiiu-env/Aroma/releases/download/beta-16/aroma-beta-16.zip";

    private void WriteMetadata(Package package)
    {
        var json = JsonSerializer.Serialize(package);
        var targetDrive = mediaDevice.Device.Name;
        var metadataPath = Path.Join(targetDrive, "metadata");

        // Create path if not exist
        if (!Directory.Exists(metadataPath)) Directory.CreateDirectory(metadataPath);

        var outFile = Path.Join(metadataPath, $"{package.Name}.zip");
        File.WriteAllText(outFile, json);
    }

    internal async Task<Package[]> GetPackages(string? category = null)
    {
        var json = await client.GetStringAsync(Repo);
        var root = JsonSerializer.Deserialize<RootObject>(json);
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
            return root.Packages
                .Where(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
        return root.Packages.ToArray();
    }

    internal async Task DownloadPackage(string packageName)
    {
        var packages = await GetPackages();
        var package = packages
            .FirstOrDefault(x => string.Equals(x.Name, packageName, StringComparison.OrdinalIgnoreCase));

        if (package is null)
        {
            throw new Exception("Package not found!");
        }
        
        var packageDownloadPath = Path.Join(Path.GetTempPath(), $"{package.Name}.zip");
        await DownloadFile(new Uri($"{DlRepo}{package.Name}.zip"), packageDownloadPath);
        FileManager.ExtractZip(packageDownloadPath, mediaDevice.Device.Name);

        WriteMetadata(package);
    }

    internal async Task DownloadAroma()
    {
        await DownloadFileAndExtract(new Uri(AromaBaseUrl), mediaDevice.Device.Name);
        await DownloadFileAndExtract(new Uri(AromaPaylodsUrl), mediaDevice.Device.Name);
    }

    internal async Task DownloadTiramisu()
    {
        await DownloadFileAndExtract(new Uri(TiramisuDl), mediaDevice.Device.Name);
    }

    private async Task<string> DownloadFile(Uri uri, string outputPath)
    {
        var fileName = Path.GetFileName(uri.LocalPath);
        var filePath = Path.Join(outputPath, fileName);
        try
        {
            var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using FileStream outStream = new(filePath, FileMode.Create);
            await response.Content.CopyToAsync(outStream);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Failed to download file: {e.Message}");
        }
        
        return filePath;
    }
    
    private async Task DownloadFileAndExtract(Uri uri, string outputPath)
    {
        var outputFile = await DownloadFile(uri, outputPath);
        FileManager.ExtractZip(outputFile, mediaDevice.Device.Name);
    }
}