using Spectre.Console;
using System.Text.Json;

namespace SDMU;

internal class Package
{
    public string? Category;
    public string? Name;
    public string? Updated;
    public string? URL;
    public string? md5;
}

internal class Downloader
{
    string _repo = "https://wiiu.cdn.fortheusers.org/repo.json";
    string _dlRepo = "https://wiiu.cdn.fortheusers.org/zips/";

    string _downloadPath = Path.GetTempPath();

    Package[]? repoPackages;

    public async void GetPackages()
    {
        var client = new HttpClient();
        var json = await client.GetStringAsync(_repo);
        var packages = JsonSerializer.Deserialize<List<Package>>(json);

        if (packages is null)
        {
            throw new Exception("Failed to download packages!");
        }

        repoPackages = [.. packages];
    }

    public async void DownloadPackage(Package package)
    {
        AnsiConsole.MarkupLine($"Downloading [bold]{package.Name}[/]...");

        var client = new HttpClient();
        var zip = await client.GetByteArrayAsync($"{_dlRepo}{package.Name}.zip");
        File.WriteAllBytes($"{package.Name}.zip", zip);

        FileManager.ExtractZip($"{package.Name}.zip", SDManager._targetDrive.Name);
    }

    public async Task DownloadAroma()
    {
        AnsiConsole.MarkupLine("Downloading Aroma...");

        var aromaPaylodsURL = "https://aroma.foryour.cafe/api/download?packages=environmentloader,wiiu-nanddumper-payload";
        var aromaBaseURL = "https://github.com/wiiu-env/Aroma/releases/download/beta-16/aroma-beta-16.zip";

        if (SDManager._targetDrive is null)
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

        FileManager.ExtractZip(aromaBasePath, SDManager._targetDrive.Name);
        FileManager.ExtractZip(aromaPayloadsPath, SDManager._targetDrive.Name);

        File.Delete(aromaBasePath);
        File.Delete(aromaPayloadsPath);
    }

    public async Task DownloadTiramisu()
    {
        AnsiConsole.MarkupLine("Downloading Tiramisu...");

        var tiramisuDL = "https://github.com/wiiu-env/Tiramisu/releases/download/v0.1.2/environmentloader-28332a7+wiiu-nanddumper-payload-5c5ec09+fw_img_loader-c2da326.zip";

        if (SDManager._targetDrive is null)
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

        FileManager.ExtractZip(tiramisuPath, SDManager._targetDrive.Name);

        File.Delete(tiramisuPath);
    }
}