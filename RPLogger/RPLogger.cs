/**
 * TODO:
 * - "Scene Logging"
 * - Optional auto-saving on a timer instead of on every message
 * - Logs file viewer (maybe, idk, probably not)
 * - Per channel logging settings
 * - Per character settings
 **/

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
using RPLogger.Windows;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace RPLogger;

/// <summary>
/// Main class for the <c>RPLogger</c> plugin
/// </summary>
public sealed class RPLogger : IDalamudPlugin
{
    // Settings
    public static string Name => "RP Logger"; // Plugin Name
    public const string ConfigCommand = "/rpl"; // Slash Command for the Config window
    internal readonly Dictionary<XivChatType, Channel> LoggedChannels = new() // Dictionary of supported chat channels with their respective formats/properties
    {
        { XivChatType.Party, new Channel("Party", "({name}) {message}", "{time}")},
        { XivChatType.CrossParty, new Channel("Cross Party", "({name}) {message}", "{time}")},
        { XivChatType.Say, new Channel("Say", "{name}: {message}", "{time} ")},
        { XivChatType.TellIncoming, new Channel("Tell", "From {name}: {message}", "{time} ", true)},
        { XivChatType.TellOutgoing, new Channel("Tell", "To {name}: {message}", "{time} ", true)},
        { XivChatType.CustomEmote, new Channel("Emote", "{name}{message}", "{time} ")},
        { XivChatType.StandardEmote, new Channel("Emote", "{name}{message}", "{time} ")},
        { XivChatType.CrossLinkShell1, new Channel("CWLS1", "{name}: {message}", "{time} ")},
        { XivChatType.CrossLinkShell2, new Channel("CWLS2", "{name}: {message}", "{time} ")},
        { XivChatType.CrossLinkShell3, new Channel("CWLS3", "{name}: {message}", "{time} ")},
        { XivChatType.CrossLinkShell4, new Channel("CWLS4", "{name}: {message}", "{time} ")},
        { XivChatType.CrossLinkShell5, new Channel("CWLS5", "{name}: {message}", "{time} ")},
        { XivChatType.CrossLinkShell6, new Channel("CWLS6", "{name}: {message}", "{time} ")},
        { XivChatType.CrossLinkShell7, new Channel("CWLS7", "{name}: {message}", "{time} ")},
        { XivChatType.CrossLinkShell8, new Channel("CWLS8", "{name}: {message}", "{time} ")},
        { XivChatType.Ls1, new Channel("LS1", "{name}: {message}", "{time} ")},
        { XivChatType.Ls2, new Channel("LS2", "{name}: {message}", "{time} ")},
        { XivChatType.Ls3, new Channel("LS3", "{name}: {message}", "{time} ")},
        { XivChatType.Ls4, new Channel("LS4", "{name}: {message}", "{time} ")},
        { XivChatType.Ls5, new Channel("LS5", "{name}: {message}", "{time} ")},
        { XivChatType.Ls6, new Channel("LS6", "{name}: {message}", "{time} ")},
        { XivChatType.Ls7, new Channel("LS7", "{name}: {message}", "{time} ")},
        { XivChatType.Ls8, new Channel("LS8", "{name}: {message}", "{time} ")},
        { XivChatType.Alliance, new Channel("Alliance", "({name}) {message}", "{time}")},
        { XivChatType.ErrorMessage, new Channel("Tell", "[Error] {message}", "{time}", true)}
    };


    // Dalamud Services/Stuff
    private IDalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    private IClientState ClientState { get; init; }
    private IChatGui ChatGui { get; init; }
    public IPluginLog Log { get; init; }

    // Plugin GUI/Config
    public Configuration Config { get; init; }
    public WindowSystem WindowSystem = new("RPLogger");
    private ConfigWindow ConfigWindow { get; init; }

    // For assigning tell errors to the correct log file
    internal ChatMessage? LastTell = null;

    /// <summary>
    /// Plugin constructor.
    /// </summary>
    /// <param name="PluginInterface">The interface for our plugin.</param>
    /// <param name="CommandManager">The command manager for handling slash commands.</param>
    /// <param name="ClientState">Contains game state information.</param>
    /// <param name="ChatGui">Handles all chat messages.</param>
    /// <param name="Log">Logger for the plugin.</param>
    public RPLogger(IDalamudPluginInterface PluginInterface, ICommandManager CommandManager, IClientState ClientState, IChatGui ChatGui, IPluginLog Log)
    {
        // Services
        this.PluginInterface = PluginInterface;
        this.CommandManager = CommandManager;
        this.ChatGui = ChatGui;
        this.Log = Log;
        this.ClientState = ClientState;

        // Plugin Config Stuff
        Config = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Config.Initialize(this.PluginInterface);
        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        // Add slash command handler
        this.CommandManager.AddHandler(ConfigCommand, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the configuration for RP Logger"
        });

        // Add UI event handlers
        this.PluginInterface.UiBuilder.Draw += DrawUI;
        this.PluginInterface.UiBuilder.OpenConfigUi += () =>
        {
            ConfigWindow.IsOpen = true;
        };

        // Add Logout event handler
        this.ClientState.Logout += (int type, int code) =>
        {
            // Reset LastTell on logout
            LastTell = null;
        };

        // Add Chat Message event handler
        this.ChatGui.ChatMessage += OnChatMessage;
    }

    /// <summary>
    /// Cleans up
    /// </summary>
    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        CommandManager.RemoveHandler(ConfigCommand);
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
        // Display the Config menu
        ConfigWindow.IsOpen = true;
    }

    /// <summary>
    /// Method <c>OnChatMessage</c> is called when a chat message event is fired.
    /// </summary>
    /// <param name="type">The type/channel of the message.</param>
    /// <param name="timestamp">The chat messages' timestamp</param>
    /// <param name="sender">The sender.</param>
    /// <param name="message">The message.</param>
    /// <param name="isHandled">Whether the event's been handled or not.</param>
    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        // this.. shouldn't be possible? I'm assuming this event can't even fire unless you're logged in but just in case lol.
        if (ClientState.LocalPlayer == null || ClientState.LocalPlayer.Name == null || !ClientState.LocalPlayer.HomeWorld.IsValid || ClientState.LocalPlayer.HomeWorld.Value.Name.ToString() == null) return;

        // Check if it's a supported chat channel and if logging is enabled for it
        if (!IsLoggingEnabled(type, message) || !LoggedChannels.TryGetValue(type, out var channelInfo)) return;

        // Write the message to the plugin Log
        Log.Debug($"[{type}] {sender}: \"{message.TextValue}\"");

        // Local var to store the time the message was received
        var currentTime = DateTimeOffset.Now;

        // If the message is a tell, set the LastTell variable
        if (type == XivChatType.ErrorMessage)
        {
            if (!FilterTellErrors(message)) return;
            if (LastTell != null)
            {
                sender = LastTell.Sender;

                // For error messages we use the timestamp of the corresponding tell
                currentTime = LastTell.Timestamp; 

                //LastTell = null;
            }
            else
            {
                Log.Error($"Somehow got an error message without a LastTell set: (type={channelInfo.Name}, senderId={sender}, sender={sender.TextValue}, message=\"{message.TextValue}\")");
                return;
            }

        }

        // Setup time prefix if the user has it enabled
        var timePrefix = (Config.Datestamp || Config.Timestamp) ? channelInfo.TimePrefixFormat.Replace("{time}", GetTimePrefix(currentTime)) : "";

        // Fix character names for log names & entries
        var playerName = ClientState.LocalPlayer.Name;
        var playerWorldName = ClientState.LocalPlayer.HomeWorld.Value.Name.ToString();
        var playerFullName = $"{playerName}@{playerWorldName}";
        
        var senderWorldName = sender.Payloads.OfType<PlayerPayload>().FirstOrDefault()?.World.Value.Name.ToString() ?? playerWorldName;
        var senderName = CorrectCharacterName(sender.TextValue.Replace(senderWorldName, "").Trim());
        var senderFullName = $"{senderName}@{senderWorldName}";


        if (channelInfo.TellsChannel && type != XivChatType.ErrorMessage)
        {
            // If the message is a tell, set the LastTell variable
            LastTell = new ChatMessage(senderFullName, sender, message, currentTime);
        }

        // Check if the subdirectories exist, if not create them.
        try
        {
            if (!Directory.Exists(Path.Combine(Config.LogsDirectory, playerFullName)))
            {
                Directory.CreateDirectory(Path.Combine(Config.LogsDirectory, playerFullName));
            }
            if (Config.SeparateTellsBySender && !Directory.Exists(Path.Combine(Config.LogsDirectory, playerFullName, "Tells")))
            {
                Directory.CreateDirectory(Path.Combine(Config.LogsDirectory, playerFullName, "Tells"));
            }
        }
        catch (IOException e)
        {
            Log.Error($"Something went wrong while trying to create a directory, maybe missing permissions?: {e.StackTrace}");
            return;
        }

        // Set the file path and Log message
        // Yes, I know the below code is a visual dumpster fire. I'm sorry.
        var useSenderForFileName = Config.SeparateTellsBySender && channelInfo.TellsChannel;
        var fileName = $"{(useSenderForFileName ? senderFullName : playerFullName)}{(Config.SeparateLogs ? (useSenderForFileName ? "" : $" {channelInfo.Name}") : "")}.txt";
        var filePath = useSenderForFileName ? Path.Combine(Config.LogsDirectory, playerFullName, "Tells", fileName) : Path.Combine(Config.LogsDirectory, playerFullName, fileName);
        var logMessage = $"{timePrefix}{channelInfo.MessageFormat.Replace("{name}", senderFullName).Replace("{message}", message.TextValue)}";


        // If we somehow got here without a message to Log, throw a tantrum (/s) and return.
        if (logMessage.IsNullOrEmpty() || filePath.IsNullOrEmpty())
        {
            Log.Error($"Something went wrong while trying to Log a message: (type={channelInfo.Name}, sender={sender.TextValue}, message=\"{message.TextValue}\")");
            return;
        }
        
        // Write the message to the Log file.
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
    /// Method <c>ShowConfig</c> opens the Config window.
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
    public static async Task FileWriteAsync(string filePath, string messaage)
    {
        using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true);
        using var sw = new StreamWriter(stream);
        await sw.WriteLineAsync(messaage);
    }

    /// <summary>
    /// Method <c>IsLoggingEnabled</c> checks if a logging a given channel/chat type is enabled in the Config
    /// </summary>
    /// <param name="type">The channel type to check</param>
    /// <returns>If logging a given channel/chat type is enabled in the Config</returns>
    private bool IsLoggingEnabled(XivChatType type, SeString message)
    {
        switch (type)
        {
            case XivChatType.Party or XivChatType.CrossParty:
                if (!Config.PartyLogging) return false;
                break;
            case XivChatType.Say:
                if (!Config.SayLogging) return false;
                break;
            case XivChatType.TellIncoming or XivChatType.TellOutgoing:
                if (!Config.TellsLogging) return false;
                break;
            case XivChatType.CustomEmote:
                if (!Config.CustomEmoteLogging) return false;
                break;
            case XivChatType.StandardEmote:
                if (!Config.StandardEmoteLogging) return false;
                break;
            case XivChatType.CrossLinkShell1:
                if (!Config.CWLS1Logging) return false;
                break;
            case XivChatType.CrossLinkShell2:
                if (!Config.CWLS2Logging) return false;
                break;
            case XivChatType.CrossLinkShell3:
                if (!Config.CWLS3Logging) return false;
                break;
            case XivChatType.CrossLinkShell4:
                if (!Config.CWLS4Logging) return false;
                break;
            case XivChatType.CrossLinkShell5:
                if (!Config.CWLS5Logging) return false;
                break;
            case XivChatType.CrossLinkShell6:
                if (!Config.CWLS6Logging) return false;
                break;
            case XivChatType.CrossLinkShell7:
                if (!Config.CWLS7Logging) return false;
                break;
            case XivChatType.CrossLinkShell8:
                if (!Config.CWLS8Logging) return false;
                break;
            case XivChatType.Ls1:
                if (!Config.LS1Logging) return false;
                break;
            case XivChatType.Ls2:
                if (!Config.LS2Logging) return false;
                break;
            case XivChatType.Ls3:
                if (!Config.LS3Logging) return false;
                break;
            case XivChatType.Ls4:
                if (!Config.LS4Logging) return false;
                break;
            case XivChatType.Ls5:
                if (!Config.LS5Logging) return false;
                break;
            case XivChatType.Ls6:
                if (!Config.LS6Logging) return false;
                break;
            case XivChatType.Ls7:
                if (!Config.LS7Logging) return false;
                break;
            case XivChatType.Ls8:
                if (!Config.LS8Logging) return false;
                break;
            case XivChatType.Alliance:
                if (!Config.AllianceLogging) return false;
                break;
            case XivChatType.ErrorMessage:
                return true;
            default:
                return false;
        }
        return true;
    }

    /// <summary>
    /// Filter to catch errors when sending tells.
    /// </summary>
    /// <param name="message">The chat message in question</param>
    /// <returns>True if the message matched a filter string</returns>
    public static bool FilterTellErrors(SeString message)
    {
        string[] filters = [
            "Your message was not heard. You must wait before using /tell, /say, /yell, or /shout again.",
            "Unable to send /tell. Recipient is in a restricted area.",
            "could not be sent."
            ];

        return filters.Any(message.TextValue.EndsWith);
    }   

    /// <summary>
    /// Remove random extra characters from character names (ex: Party numbers)
    /// </summary>
    /// <param name="name">A character's Name</param>
    /// <returns>The corrected Name as a string</returns>
    public static string CorrectCharacterName(string name)
    {
        return new string(Array.FindAll<char>(name.ToCharArray(), (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-'))));
    }

    /// <summary>
    /// Method <c>GetTimePrefix</c> returns a string containing the current time and date in the format specified in the Config.
    /// </summary>
    /// <param name="time">A DateTimeoffset</param>
    /// <returns>A string containing the current time and date in the format specified in the Config</returns>
    public string GetTimePrefix(DateTimeOffset time)
    {
        var timeStamp = Config.Timestamp12Hour ? $"{time:hh:mm:ss tt}" : $"{time:HH:mm:ss}";
        var dateStamp = Config.MonthDayYear ? $"{time:MM/dd/yyyy}" : $"{time:dd.MM.yyyy}";

        return $"[{(Config.Datestamp ? dateStamp : "")}{((Config.Datestamp && Config.Timestamp) ? " " : "")}{(Config.Timestamp ? timeStamp : "")}]";
    }
}

