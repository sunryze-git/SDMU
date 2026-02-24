using System.Buffers;
using System.IO.Compression;
using SDMU.NewFramework;
using Spectre.Console;

namespace SDMU.Utilities;

internal class FileManager(MediaDevice mediaDevice)
{
    internal static string BackupFolder => GetBackupPath();

    private static string GetBackupPath()
    {
        string baseDirectory;

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            // Check if we are running under sudo
            var sudoUser = Environment.GetEnvironmentVariable("SUDO_USER");

            // Path is /home/username
            baseDirectory = !string.IsNullOrEmpty(sudoUser) ? $"/home/{sudoUser}" : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        else // Windows
        {
            baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        // Fallback if somehow still empty
        if (string.IsNullOrWhiteSpace(baseDirectory))
            baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // We can explicitly add "Documents" for Linux/macOS to keep it clean
        var finalPath = OperatingSystem.IsWindows() 
            ? Path.Combine(baseDirectory, "SDMU_Backup")
            : Path.Combine(baseDirectory, "Documents", "SDMU_Backup");

        return finalPath;
    }

    internal static async Task ExtractZipAsync(string zipPath, string extractPath)
    {
        // Extract the zip file
        await ZipFile.ExtractToDirectoryAsync(zipPath, extractPath, true);
        File.Delete(zipPath);
    }

    private async Task CopyFilesAsync(string source, string target)
    {
        await AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new TransferSpeedColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var entries = Directory.GetFileSystemEntries(source, "*", new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true
                });

                var totalSize = entries
                    .Where(e => !Directory.Exists(e))
                    .Select(e => new FileInfo(e).Length)
                    .Sum();
                
                var mainTask = ctx.AddTask("Copying Files", maxValue: totalSize);
                
                var buffer = ArrayPool<byte>.Shared.Rent(81920); // 80 KB buffer
                try
                {
                    foreach (var item in entries)
                    {
                        var relativePath = Path.GetRelativePath(source, item);
                        var destinationPath = Path.Combine(target, relativePath);

                        if (Directory.Exists(item))
                        {
                            Directory.CreateDirectory(destinationPath);
                            continue;
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                        await using var sourceStream = new FileStream(item, FileMode.Open, FileAccess.Read,
                            FileShare.Read, 4096, true);
                        await using var destStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write,
                            FileShare.None, 4096, true);

                        int bytesRead;
                        while ((bytesRead = await sourceStream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                        {
                            // Use ReadOnlyMemory<byte> overload
                            await destStream.WriteAsync(buffer.AsMemory(0, bytesRead));

                            mainTask.Increment(bytesRead);
                        }
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            });
    }

    internal async Task BackupMedia()
    {
        var backupTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
    
        // Combine directly: BackupFolder + TimeStamp
        var targetDir = Path.Combine(BackupFolder, backupTime);
    
        // Ensure the specific timestamp folder is created
        Directory.CreateDirectory(targetDir);

        await CopyFilesAsync(mediaDevice.Device.Name, targetDir);
    }

    internal async Task RestoreMedia()
    {
        var backupFolders = Directory.GetDirectories(BackupFolder);
        if (backupFolders.Length == 0) throw new Exception("No backups found.");
        
        // Ask user to select a backup to restore
        var selectedBackup = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("[yellow]Select a backup to restore:[/]")
            .PageSize(10)
            .AddChoices(backupFolders));

        // Format SD Card
        await mediaDevice.Format();

        // Copy files from backup to SD Card
        await CopyFilesAsync(selectedBackup, mediaDevice.Device.Name);
    }

    internal async Task DeleteBackup()
    {
        var backupFolders = Directory.GetDirectories(BackupFolder);
        if (backupFolders.Length == 0) return;
        
        var prompt = new SelectionPrompt<string>()
            .Title("[yellow]Select a backup to delete:[/]")
            .PageSize(10)
            .AddChoices(backupFolders);

        var selectedBackup = AnsiConsole.Prompt(prompt);
        try
        {
            // Directory.Delete is sync, offload to background for responsiveness
            await Task.Run(() => Directory.Delete(selectedBackup, true));
            AnsiConsole.MarkupLine("[bold green]✔ Backup deleted![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to delete backup: {ex.Message}[/]");
        }

        await Task.Delay(2000);
    }
}