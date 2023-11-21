using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using RPLogger.Windows;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using System;
using Dalamud.Utility;
using System.Collections.Generic;
using Lumina.Excel.GeneratedSheets;
using World = Lumina.Excel.GeneratedSheets.World;
using System.Linq;
using Lumina.Excel;
using System.Text.RegularExpressions;
using System.Reflection;

namespace RPLogger;

/// <summary>
/// Main class for the <c>RPLogger</c> plugin
/// </summary>
public sealed class RPLogger : IDalamudPlugin
{
    // Plugin Settings
    public string Name => "RP Logger";
    private const string CommandName = "/rpl"; 
    private List<string> WorldNames;
    private Dictionary<XivChatType, Channel> LoggedChannels = new Dictionary<XivChatType, Channel>{
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


    // Dalamud Services
    private DalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    private IClientState ClientState { get; init; }
    private IDataManager DataManager { get; init; }
    private IChatGui ChatGui { get; init; }
    private IPluginLog Log { get; init; }
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
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager commandManager,
        [RequiredVersion("1.0")] IClientState clientState,
        [RequiredVersion("1.0")] IDataManager dataManager,
        [RequiredVersion("1.0")] IChatGui chatGui,
        [RequiredVersion("1.0")] IPluginLog pluginLog)
    {
        
        // Services
        this.PluginInterface = pluginInterface;
        this.CommandManager = commandManager;
        this.ChatGui = chatGui;
        this.Log = pluginLog;
        this.ClientState = clientState;
        this.DataManager = dataManager;

        // UI / Commands
        this.WorldNames = new List<string>();
        this.Config = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.Config.Initialize(this.PluginInterface);
        ConfigWindow = new ConfigWindow(this);
            
        WindowSystem.AddWindow(ConfigWindow);

        this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the configuration for RP Logger"
        });

        this.PluginInterface.UiBuilder.Draw += DrawUI;
        this.PluginInterface.UiBuilder.OpenConfigUi += () =>
        {
            ConfigWindow.IsOpen = true;
        };

        this.ChatGui.ChatMessage += OnChatMessage;
        this.DataManager.GetExcelSheet<World>().Where(x => x.Name != null && x.IsPublic).ToList().ForEach(x => WorldNames.Add(x.Name.ToString()));

    }

    /// <summary>
    /// Cleans up/disposes of objects in use by the plugin.
    /// </summary>
    public void Dispose()
    {
        this.WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        this.CommandManager.RemoveHandler(CommandName);
        this.ChatGui.ChatMessage -= OnChatMessage;
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
        // This.. shouldn't be possible? I'm assuming this event can't even fire unless you're logged in but just in case lol.
        if (!this.ClientState.IsLoggedIn || this.ClientState.LocalPlayer == null || this.ClientState.LocalPlayer.Name == null || this.ClientState.LocalPlayer.HomeWorld.GameData == null ||this.ClientState.LocalPlayer.HomeWorld.GameData.Name == null) return;

        // Check if it's a supported chat channel
        if (!this.LoggedChannels.ContainsKey(type)) return;

        // Check if the channel is enabled in the config
        // Return based on channel logging options
        switch (type)
        {
            case XivChatType.Party or XivChatType.CrossParty:
                if (!this.Config.PartyLogging) return;
                break;
            case XivChatType.Say:
                if (!this.Config.SayLogging) return;
                break;
            case XivChatType.TellIncoming or XivChatType.TellOutgoing:
                if (!this.Config.TellsLogging) return;
                break;
            case XivChatType.CustomEmote:
                if (!this.Config.CustomEmoteLogging) return;
                break;
            case XivChatType.StandardEmote:
                if (!this.Config.StandardEmoteLogging) return;
                break;
            case XivChatType.CrossLinkShell1:
                if (!this.Config.CWLS1Logging) return;
                break;
            case XivChatType.CrossLinkShell2:
                if (!this.Config.CWLS2Logging) return;
                break;
            case XivChatType.CrossLinkShell3:
                if (!this.Config.CWLS3Logging) return;
                break;
            case XivChatType.CrossLinkShell4:
                if (!this.Config.CWLS4Logging) return;
                break;
            case XivChatType.CrossLinkShell5:
                if (!this.Config.CWLS5Logging) return;
                break;
            case XivChatType.CrossLinkShell6:
                if (!this.Config.CWLS6Logging) return;
                break;
            case XivChatType.CrossLinkShell7:
                if (!this.Config.CWLS7Logging) return;
                break;
            case XivChatType.CrossLinkShell8:
                if (!this.Config.CWLS8Logging) return;
                break;
            case XivChatType.Ls1:
                if (!this.Config.LS1Logging) return;
                break;
            case XivChatType.Ls2:
                if (!this.Config.LS2Logging) return;
                break;
            case XivChatType.Ls3:
                if (!this.Config.LS3Logging) return;
                break;
            case XivChatType.Ls4:
                if (!this.Config.LS4Logging) return;
                break;
            case XivChatType.Ls5:
                if (!this.Config.LS5Logging) return;
                break;
            case XivChatType.Ls6:
                if (!this.Config.LS6Logging) return;
                break;
            case XivChatType.Ls7:
                if (!this.Config.LS7Logging) return;
                break;
            case XivChatType.Ls8:
                if (!this.Config.LS8Logging) return;
                break;
            case XivChatType.Alliance:
                if (!this.Config.AllianceLogging) return;
                break;
        }

        // Write the message to the plugin log
        this.Log.Debug($"[{type}] {sender}: \"{message.TextValue}\"");

        // Yes, I know the below code is a visual dumpster fire but it works and I'm too lazy to clean it up right now.
        string playerName = $"{this.ClientState.LocalPlayer.Name}@{this.ClientState.LocalPlayer.HomeWorld.GameData.Name}";
        string senderWorldName = this.WorldNames.FirstOrDefault(sender.TextValue.Contains);
        string senderName = senderWorldName.IsNullOrEmpty() ? string.Concat(CorrectCharacterName(sender.TextValue),"@", this.ClientState.LocalPlayer.HomeWorld.GameData.Name) : string.Concat(CorrectCharacterName(sender.TextValue.Replace(senderWorldName, "").Trim()),"@",senderWorldName);

        // Time stamp stuff
        DateTimeOffset currentTime = DateTimeOffset.Now;
        string timeStamp = this.Config.Timestamp12Hour ? $"{currentTime:hh:mm:ss tt}" : $"{currentTime:HH:mm:ss}";
        string dateStamp = this.Config.MonthDayYear ? $"{currentTime:MM/dd/yyyy}" : $"{currentTime:dd.MM.yyyy}";
        string timePrefix = (this.Config.Timestamp || this.Config.Datestamp) ? $"[{(this.Config.Datestamp ? dateStamp + "" : "")}{((this.Config.Datestamp && this.Config.Timestamp) ? " " : "")}{(this.Config.Timestamp ? timeStamp : "")}] " : "";

        // Check if the subdirectories exist, if not create them.
        if (!Directory.Exists(Path.Combine(this.Config.LogsDirectory, playerName))) Directory.CreateDirectory(Path.Combine(this.Config.LogsDirectory, playerName));
        if (this.Config.SeparateTellsBySender && !Directory.Exists(Path.Combine(this.Config.LogsDirectory, playerName, "Tells"))) Directory.CreateDirectory(Path.Combine(this.Config.LogsDirectory, playerName, "Tells"));

        // Set the file path and log message
        string fileName = $"{(this.Config.SeparateTellsBySender && this.LoggedChannels[type].TellsChannel ? senderName : playerName)}{(this.Config.SeparateLogs ? this.Config.SeparateTellsBySender && this.LoggedChannels[type].TellsChannel ? "" : $" {this.LoggedChannels[type].Name}" : "")}.txt";
        string filePath = (this.Config.SeparateTellsBySender && this.LoggedChannels[type].TellsChannel) ? Path.Combine(this.Config.LogsDirectory, playerName, "Tells", fileName) : Path.Combine(this.Config.LogsDirectory, playerName, fileName);
        string logMessage = $"{timePrefix}{string.Format(this.LoggedChannels[type].MessageFormat, senderName, message.TextValue)}";

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
        this.WindowSystem.Draw();
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
}

