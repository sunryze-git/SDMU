using SDMU.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

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
    
    public string LocalPath { get; set; } = string.Empty;
}

internal class RootObject
{
    [JsonPropertyName("packages")]
    public IEnumerable<Package>? Packages { get; set; }
}

internal class Downloader(HttpClient client, MediaDevice mediaDevice)
{
    private const string Repo = "https://wiiu.cdn.fortheusers.org/repo.json";
    private const string DlRepo = "https://wiiu.cdn.fortheusers.org/zips/";
    private const string TiramisuDl = "https://github.com/wiiu-env/Tiramisu/releases/download/v0.1.2/environmentloader-28332a7+wiiu-nanddumper-payload-5c5ec09+fw_img_loader-c2da326.zip";
    private const string AromaPaylodsUrl = "https://aroma.foryour.cafe/api/download?packages=environmentloader,wiiu-nanddumper-payload";
    private const string AromaBaseUrl = "https://github.com/wiiu-env/Aroma/releases/download/beta-16/aroma-beta-16.zip";

    private readonly Dictionary<string, Package> _packageCache = new(StringComparer.OrdinalIgnoreCase);
    
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, // Or whatever the repo uses
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    private async Task WriteMetadata(Package package)
    {
        var json = JsonSerializer.Serialize(package, SerializerOptions);
        var metadataPath = Path.Combine(mediaDevice.Device.Name, "metadata");

        // Create path if not exist
        if (!Directory.Exists(metadataPath)) Directory.CreateDirectory(metadataPath);

        var outFile = Path.Combine(metadataPath, $"{package.Name}.json");
        await File.WriteAllTextAsync(outFile, json);
    }

    internal async Task<IEnumerable<Package>> GetPackages(string? category = null)
    {
        if (_packageCache.Count == 0)
        {
            var json = await client.GetStringAsync(Repo);
            var root = JsonSerializer.Deserialize<RootObject>(json, SerializerOptions);

            if (root?.Packages != null)
            {
                foreach (var p in root.Packages)
                {
                    if (!string.IsNullOrEmpty(p.Name))
                        _packageCache[p.Name] = p;
                }
            }
        }

        if (category == null)
        {
            return _packageCache.Values;
        }

        return _packageCache.Values.Where(x => 
            string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase));
    }

    internal async Task DownloadPackage(string packageName)
    {
        await GetPackages(); // Ensures cache is ready

        // O(1) Lookup - This is the "Speed Hack"
        if (!_packageCache.TryGetValue(packageName, out var package))
        {
            throw new Exception($"Package '{packageName}' not found in the repository!");
        }

        var tempZipPath = Path.Combine(Path.GetTempPath(), $"{package.Name}_{Guid.NewGuid()}.zip");
        
        try 
        {
            await DownloadFile(new Uri($"{DlRepo}{package.Name}.zip"), tempZipPath);
            await FileManager.ExtractZipAsync(tempZipPath, mediaDevice.Device.Name);
            await WriteMetadata(package);
        }
        finally 
        {
            if (File.Exists(tempZipPath)) File.Delete(tempZipPath);
        }
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

    private async Task DownloadFile(Uri uri, string fullPath)
    {
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var outStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
        await response.Content.CopyToAsync(outStream);
    }
    
    private async Task DownloadFileAndExtract(Uri uri, string extractPath)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
        try 
        {
            await DownloadFile(uri, tempFile);
            await FileManager.ExtractZipAsync(tempFile, extractPath);
        }
        finally 
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}