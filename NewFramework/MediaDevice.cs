using System.Diagnostics;
using System.Text.Json;
using Spectre.Console;

namespace SDMU.NewFramework;

// Represents an object of the SD Card.

internal class MediaDevice
{
    public readonly DriveInfo Device = SetTargetDrive();

    private static DriveInfo[] GetRemovableDrives()
    {
        var drives = DriveInfo.GetDrives();
        return drives
            .Where(drive => drive is { DriveType: DriveType.Removable, IsReady: true })
            .ToArray();
    }

    private static DriveInfo SetTargetDrive()
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

    private async Task TryRunProcess(string fileName, string arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        process.Start();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Process failed with exit code {process.ExitCode}: {error.Trim()}");
        }
    }

    private async Task<string> ResolvePhysicalPathLinux(string mountPoint)
    {
        var target = mountPoint.TrimEnd('/');

        foreach (var line in await File.ReadAllLinesAsync("/proc/mounts"))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;

            var currentMount = parts[1].Replace("\\040", " ").TrimEnd('/');
            if (currentMount == target) 
                return parts[0];
        }
    
        throw new Exception($"Could not find physical device for mount point: {mountPoint}");
    }

    private Task FormatWindows() 
        => TryRunProcess("cmd.exe", $"/C format {Device.Name.TrimEnd('\\')} /FS:FAT32 /V:HBSD /Q /X /y");

    private Task FormatMacAsync() 
        => TryRunProcess("diskutil", $"eraseDisk FAT32 WIIU_SD MBRFormat {Device.Name}");

    private async Task FormatLinux()
    {
        var mountPoint = Device.Name.TrimEnd('/');
        var physicalPath = await ResolvePhysicalPathLinux(Device.Name.TrimEnd('/'));
        
        AnsiConsole.MarkupLine("[yellow]Unmounting SD Card...[/]");
        await TryRunProcess("umount", physicalPath);
        
        AnsiConsole.MarkupLine("[yellow]Formatting SD Card...[/]");
        await TryRunProcess("mkfs.vfat", $"-F 32 -n WIIU_SD {physicalPath}");
        
        if (!Directory.Exists(mountPoint))
        {
            AnsiConsole.MarkupLine("[yellow]Creating mount directory...[/]");
            await TryRunProcess("mkdir", $"-p {mountPoint}");
        }
        
        AnsiConsole.MarkupLine("[yellow]Mounting SD Card...[/]");
        await TryRunProcess("mount", $"{physicalPath} {mountPoint}");
    }

    // To support the three OSes, we need to do formatting seperately per OS
    internal async Task Format()
    {
        try
        {
            await AnsiConsole.Status().StartAsync("Formatting...", async _ =>
            {
                if (OperatingSystem.IsWindows()) await FormatWindows();
                else if (OperatingSystem.IsMacOS()) await FormatMacAsync();
                else if (OperatingSystem.IsLinux()) await FormatLinux();
                else throw new PlatformNotSupportedException();
                
                AnsiConsole.MarkupLine("[green]Format completed successfully![/]");
            });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Format Failed:[/] {ex.Message}");
        }
        finally
        {
            AnsiConsole.MarkupLine("[red]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
    }

    private string GetEnvironmentPath(string environment) 
        => Path.Combine(Device.Name, "wiiu", "environments", environment);

    public bool IsHomebrewPresent() 
        => Directory.Exists(Path.Combine(Device.Name, "wiiu"));

    public bool IsAromaInstalled() 
        => Directory.Exists(GetEnvironmentPath("aroma"));

    public bool IsTiramisuInstalled() 
        => Directory.Exists(GetEnvironmentPath("tiramisu"));

    public async Task<Package[]> GetInstalledPackages()
    {
        var metadataPath = Path.Combine(Device.Name, "metadata");
        if (!Directory.Exists(metadataPath)) return [];

        var metadataFiles = Directory.GetFiles(metadataPath, "*.json");

        var readTasks = metadataFiles.Select(async file =>
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);
                return (Content: content, FilePath: file);
            }
            catch (IOException)
            {
                return (Content: null, FilePath: file);
            }
        });
        
        var results = await Task.WhenAll(readTasks);
        return results
            .Where(r => r.Content != null)
            .Select(r => TryDeserialize(r.Content!, r.FilePath))
            .OfType<Package>()
            .ToArray();
    }

    private static Package? TryDeserialize(string json, string localPath)
    {
        try
        {
            var pkg = JsonSerializer.Deserialize<Package>(json);
            pkg!.LocalPath = localPath;
            return pkg;
        }
        catch (JsonException ex)
        {
            AnsiConsole.WriteException(ex);
            return null;
        }
    }
}