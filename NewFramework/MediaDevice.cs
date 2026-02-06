using System.Diagnostics;
using System.Text.Json;
using Spectre.Console;

namespace SDMU.NewFramework;

// Represents an object of the SD Card.

internal class MediaDevice
{
    public readonly DriveInfo Device;

    public bool HasTiramisu => IsTiramisuInstalled();
    public bool HasAroma => IsAromaInstalled();
    public bool HasHomebrew => IsHomebrewPresent();
    public Package[] InstalledPackages => GetInstalledPackages();

    public MediaDevice()
    {
        Device = SetTargetDrive();
    }

    private DriveInfo[] GetRemovableDrives()
    {
        var drives = DriveInfo.GetDrives();
        return drives
            .Where(drive => drive is { DriveType: DriveType.Removable, IsReady: true })
            .ToArray();
    }

    private DriveInfo SetTargetDrive()
    {
        var drives = GetRemovableDrives();
        while (drives.Length < 1)
        {
            // Prompt to say no removable drives were found
            // allow the user to plug in a drive and try again
            AnsiConsole.MarkupLine("[red]No SD Media found![/]");
            AnsiConsole.MarkupLine("[yellow]Please plug in a SD Card and try again.[/]");
            AnsiConsole.MarkupLine("[yellow]Press any key to continue...[/]");
            Console.ReadKey();
            drives = GetRemovableDrives();
        }

        // Prompt the user to select a drive
        var drivePrompt = new SelectionPrompt<DriveInfo>()
            .Title("Please select a drive:")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more drives)[/]")
            .AddChoices(drives)
            .UseConverter(drive =>
                $"[yellow]{drive.Name}[/] [grey]({drive.DriveType}, {drive.DriveFormat}, {drive.TotalSize / 1000000000} GB, {drive.AvailableFreeSpace / 1000000000} GB free)[/]");

        return AnsiConsole.Prompt(drivePrompt);
    }

    private int FormatWindows()
    {
        // CMD Command: format {_targetDrive.Name} /FS:FAT32 /V:WIIU_SD /Q /X | then exit
        var process = new Process();
        var startInfo = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe"
        };
        var name = Device.Name.Remove(Device.Name.Length - 1);
        startInfo.Arguments = $"/C format {name} /FS:FAT32 /V:HBSD /Q /X /y";

        // Redirect standard output and error streams to null
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.UseShellExecute = false;

        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();

        return process.ExitCode;
    }

    private int FormatMac()
    {
        // CMD Command: diskutil eraseDisk FAT32 WIIU_SD MBRFormat /dev/disk2
        var process = new Process();
        var startInfo = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "diskutil",
            Arguments = $"eraseDisk FAT32 WIIU_SD MBRFormat {Device.Name}",
            // Redirect standard output and error streams to null
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();

        return process.ExitCode;
    }

    private int FormatLinux()
    {
        // CMD Command: mkfs.vfat -F 32 -n WIIU_SD /dev/sdb
        var process = new Process();
        var startInfo = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "mkfs.vfat",
            Arguments = $"-F 32 -n WIIU_SD {Device.Name}",
            // Redirect standard output and error streams to null
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();

        return process.ExitCode;
    }

    // To support the three OSes, we need to do formatting seperately per OS
    internal void Format()
    {
        var os = Environment.OSVersion.Platform;
        Debug.WriteLine($"OS is {os}");
        AnsiConsole.Status()
            .Start($"Formatting {Device.Name}...", ctx =>
            {
                int rc;
                ctx.Spinner(Spinner.Known.Dots);

                switch (os)
                {
                    case PlatformID.Win32NT:
                        rc = FormatWindows();
                        break;
                    case PlatformID.Unix:
                        rc = FormatLinux();
                        break;
                    case PlatformID.MacOSX:
                        rc = FormatMac();
                        break;
                    default:
                        throw new Exception("Unsupported OS!");
                }


                if (rc == 0) return;
                ctx.Spinner(Spinner.Known.Default); // Stop the spinner
                throw new Exception("Failed to format the SD Card!");
            });
    }

    private bool IsHomebrewPresent()
    {
        var homebrewFolder = $"{Device}wiiu";
        return Directory.Exists(homebrewFolder);
    }

    private bool IsAromaInstalled()
    {
        var aromaFolder = $@"{Device}wiiu\environments\aroma";
        return Directory.Exists(aromaFolder);
    }

    private bool IsTiramisuInstalled()
    {
        var tiramisuFolder = $@"{Device}wiiu\environments\tiramisu";
        return Directory.Exists(tiramisuFolder);
    }

    private Package[] GetInstalledPackages()
    {
        var metadataPath = $"{Device.Name}\\metadata";

        if (metadataPath is null) throw new Exception("No target drive found!");

        if (!Directory.Exists(metadataPath))
            // No applications installed
            return [];

        var metadataFiles = Directory.GetFiles(metadataPath, "*.json");

        if (metadataFiles.Length == 0) throw new Exception("No metadata found!");

        List<Package> packages = [];
        packages.AddRange(metadataFiles
            .Select(File.ReadAllText)
            .Select(json => JsonSerializer.Deserialize<Package>(json))
            .OfType<Package>());

        return [.. packages];
    }
}