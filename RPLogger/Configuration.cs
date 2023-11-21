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
    public bool EmoteLogging { get; set; } = true;
    public bool SayLogging { get; set; } = true;
    public bool TellsLogging { get; set; } = true;
    public bool Timestamp { get; set; } = true;
    public bool SeparateLogs { get; set; } = true;
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

