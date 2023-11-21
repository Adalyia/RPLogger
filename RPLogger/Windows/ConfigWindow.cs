using System;
using System.IO;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace RPLogger.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private RPLogger Plugin;

    public ConfigWindow(RPLogger plugin) : base(
        string.Concat(plugin.Name, " Config"), // window title
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(500, 250);
        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = plugin.Configuration;
        this.Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        #region Channel Logging Options
        // Create grid for our channel logging options
        ImGui.BeginTable("##configtable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoPadOuterX);

        ImGui.TableNextColumn();
        var emoteLogging = this.Configuration.EmoteLogging;
        if (ImGui.Checkbox("Log Emotes", ref emoteLogging))
        {
            this.Configuration.EmoteLogging = emoteLogging;

            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        var partyLogging = this.Configuration.PartyLogging;
        if (ImGui.Checkbox("Log Party Chat", ref partyLogging))
        {
            this.Configuration.PartyLogging = partyLogging;

            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.Configuration.Save();
        }

        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        var tellsLogging = this.Configuration.TellsLogging;
        if (ImGui.Checkbox("Log Tells", ref tellsLogging))
        {
            this.Configuration.TellsLogging = tellsLogging;

            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.Configuration.Save();
        }
        ImGui.TableNextColumn();


        // can't ref a property, so use a local 
        var sayLogging = this.Configuration.SayLogging;
        if (ImGui.Checkbox("Log Say Chat", ref sayLogging))
        {
            this.Configuration.SayLogging = sayLogging;

            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.Configuration.Save();
        }
        ImGui.TableNextRow();

        ImGui.EndTable();

        #endregion

        ImGui.Separator();

        #region General Options
        var timestamp = this.Configuration.Timestamp;
        if (ImGui.Checkbox("Timestamp Log Messages", ref timestamp))
        {
            this.Configuration.Timestamp = timestamp;

            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.Configuration.Save();
        }

        

        var separateLogs = this.Configuration.SeparateLogs;
        if (ImGui.Checkbox("Split channels into separate log files", ref separateLogs))
        {
            this.Configuration.SeparateLogs = separateLogs;

            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.Configuration.Save();
        }
        #endregion

        ImGui.Separator();

        #region File Options

        var logsDir = this.Configuration.LogsDirectory;
        if (ImGui.InputText("Logs Directory", ref logsDir, 4096))
        {
            if (Directory.Exists(logsDir))
            {
                this.Configuration.LogsDirectory = logsDir;
                this.Configuration.Save();

            }
            
        }
        ImGui.Text(Directory.Exists(logsDir) ? "" : "Error: Chosen folder does not exist. The config will not save until the folder is valid.");

        #endregion

        ImGui.Separator();

        #region Button Row
        if (ImGui.Button("Open Logs Folder"))
        {
            Process.Start("explorer.exe", $@"{this.Configuration.LogsDirectory}");
        }

        ImGui.SameLine();

        switch (ImGui.GetIO().KeyShift)
        {
            case true:
                
                if (ImGui.Button("Delete Logs"))
                {
                    foreach (var file in Directory.GetFiles(this.Configuration.LogsDirectory))
                    {
                        File.Delete(file);
                    }
                }
                
                break;
            case false:
                ImGui.BeginDisabled();

                ImGui.Button("Delete Logs");

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.SetTooltip("Hold SHIFT to delete, this can NOT be reversed.");
                }
                ImGui.EndDisabled();
                
                break;  
        }


        ImGui.SameLine();

        if (ImGui.Button("Close"))
        {
            this.IsOpen = false;
        }
        #endregion 


    }
}
