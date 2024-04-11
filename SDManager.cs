// SDManager class
// Responsible for interacting with an SD Card
// Will Impliment:
// - Determine if an SD Card is present in the system\
// - Read/Write data to the SD Card
// - Format the SD Card
// - Determine if there is already homebrew on the SD Card

using Spectre.Console;

namespace SDMU;
public static class SDManager
{
    public static DriveInfo? _targetDrive;

    internal static DriveInfo[] _sdCardPath()
    { // Returns a list of all the removable drives in the system
        var drives = DriveInfo.GetDrives();
        List<DriveInfo> sdCardPaths = new List<DriveInfo>();
        foreach (DriveInfo drive in drives)
        {
            if (true)
            {
                if (drive.IsReady)
                {
                    sdCardPaths.Add(drive);
                }
            }
        }
        return sdCardPaths.ToArray();
    }

    internal static void FormatSDCard()
    {
        Console.WriteLine($"Formatting {_targetDrive.Name}...");
        // CMD Command: format {_targetDrive.Name} /FS:FAT32 /V:WIIU_SD /Q /X | then exit
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        var name = _targetDrive.Name.Remove(_targetDrive.Name.Length - 1);
        startInfo.Arguments = $"/C format {name} /FS:FAT32 /V:HBSD /Q /X /y";
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new Exception("Failed to format the SD Card!");
        }
    }

    internal static bool IsHomebrewPresent()
    { // Checks if the homebrew folder is present on the SD Card
        string homebrewFolder = $"{_targetDrive}wiiu";
        if (Directory.Exists(homebrewFolder))
        {
            return true;
        }
        return false;
    }

    internal static DriveInfo DetermineTargetDrive()
    {
        var table = new Table();
        table.AddColumn("Drive");
        table.AddColumn("Type");
        table.AddColumn("Format");
        table.AddColumn("Size");
        table.AddColumn("Free Space");

        AnsiConsole.MarkupLine("Checking for removable media...");

        var drives = _sdCardPath();
        if (drives.Length < 1)
        {
            AnsiConsole.MarkupLine("[red]No removable media found![/]");
            throw new Exception("No removable media found!");
        }

        foreach (var drive in drives)
        {
            if (drive is null)
            {
                continue;
            }

            table.AddRow(drive.Name, drive.DriveType.ToString(), drive.DriveFormat, $"{(drive.TotalSize / 1000000000).ToString()} GB", $"{(drive.AvailableFreeSpace / 1000000000).ToString()} GB");
        }
        AnsiConsole.Write(table);
        // 

        // Step 2: Ask user to select a drive
        SelectionPrompt<DriveInfo> drivePrompt = new SelectionPrompt<DriveInfo>()
            .Title("Which drive would you like to use?")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more drives)[/]")
            .AddChoices(drives);

        _targetDrive = AnsiConsole.Prompt(drivePrompt);
        AnsiConsole.MarkupLine($"[green]Selected drive: {_targetDrive.Name}[/]");
        //

        return _targetDrive;
    }
}