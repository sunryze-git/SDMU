﻿using SDMU.Menus;
using SDMU.NewFramework;
using SDMU.Utilities;
using Spectre.Console;

internal static class MainMenu
{
    static bool homebrewDetected;

    public static void Show()
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

            if (!homebrewDetected)
            {
                new MediaDevice();
            }
            AnsiConsole.MarkupLine($"[yellow]SD Card: {MediaDevice.Device?.Name}[/]\n");
            // 

            homebrewDetected = MediaDevice.HasHomebrew;
            var mainMenuItems = new List<(string Name, string Id)>();

            // Conditional items based on Homebrew detection
            if (!homebrewDetected)
            {
                mainMenuItems.Add(("Install Homebrew", "install"));
            }
            else
            {
                mainMenuItems.AddRange(new[]
                {
                    ("Update Homebrew", "update")
                });
            }

            // Adding spacers
            mainMenuItems.Add((" ", "spacer1"));

            // Common items
            mainMenuItems.AddRange(new[]
            {
                ("Manage Applications", "appmenu"),
                ("Manage SD Card", "sdmenu"),
                (" ", "spacer2"), // Another spacer
                ("Exit", "exit")
            });

            var prompt = new SelectionPrompt<(string Name, string Id)>()
                .PageSize(10)
                .UseConverter(item => item.Name)
                .AddChoices(mainMenuItems);

            var selectedItem = AnsiConsole.Prompt(prompt);

            // Exit the loop and program if "Exit" is selected
            if (selectedItem.Id == "exit") break;

            // Ignore selections of spacers
            if (selectedItem.Id.StartsWith("spacer")) continue;

            HandleSelection(selectedItem.Id, homebrewDetected);
        }
    }

    private static void HandleSelection(string selectionId, bool homebrewDetected)
    {
        switch (selectionId)
        {
            case "install":
                HBManager.InstallHomebrew();
                break;
            case "update":
                HBManager.UpdateHomebrew();
                break;
            case "appmenu":
                AppMenu.Show();
                break;
            case "sdmenu":
                SDMenu.Show();
                break;
            default:
                AnsiConsole.WriteLine("Invalid selection.");
                break;
        }
    }
}
