using Spectre.Console;

namespace SDMU.NewFramework;
internal class Updater
{
    public static async void ComparePackageHash()
    {
        // Get installed packages
        var installedPackages = MediaDevice.InstalledPackages;
        var latestPackages = await Downloader.GetPackages();

        // Sort list to only include instaled packages wtih a different MD5 than the latest
        var outdatedPackages = installedPackages.Where(p => latestPackages.Any(l => l.Name == p.Name && l.Md5 != p.Md5)).ToList();

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
            AnsiConsole.MarkupLine($"[yellow]Package {outdatedPackage.Name} is outdated![/]");
            AnsiConsole.MarkupLine($"[yellow]Downloading latest version...[/]");
            await Downloader.DownloadPackage(outdatedPackage.Name);
        }
    }
}