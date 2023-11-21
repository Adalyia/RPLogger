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
    private XivChatType[] LoggedTypes = {XivChatType.Party, XivChatType.Say, XivChatType.TellIncoming, XivChatType.TellOutgoing, XivChatType.CustomEmote, XivChatType.StandardEmote};

    // Dalamud Services
    private DalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    private IClientState ClientState { get; init; }
    private IDataManager DataManager { get; init; }
    private IChatGui ChatGui { get; init; }
    private IPluginLog Log { get; init; }
    public Configuration Configuration { get; init; }
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
        this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.Configuration.Initialize(this.PluginInterface);
        ConfigWindow = new ConfigWindow(this);
            
        WindowSystem.AddWindow(ConfigWindow);

        this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the configuration for RP Logger"
        });

        this.PluginInterface.UiBuilder.Draw += DrawUI;
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
        if (!this.LoggedTypes.Contains(type)) return;

        // This.. shouldn't be possible? I'm assuming this event can't even fire unless you're logged in but just in case lol.
        if (!this.ClientState.IsLoggedIn || this.ClientState.LocalPlayer == null || this.ClientState.LocalPlayer.Name == null || this.ClientState.LocalPlayer.HomeWorld.GameData == null ||this.ClientState.LocalPlayer.HomeWorld.GameData.Name == null) return;

        // Vars for file writes
        string logMessage = "";
        string fileName = "";
        string senderWorldName = this.WorldNames.FirstOrDefault(sender.TextValue.Contains);
        string senderName = senderWorldName.IsNullOrEmpty() ? CorrectCharacterName(sender.TextValue) : string.Concat(CorrectCharacterName(sender.TextValue.Replace(senderWorldName, "").Trim()),"@",senderWorldName);

        // Process chat messages
        switch (type)
        {
            case XivChatType.Party:
                if (!this.Configuration.PartyLogging) return;

                this.Log.Debug($"Received {type} Chat from {senderName}: \"{message.TextValue}\"");
                logMessage = $"{(this.Configuration.Timestamp ? $"[{DateTimeOffset.Now:yyyy.MM.dd HH:mm:ss}] " : "")}({senderName}) {message.TextValue}";
                fileName = $"{this.ClientState.LocalPlayer.Name}@{this.ClientState.LocalPlayer.HomeWorld.GameData.Name}{(this.Configuration.SeparateLogs ? " Party":"")}.txt";

                break;
            case XivChatType.Say:
                if (!this.Configuration.SayLogging) return;

                this.Log.Debug($"Received {type} Chat from {senderName}: \"{message.TextValue}\"");
                logMessage = $"{(this.Configuration.Timestamp ? $"[{DateTimeOffset.Now:yyyy.MM.dd HH:mm:ss}] " : "")}{senderName}: {message.TextValue}";
                fileName = $"{this.ClientState.LocalPlayer.Name}@{this.ClientState.LocalPlayer.HomeWorld.GameData.Name}{(this.Configuration.SeparateLogs ? " Say" : "")}.txt";

                break;
            case XivChatType.TellIncoming or XivChatType.TellOutgoing:
                if (!this.Configuration.TellsLogging) return;

                this.Log.Debug($"{(type.Equals(XivChatType.TellIncoming) ? "Received" : "Sent")} {type} Chat {(type.Equals(XivChatType.TellIncoming) ? "from" : "to")} {senderName}: \"{message.TextValue}\"");
                logMessage = $"{(this.Configuration.Timestamp ? $"[{DateTimeOffset.Now:yyyy.MM.dd HH:mm:ss}] " : "")}{(type.Equals(XivChatType.TellIncoming) ? "From " : "To ")}{senderName}: {message.TextValue}";
                fileName = $"{this.ClientState.LocalPlayer.Name}@{this.ClientState.LocalPlayer.HomeWorld.GameData.Name}{(this.Configuration.SeparateLogs ? " Tells" : "")}.txt";

                break;
            case XivChatType.CustomEmote or XivChatType.StandardEmote:
                if (!this.Configuration.EmoteLogging) return;

                this.Log.Debug($"Received {type} Chat from {senderName}: \"{message.TextValue}\"");
                
                logMessage = $"{(this.Configuration.Timestamp ? $"[{DateTimeOffset.Now:yyyy.MM.dd HH:mm:ss}] " : "")}{senderName}{message.TextValue}";
                fileName = $"{this.ClientState.LocalPlayer.Name}@{this.ClientState.LocalPlayer.HomeWorld.GameData.Name}{(this.Configuration.SeparateLogs ? " Emote" : "")}.txt";
        
                break;
            default:
                // If the message type doesn't match any of the logged types or the option is disabled, we don't care to log it.
                return;
        }

        

        // If we somehow got here without a message to log, return.
        if (logMessage.IsNullOrEmpty() || fileName.IsNullOrEmpty()) return;

        // Write the message to the log file.
        Task.Run(async () => await FileWriteAsync(Path.Combine(this.Configuration.LogsDirectory, fileName), logMessage));
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

