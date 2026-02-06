using Spectre.Console;

namespace SDMU.NewFramework;
internal class Updater(MediaDevice mediaDevice, Downloader downloader)
{
    public async Task ComparePackageHash()
    {
        // Get installed packages
        var installedPackages = mediaDevice.InstalledPackages;
        var latestPackages = await downloader.GetPackages();

        // Sort list to only include instaled packages wtih a different MD5 than the latest
        var outdatedPackages = installedPackages
            .Where(p => latestPackages.Any(l => l.Name == p.Name && l.Md5 != p.Md5))
            .Where(p => p.Name is not null)
            .ToList();

        // Quit if no outdated packages are found
        if (outdatedPackages.Count < 1)
        {
            AnsiConsole.MarkupLine("[green]All packages are up to date![/]");
            Thread.Sleep(2000);
            return;
        }

        // Update the packages that are outdated
        foreach (var outdatedPackage in outdatedPackages)
        {
            AnsiConsole.MarkupLine($"[yellow]Package {outdatedPackage.Name} is outdated! Downloading Update...[/]");
            await downloader.DownloadPackage(outdatedPackage.Name!);
        }
    }
}