using SDMU.NewFramework;
using SDMU.Utilities;
using Spectre.Console;

namespace SDMU.Menus;

internal class MainMenu(MediaDevice mediaDevice, FileManager fileManager, Downloader downloader, HbManager hbManager,
    AppMenu appMenu, SdMenu sdMenu)
{
    private bool HomebrewDetected => mediaDevice.IsHomebrewPresent();

    public async Task Show()
    {
        while (true)
        {
            Console.Clear();

            // Write Header
            AnsiConsole.Write(
                new Panel(
                        new FigletText("Welcome to SDMU!")
                            .Centered()
                            .Color(Color.LightSteelBlue))
                    .Expand()
                    .Border(BoxBorder.Rounded)
                    .Header("[yellow]Always make sure to reference the [link=https://wiiu.hacks.guide#/][blue]Wii U Hacks Guide![/][/][/]")
                    .HeaderAlignment(Justify.Center)
                    .BorderStyle(new Style(Color.White)));
            
            AnsiConsole.MarkupLine($"[yellow]SD Card: {mediaDevice.Device?.Name}[/]\n");
            // 
            var mainMenuItems = new List<(string Name, Func<Task> method)>()
            {
                ("Manage Applications", appMenu.Show),
                ("Manage SD Card", sdMenu.Show),
                ("Exit", () => { 
                    Environment.Exit(0);
                    return Task.CompletedTask;
                })
            };

            // Conditional items based on Homebrew detection
            if (!HomebrewDetected)
            {
                mainMenuItems.Insert(0,("Install Homebrew", hbManager.InstallHomebrew));
            }
            else
            {
                mainMenuItems.Insert(0, ("Update Homebrew", hbManager.UpdateHomebrew));
            }

            var prompt = new SelectionPrompt<(string Name, Func<Task> id)>()
                .PageSize(10)
                .UseConverter(item => item.Name)
                .AddChoices(mainMenuItems);

            var selectedItem = AnsiConsole.Prompt(prompt);
            await selectedItem.Item2.Invoke();
        }
    }
}