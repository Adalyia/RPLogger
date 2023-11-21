using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Utility;
using System;
using System.IO;

namespace RPLogger;


/// <summary>
/// Config class for the plugin.
/// </summary>
[Serializable]
public class Configuration : IPluginConfiguration
{
    // Config version 
    public int Version { get; set; } = 0;

    // Options
    public bool PartyLogging { get; set; } = true;
    public bool CustomEmoteLogging { get; set; } = true;
    public bool StandardEmoteLogging { get; set; } = false;
    public bool SayLogging { get; set; } = true;
    public bool TellsLogging { get; set; } = true;
    public bool CWLS1Logging { get; set; } = false;
    public bool CWLS2Logging { get; set; } = false;
    public bool CWLS3Logging { get; set; } = false;
    public bool CWLS4Logging { get; set; } = false;
    public bool CWLS5Logging { get; set; } = false;
    public bool CWLS6Logging { get; set; } = false;
    public bool CWLS7Logging { get; set; } = false;
    public bool CWLS8Logging { get; set; } = false;
    public bool LS1Logging { get; set; } = false;
    public bool LS2Logging { get; set; } = false;
    public bool LS3Logging { get; set; } = false;
    public bool LS4Logging { get; set; } = false;
    public bool LS5Logging { get; set; } = false;
    public bool LS6Logging { get; set; } = false;
    public bool LS7Logging { get; set; } = false;
    public bool LS8Logging { get; set; } = false;
    public bool AllianceLogging { get; set; } = false;

    public bool Timestamp { get; set; } = true;
    public bool Timestamp12Hour { get; set; } = false;
    public bool Datestamp { get; set; } = true;
    public bool MonthDayYear { get; set; } = false;

    public bool SeparateLogs { get; set; } = true;
    public bool SeparateTellsBySender { get; set; } = true;
    public string LogsDirectory { get; set; } = "";


    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? PluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        this.PluginInterface = pluginInterface;
        if (this.LogsDirectory.IsNullOrEmpty())
        {
            this.LogsDirectory = this.PluginInterface!.GetPluginConfigDirectory();
        }

    }

    public void Save()
    {
        this.PluginInterface!.SavePluginConfig(this);
    }
}

