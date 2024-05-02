using Spectre.Console;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Text.Json;

namespace SDMU.NewFramework;

// Represents an object of the SD Card.

internal class MediaDevice
{
    public static DriveInfo? Device;

    public static bool HasTiramisu => IsTiramisuInstalled();
    public static bool HasAroma => IsAromaInstalled();
    public static bool HasHomebrew => IsHomebrewPresent();
    public static Package[] InstalledPackages => GetInstalledPackages();

    public MediaDevice()
    {
        SetTargetDrive();
    }

    private static DriveInfo[] GetRemovableDrives()
    {
        var drives = DriveInfo.GetDrives();
        List<DriveInfo> removableDrives = new List<DriveInfo>();
        foreach (DriveInfo drive in drives)
        {
            if (drive.DriveType == DriveType.Removable)
            {
                if (drive.IsReady) removableDrives.Add(drive);
            }
        }
        return removableDrives.ToArray();
    }

    private void SetTargetDrive()
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
        SelectionPrompt<DriveInfo> drivePrompt = new SelectionPrompt<DriveInfo>()
            .Title("Please select a drive:")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more drives)[/]")
            .AddChoices(drives)
            .UseConverter(drive => $"[yellow]{drive.Name}[/] [grey]({drive.DriveType}, {drive.DriveFormat}, {drive.TotalSize / 1000000000} GB, {drive.AvailableFreeSpace / 1000000000} GB free)[/]");

        Device = AnsiConsole.Prompt(drivePrompt);
    }

    private static int FormatWindows()
    {
        // CMD Command: format {_targetDrive.Name} /FS:FAT32 /V:WIIU_SD /Q /X | then exit
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        var name = MediaDevice.Device?.Name.Remove(MediaDevice.Device.Name.Length - 1);
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

    private static int FormatMac()
    {
        // CMD Command: diskutil eraseDisk FAT32 WIIU_SD MBRFormat /dev/disk2
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "diskutil";
        startInfo.Arguments = $"eraseDisk FAT32 WIIU_SD MBRFormat {MediaDevice.Device?.Name}";

        // Redirect standard output and error streams to null
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.UseShellExecute = false;

        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();

        return process.ExitCode;
    }

    private static int FormatLinux()
    {
        // CMD Command: mkfs.vfat -F 32 -n WIIU_SD /dev/sdb
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "mkfs.vfat";
        startInfo.Arguments = $"-F 32 -n WIIU_SD {MediaDevice.Device?.Name}";

        // Redirect standard output and error streams to null
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.UseShellExecute = false;

        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();

        return process.ExitCode;
    }

    // To support the three OSes, we need to do formatting seperately per OS
    internal static void Format()
    {
        var os = System.Environment.OSVersion.Platform;
        Debug.WriteLine($"OS is {os}");
        AnsiConsole.Status()
            .Start($"Formatting {MediaDevice.Device?.Name}...", ctx =>
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


                if (rc != 0)
                {
                    ctx.Spinner(Spinner.Known.Default); // Stop the spinner
                    throw new Exception("Failed to format the SD Card!");
                }
            });
    }

    private static bool IsHomebrewPresent()
    {
        string homebrewFolder = $"{Device}wiiu";
        return Directory.Exists(homebrewFolder);
    }

    private static bool IsAromaInstalled()
    {
        string aromaFolder = $"{Device}wiiu\\environments\\aroma";
        return Directory.Exists(aromaFolder);
    }

    private static bool IsTiramisuInstalled()
    {
        string tiramisuFolder = $"{Device}wiiu\\environments\\tiramisu";
        return Directory.Exists(tiramisuFolder);
    }

    private static Package[] GetInstalledPackages()
    {
        var metadataPath = $"{Device?.Name}\\metadata";

        if (metadataPath is null)
        {
            throw new Exception("No target drive found!");
        }

        if (!Directory.Exists(metadataPath))
        {
            // No applications installed
            return Array.Empty<Package>();
        }

        var metadataFiles = Directory.GetFiles(metadataPath, "*.json");

        if (metadataFiles.Length == 0)
        {
            throw new Exception("No metadata found!");
        }

        List<Package> packages = new();
        foreach (var file in metadataFiles)
        {
            var json = File.ReadAllText(file);
            var package = JsonSerializer.Deserialize<Package>(json);

            if (package is not null) { packages.Add(package); }
        }

        return [.. packages];
    }
}