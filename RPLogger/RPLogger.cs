using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using Dalamud.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using World = Lumina.Excel.GeneratedSheets.World;
using RPLogger.Windows;

namespace RPLogger;

/// <summary>
/// Main class for the <c>RPLogger</c> plugin
/// </summary>
public sealed class RPLogger : IDalamudPlugin
{
    // Settings
    public string Name => "RP Logger";
    private const string ConfigWindowCommandName = "/rpl"; // Slash Command for the config window
    private List<string> worldNames; // List of world names, used to format character names in log files
    private Dictionary<XivChatType, Channel> loggedChannels = new Dictionary<XivChatType, Channel>{
        { XivChatType.Party, new Channel("Party", "({0}) {1}")},
        { XivChatType.CrossParty, new Channel("Cross Party", "({0}) {1}")},
        { XivChatType.Say, new Channel("Say", "{0}: {1}")},
        { XivChatType.TellIncoming, new Channel("Tell", "From {0}: {1}", true)},
        { XivChatType.TellOutgoing, new Channel("Tell", "To {0}: {1}", true)},
        { XivChatType.CustomEmote, new Channel("Emote", "{0}{1}")},
        { XivChatType.StandardEmote, new Channel("Emote", "{0}{1}")},
        { XivChatType.CrossLinkShell1, new Channel("CWLS1", "{0}: {1}")},
        { XivChatType.CrossLinkShell2, new Channel("CWLS2", "{0}: {1}")},
        { XivChatType.CrossLinkShell3, new Channel("CWLS3", "{0}: {1}")},
        { XivChatType.CrossLinkShell4, new Channel("CWLS4", "{0}: {1}")},
        { XivChatType.CrossLinkShell5, new Channel("CWLS5", "{0}: {1}")},
        { XivChatType.CrossLinkShell6, new Channel("CWLS6", "{0}: {1}")},
        { XivChatType.CrossLinkShell7, new Channel("CWLS7", "{0}: {1}")},
        { XivChatType.CrossLinkShell8, new Channel("CWLS8", "{0}: {1}")},
        { XivChatType.Ls1, new Channel("LS1", "{0}: {1}")},
        { XivChatType.Ls2, new Channel("LS2", "{0}: {1}")},
        { XivChatType.Ls3, new Channel("LS3", "{0}: {1}")},
        { XivChatType.Ls4, new Channel("LS4", "{0}: {1}")},
        { XivChatType.Ls5, new Channel("LS5", "{0}: {1}")},
        { XivChatType.Ls6, new Channel("LS6", "{0}: {1}")},
        { XivChatType.Ls7, new Channel("LS7", "{0}: {1}")},
        { XivChatType.Ls8, new Channel("LS8", "{0}: {1}")},
        { XivChatType.Alliance, new Channel("Alliance", "{0}: {1}")}
    };


    // Dalamud Services/Stuff
    private DalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    private IClientState ClientState { get; init; }
    private IDataManager DataManager { get; init; }
    private IChatGui ChatGui { get; init; }
    public IPluginLog Log { get; init; }

    // Plugin UI
    public Configuration Config { get; init; }
    public WindowSystem WindowSystem = new("RPLogger");
    private ConfigWindow ConfigWindow { get; init; }

    /// <summary>
    /// Plugin constructor.
    /// </summary>
    /// <param name="pluginInterface">The interface for our plugin.</param>
    /// <param name="commandManager">The command manager for handling slash commands.</param>
    /// <param name="clientState">Contains game state information.</param>
    /// <param name="chatGui">Handles all chat messages.</param>
    /// <param name="pluginLog">Logger for the plugin.</param>
    public RPLogger(
        DalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IClientState clientState,
        IDataManager dataManager,
        IChatGui chatGui,
        IPluginLog pluginLog)
    {
        
        // Services
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        ChatGui = chatGui;
        Log = pluginLog;
        ClientState = clientState;
        DataManager = dataManager;

        // Plugin Config Stuff
        Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Config.Initialize(PluginInterface);
        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        // Add slash command handler
        CommandManager.AddHandler(ConfigWindowCommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the configuration for RP Logger"
        });

        // Add UI event handlers
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += () =>
        {
            ConfigWindow.IsOpen = true;
        };

        // Add Chat Message event handler
        ChatGui.ChatMessage += OnChatMessage;

        // Initialize and populate world names list
        worldNames = new List<string>();
        DataManager.GetExcelSheet<World>()?.Where(world => world != null && world.Name != null && world.IsPublic).ToList().ForEach(world =>
        {
            if (world.Name != null)
                worldNames.Add(world.Name.ToString());
        });

    }

    /// <summary>
    /// Cleans up/disposes of objects in use by the plugin.
    /// </summary>
    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        CommandManager.RemoveHandler(ConfigWindowCommandName);
        ChatGui.ChatMessage -= OnChatMessage;
        PluginInterface.UiBuilder.Draw -= DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi -= ShowConfig;
    }

    /// <summary>
    /// Method <c>OnCommand</c> is called when the slash command is used.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="args"></param>
    private void OnCommand(string command, string args)
    {
        // Display the config menu
        ConfigWindow.IsOpen = true;
    }

    /// <summary>
    /// Method <c>OnChatMessage</c> is called when a chat message event is fired.
    /// </summary>
    /// <param name="type">The type/channel of the message.</param>
    /// <param name="senderId">The ID of the sender.</param>
    /// <param name="sender">The sender.</param>
    /// <param name="message">The message.</param>
    /// <param name="isHandled">Whether the event's been handled or not.</param>
    internal void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        // this.. shouldn't be possible? I'm assuming this event can't even fire unless you're logged in but just in case lol.
        if (!ClientState.IsLoggedIn || ClientState.LocalPlayer == null || ClientState.LocalPlayer.Name == null || ClientState.LocalPlayer.HomeWorld.GameData == null ||ClientState.LocalPlayer.HomeWorld.GameData.Name == null) return;

        // Check if it's a supported chat channel
        if (!loggedChannels.ContainsKey(type)) return;

        // Check if the channel is enabled in the config
        // Return based on channel logging options
        switch (type)
        {
            case XivChatType.Party or XivChatType.CrossParty:
                if (!Config.PartyLogging) return;
                break;
            case XivChatType.Say:
                if (!Config.SayLogging) return;
                break;
            case XivChatType.TellIncoming or XivChatType.TellOutgoing:
                if (!Config.TellsLogging) return;
                break;
            case XivChatType.CustomEmote:
                if (!Config.CustomEmoteLogging) return;
                break;
            case XivChatType.StandardEmote:
                if (!Config.StandardEmoteLogging) return;
                break;
            case XivChatType.CrossLinkShell1:
                if (!Config.CWLS1Logging) return;
                break;
            case XivChatType.CrossLinkShell2:
                if (!Config.CWLS2Logging) return;
                break;
            case XivChatType.CrossLinkShell3:
                if (!Config.CWLS3Logging) return;
                break;
            case XivChatType.CrossLinkShell4:
                if (!Config.CWLS4Logging) return;
                break;
            case XivChatType.CrossLinkShell5:
                if (!Config.CWLS5Logging) return;
                break;
            case XivChatType.CrossLinkShell6:
                if (!Config.CWLS6Logging) return;
                break;
            case XivChatType.CrossLinkShell7:
                if (!Config.CWLS7Logging) return;
                break;
            case XivChatType.CrossLinkShell8:
                if (!Config.CWLS8Logging) return;
                break;
            case XivChatType.Ls1:
                if (!Config.LS1Logging) return;
                break;
            case XivChatType.Ls2:
                if (!Config.LS2Logging) return;
                break;
            case XivChatType.Ls3:
                if (!Config.LS3Logging) return;
                break;
            case XivChatType.Ls4:
                if (!Config.LS4Logging) return;
                break;
            case XivChatType.Ls5:
                if (!Config.LS5Logging) return;
                break;
            case XivChatType.Ls6:
                if (!Config.LS6Logging) return;
                break;
            case XivChatType.Ls7:
                if (!Config.LS7Logging) return;
                break;
            case XivChatType.Ls8:
                if (!Config.LS8Logging) return;
                break;
            case XivChatType.Alliance:
                if (!Config.AllianceLogging) return;
                break;
        }

        // Write the message to the plugin log
        Log.Debug($"[{type}] {sender}: \"{message.TextValue}\"");

        // Yes, I know the below code is a visual dumpster fire but it works and a cleanup is Soon™
        string playerName = $"{ClientState.LocalPlayer.Name}@{ClientState.LocalPlayer.HomeWorld.GameData.Name}";
        string? senderWorldName = worldNames.FirstOrDefault(sender.TextValue.Contains);

        string senderName = senderWorldName.IsNullOrEmpty() ? string.Concat(CorrectCharacterName(sender.TextValue),"@", ClientState.LocalPlayer.HomeWorld.GameData.Name) : string.Concat(CorrectCharacterName(sender.TextValue.Replace(senderWorldName, "").Trim()),"@",senderWorldName);

        // Time stamp stuff
        DateTimeOffset currentTime = DateTimeOffset.Now;
        string timePrefix = Config.Datestamp || Config.Timestamp ? GetTimePrefix(currentTime) : "";

        // Check if the subdirectories exist, if not create them.
        if (!Directory.Exists(Path.Combine(Config.LogsDirectory, playerName))) Directory.CreateDirectory(Path.Combine(Config.LogsDirectory, playerName));
        if (Config.SeparateTellsBySender && !Directory.Exists(Path.Combine(Config.LogsDirectory, playerName, "Tells"))) Directory.CreateDirectory(Path.Combine(Config.LogsDirectory, playerName, "Tells"));

        // Set the file path and log message
        string fileName = $"{(Config.SeparateTellsBySender && loggedChannels[type].TellsChannel ? senderName : playerName)}{(Config.SeparateLogs ? Config.SeparateTellsBySender && loggedChannels[type].TellsChannel ? "" : $" {loggedChannels[type].Name}" : "")}.txt";
        string filePath = (Config.SeparateTellsBySender && loggedChannels[type].TellsChannel) ? Path.Combine(Config.LogsDirectory, playerName, "Tells", fileName) : Path.Combine(Config.LogsDirectory, playerName, fileName);
        string logMessage = $"{timePrefix}{string.Format(loggedChannels[type].MessageFormat, senderName, message.TextValue)}";

        // If we somehow got here without a message to log, return.
        if (logMessage.IsNullOrEmpty() || filePath.IsNullOrEmpty()) return;
        
        // Write the message to the log file.
        Task.Run(async () => await FileWriteAsync(filePath, logMessage));
    }

    /// <summary>
    /// Method <c>DrawUI</c> all windows registered in the <c>WindowSystem</c>
    /// </summary>
    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    /// <summary>
    /// Method <c>ShowConfig</c> opens the config window.
    /// </summary>
    private void ShowConfig()
    {
        ConfigWindow.IsOpen = true;
    }
    
    /// <summary>
    /// Method <c>FileWriteAsync</c> writes a string to a file asynchronously.
    /// </summary>
    /// <param name="filePath">The full path of the file to be written/appended to.</param>
    /// <param name="messaage">The string to write/append to the file</param>
    /// <returns></returns>
    public async Task FileWriteAsync(string filePath, string messaage)
    {
        using (FileStream stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true))
        using (StreamWriter sw = new StreamWriter(stream))
        {
            await sw.WriteLineAsync(messaage);
        }
    }

    /// <summary>
    /// Remove random extra characters from character names (ex: Party numbers)
    /// </summary>
    /// <param name="name">A character's name</param>
    /// <returns>The corrected name as a string</returns>
    public string CorrectCharacterName(string name)
    {
        return new string(Array.FindAll<char>(name.ToCharArray(), (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-'))));
    }

    /// <summary>
    /// Method <c>GetTimePrefix</c> returns a string containing the current time and date in the format specified in the config.
    /// </summary>
    /// <param name="time">A DateTimeoffset</param>
    /// <returns>A string containing the current time and date in the format specified in the config</returns>
    public string GetTimePrefix(DateTimeOffset time)
    {
        string timeStamp = Config.Timestamp12Hour ? $"{time:hh:mm:ss tt}" : $"{time:HH:mm:ss}";
        string dateStamp = Config.MonthDayYear ? $"{time:MM/dd/yyyy}" : $"{time:dd.MM.yyyy}";

        return $"[{(Config.Datestamp ? dateStamp + "" : "")}{((Config.Datestamp && Config.Timestamp) ? " " : "")}{(Config.Timestamp ? timeStamp : "")}]";
    }
}

