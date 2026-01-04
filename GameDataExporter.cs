using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Web.Script.Serialization;

namespace GameDataExporter
{
    // Data models for JSON serialization
    public class GameActionPoco
    {
        public string AdditionalArguments { get; set; }
        public string Arguments { get; set; }
        public Guid? EmulatorId { get; set; }
        public Guid? EmulatorProfileId { get; set; }
        public int InitialTrackingDelay { get; set; }
        public bool IsPlayAction { get; set; }
        public string Name { get; set; }
        public bool OverrideDefaultArgs { get; set; }
        public string Path { get; set; }
        public string Script { get; set; }
        public int TrackingFrequency { get; set; }
        public TrackingMode TrackingMode { get; set; }
        public string TrackingPath { get; set; }
        public GameActionType Type { get; set; }
        public string WorkingDir { get; set; }
    }

    public class GamePoco
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public ReleaseDate? ReleaseDate { get; set; }
        public ulong Playtime { get; set; } // Changed to ulong
        public bool IsInstalled { get; set; }
        public string InstallDirectory { get; set; }
        public string Icon { get; set; }
        public bool Hidden { get; set; }
        public List<GameActionPoco> GameActions { get; set; }
    }

    public class GameDataExporter : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public override Guid Id { get; } = Guid.Parse("66b8eca4-3f39-4b79-a359-3cb98d5b18fd");

        public GameDataExporter(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        private void ExportLibrary()
        {
            var gamesOutput = new List<GamePoco>();
            foreach (var game in PlayniteApi.Database.Games)
            {
                var gameActionsOutput = new List<GameActionPoco>();
                if (game.GameActions != null)
                {
                    foreach (var action in game.GameActions)
                    {
                        gameActionsOutput.Add(new GameActionPoco
                        {
                            AdditionalArguments = action.AdditionalArguments,
                            Arguments = action.Arguments,
                            //EmulatorId = action.EmulatorId,
                            //EmulatorProfileId = action.EmulatorProfileId,
                            InitialTrackingDelay = action.InitialTrackingDelay,
                            IsPlayAction = action.IsPlayAction,
                            Name = action.Name,
                            OverrideDefaultArgs = action.OverrideDefaultArgs,
                            Path = action.Path,
                            Script = action.Script,
                            TrackingFrequency = action.TrackingFrequency,
                            TrackingMode = action.TrackingMode,
                            TrackingPath = action.TrackingPath,
                            Type = action.Type,
                            WorkingDir = action.WorkingDir
                        });
                    }
                }

                gamesOutput.Add(new GamePoco
                {
                    Id = game.Id,
                    Name = game.Name,
                    Source = game.Source?.Name,
                    ReleaseDate = game.ReleaseDate,
                    Playtime = game.Playtime,
                    IsInstalled = game.IsInstalled,
                    InstallDirectory = game.InstallDirectory,
                    Icon = game.Icon,
                    Hidden = game.Hidden,
                    GameActions = gameActionsOutput
                });
            }

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(gamesOutput);
            var filePath = Path.Combine(GetPluginUserDataPath(), "library.json");
            File.WriteAllText(filePath, json);
        }

        private void StartGameActionManually(GameAction action)
        {
            if (action == null) return;

            switch (action.Type)
            {
                case GameActionType.File:
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = action.Path;
                    process.StartInfo.Arguments = action.Arguments;
                    process.StartInfo.WorkingDirectory = action.WorkingDir;
                    process.Start();
                    break;
                case GameActionType.URL: // Corrected from Url
                    System.Diagnostics.Process.Start(action.Path);
                    break;
                case GameActionType.Emulator:
                case GameActionType.Script:
                default:
                    logger.Warn($"Starting game actions of type '{action.Type}' is not supported by this plugin.");
                    break;
            }
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            ExportLibrary();

            // Register URI handler
            PlayniteApi.UriHandler.RegisterSource("gamedataexporter", (uriArgs) =>
            {
                if (uriArgs.Arguments.Count() >= 3 && uriArgs.Arguments[0] == "start_gameaction")
                {
                    var gameIdString = uriArgs.Arguments[1];
                    var actionName = uriArgs.Arguments[2];

                    if (Guid.TryParse(gameIdString, out var gameId))
                    {
                        var game = PlayniteApi.Database.Games.Get(gameId);
                        if (game != null)
                        {
                            var action = game.GameActions?.FirstOrDefault(a => a.Name == actionName);
                            if (action != null)
                            {
                                StartGameActionManually(action);
                            }
                            else
                            {
                                logger.Warn($"Game action '{actionName}' not found for game '{game.Name}'.");
                            }
                        }
                        else
                        {
                            logger.Warn($"Game with ID '{gameId}' not found.");
                        }
                    }
                    else
                    {
                        logger.Warn($"Invalid Game ID format '{gameIdString}'.");
                    }
                }
            });
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            ExportLibrary();
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            ExportLibrary();
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            ExportLibrary();
        }
        
        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = "Update Game Data JSON",
                    Action = (a) =>
                    {
                        ExportLibrary();
                    },
                    MenuSection = "@Game Data Exporter"
                }
            };
        }
        
        // --- Unused event handlers from template ---
        public override void OnGameStarted(OnGameStartedEventArgs args) { }
        public override void OnGameStarting(OnGameStartingEventArgs args) { }
        public override void OnGameStopped(OnGameStoppedEventArgs args) { }
        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args) { }
    }
}