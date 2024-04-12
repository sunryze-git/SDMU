// SDManager class
// Responsible for interacting with an SD Card
// Will Impliment:
// - Determine if an SD Card is present in the system\
// - Read/Write data to the SD Card
// - Format the SD Card
// - Determine if there is already homebrew on the SD Card

using Spectre.Console;

namespace SDMU.Utilities;
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
        AnsiConsole.Status()
            .Start($"Formatting {_targetDrive.Name}...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);

                // CMD Command: format {_targetDrive.Name} /FS:FAT32 /V:WIIU_SD /Q /X | then exit
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                var name = _targetDrive.Name.Remove(_targetDrive.Name.Length - 1);
                startInfo.Arguments = $"/C format {name} /FS:FAT32 /V:HBSD /Q /X /y";

                // Redirect standard output and error streams to null
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    ctx.Spinner(Spinner.Known.Default); // Stop the spinner
                    throw new Exception("Failed to format the SD Card!");
                }
            });
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
        var drives = _sdCardPath();
        if (drives.Length < 1)
        {
            throw new Exception("No removable media found!");
        }

        // Prompt the user to select a drive
        SelectionPrompt<DriveInfo> drivePrompt = new SelectionPrompt<DriveInfo>()
            .Title("Please select a drive:")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more drives)[/]")
            .AddChoices(drives)
            .UseConverter(drive => $"[yellow]{drive.Name}[/] [grey]({drive.DriveType}, {drive.DriveFormat}, {drive.TotalSize / 1000000000} GB, {drive.AvailableFreeSpace / 1000000000} GB free)[/]");

        _targetDrive = AnsiConsole.Prompt(drivePrompt);
        return _targetDrive;
    }
}