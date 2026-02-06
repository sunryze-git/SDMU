using SDMU.NewFramework;
using SDMU.Utilities;
using Spectre.Console;

namespace SDMU.Menus;

internal class MainMenu
{
    private readonly MediaDevice _mediaDevice;
    private readonly HbManager _hbManager;
    private readonly FileManager _fileManager;
    private readonly Downloader _downloader;
    private readonly AppTypes _appType;
    private readonly AppMenu _appMenu;
    private readonly Updater _updater;
    private readonly SdMenu _sdMenu;
    public MainMenu()
    {
        _mediaDevice = new MediaDevice();
        _fileManager = new FileManager(_mediaDevice);
        _downloader = new Downloader(new HttpClient(),  _mediaDevice);
        _appType = new AppTypes(_downloader);
        _hbManager = new HbManager(_mediaDevice,  _fileManager, _downloader,  _appType);
        _updater = new Updater(_mediaDevice, _downloader);
        _appMenu = new AppMenu(_mediaDevice, _fileManager, _downloader, _appType, _updater);
        _sdMenu = new SdMenu(_fileManager, _mediaDevice);
    }

    private bool HomebrewDetected => _mediaDevice.HasHomebrew;

    public void Show()
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
            
            AnsiConsole.MarkupLine($"[yellow]SD Card: {_mediaDevice.Device?.Name}[/]\n");
            // 
            var mainMenuItems = new List<(string Name, Func<Task> Delegate)>()
            {
                ("Manage Applications", () => Task.Run(_appMenu.Show)),
                ("Manage SD Card", () => Task.Run(_sdMenu.Show)),
                ("Exit", () => Task.Run(Environment.Exit(0)))
            };

            // Conditional items based on Homebrew detection
            
            if (!HomebrewDetected)
            {
                mainMenuItems.Add(("Install Homebrew", "install"));
            }
            else
            {
                mainMenuItems.AddRange([
                    ("Update Homebrew", "update")
                ]);
            }

            var prompt = new SelectionPrompt<(string Name, string Id)>()
                .PageSize(10)
                .UseConverter(item => item.Name)
                .AddChoices(mainMenuItems);

            var selectedItem = AnsiConsole.Prompt(prompt);

            // Exit the loop and program if "Exit" is selected
            if (selectedItem.Id == "exit") break;

            // Ignore selections of spacers
            if (selectedItem.Id.StartsWith("spacer")) continue;

            HandleSelection(selectedItem.Id, HomebrewDetected);
        }
    }

    private void HandleSelection(string selectionId, bool homebrewDetected)
    {
        switch (selectionId)
        {
            case "install":
                _hbManager.InstallHomebrew();
                break;
            case "update":
                _hbManager.UpdateHomebrew();
                break;
            case "appmenu":
                AppMenu.Show();
                break;
            case "sdmenu":
                SdMenu.Show();
                break;
            default:
                AnsiConsole.WriteLine("Invalid selection.");
                break;
        }
    }
}