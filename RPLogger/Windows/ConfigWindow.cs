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
        this.Size = new Vector2(500, 650);
        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = plugin.Config;
        this.Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        #region Emote Logging Options
        
        // Create grid for our channel logging options
        ImGui.Text("Channel Logging Options"); // Header
        ImGui.BeginTable("EmoteOptionTable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoPadOuterX); // 2 Columns
        ImGui.TableNextColumn();

        // Col 1
        var customEmoteLogging = this.Configuration.CustomEmoteLogging;
        if (ImGui.Checkbox("Log Custom Emotes", ref customEmoteLogging))
        {
            this.Configuration.CustomEmoteLogging = customEmoteLogging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 2
        var standardEmoteLogging = this.Configuration.StandardEmoteLogging;
        if (ImGui.Checkbox("Log Standard Emotes", ref standardEmoteLogging))
        {
            this.Configuration.StandardEmoteLogging = standardEmoteLogging;
            this.Configuration.Save();
        }

        ImGui.EndTable();

        #endregion

        ImGui.Separator();

        #region General Channel Logging Options

        ImGui.Text("General Channel Logging Options"); // Header
        ImGui.BeginTable("GeneralChannelsOptionTable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoPadOuterX); // 2 Columns
        ImGui.TableNextColumn();

        // Col 1
        var partyLogging = this.Configuration.PartyLogging;
        if (ImGui.Checkbox("Log Party Chat", ref partyLogging))
        {
            this.Configuration.PartyLogging = partyLogging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 2
        var tellsLogging = this.Configuration.TellsLogging;
        if (ImGui.Checkbox("Log Tells", ref tellsLogging))
        {
            this.Configuration.TellsLogging = tellsLogging;
            this.Configuration.Save();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        // Col 1 Row 2
        var sayLogging = this.Configuration.SayLogging;
        if (ImGui.Checkbox("Log Say Chat", ref sayLogging))
        {
            this.Configuration.SayLogging = sayLogging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 2 Row 2
        var allianceLogging = this.Configuration.AllianceLogging;
        if (ImGui.Checkbox("Log Alliance Chat", ref allianceLogging))
        {
            this.Configuration.AllianceLogging = allianceLogging;
            this.Configuration.Save();
        }

        ImGui.EndTable();
        #endregion

        ImGui.Separator();

        #region CWLS Options
        ImGui.Text("CWLS Logging Options"); // Header
        ImGui.BeginTable("CWLSOptionsTable", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoPadOuterX); // 4 Columns
        ImGui.TableNextColumn();

        // Col 1
        var cwls1Logging = this.Configuration.CWLS1Logging;
        if (ImGui.Checkbox("Log CWLS 1", ref cwls1Logging))
        {
            this.Configuration.CWLS1Logging = cwls1Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 2
        var cwls2Logging = this.Configuration.CWLS2Logging;
        if (ImGui.Checkbox("Log CWLS 2", ref cwls2Logging))
        {
            this.Configuration.CWLS2Logging = cwls2Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 3
        var cwls3Logging = this.Configuration.CWLS3Logging;
        if (ImGui.Checkbox("Log CWLS 3", ref cwls3Logging))
        {
            this.Configuration.CWLS3Logging = cwls3Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 4
        var cwls4Logging = this.Configuration.CWLS4Logging;
        if (ImGui.Checkbox("Log CWLS 4", ref cwls4Logging))
        {
            this.Configuration.CWLS4Logging = cwls4Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        // Col 1 Row 2
        var cwls5Logging = this.Configuration.CWLS5Logging;
        if (ImGui.Checkbox("Log CWLS 5", ref cwls5Logging))
        {
            this.Configuration.CWLS5Logging = cwls5Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 2 Row 2
        var cwls6Logging = this.Configuration.CWLS6Logging;
        if (ImGui.Checkbox("Log CWLS 6", ref cwls6Logging))
        {
            this.Configuration.CWLS6Logging = cwls6Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 3 Row 2
        var cwls7Logging = this.Configuration.CWLS7Logging;
        if (ImGui.Checkbox("Log CWLS 7", ref cwls7Logging))
        {
            this.Configuration.CWLS7Logging = cwls7Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 4 Row 2
        var cwls8Logging = this.Configuration.CWLS8Logging;
        if (ImGui.Checkbox("Log CWLS 8", ref cwls8Logging))
        {
            this.Configuration.CWLS8Logging = cwls8Logging;
            this.Configuration.Save();
        }

        ImGui.EndTable();
        #endregion

        ImGui.Separator();

        #region LS Options
        ImGui.Text("LS Logging Options"); // Header
        ImGui.BeginTable("LSOptionTable", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoPadOuterX); // 4 Columns
        ImGui.TableNextColumn();

        // Col 1
        var ls1Logging = this.Configuration.LS1Logging;
        if (ImGui.Checkbox("Log LS 1", ref ls1Logging))
        {
            this.Configuration.LS1Logging = ls1Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 2
        var ls2Logging = this.Configuration.LS2Logging;
        if (ImGui.Checkbox("Log LS 2", ref ls2Logging))
        {
            this.Configuration.LS2Logging = ls2Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 3
        var ls3Logging = this.Configuration.LS3Logging;
        if (ImGui.Checkbox("Log LS 3", ref ls3Logging))
        {
            this.Configuration.LS3Logging = ls3Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 4
        var ls4Logging = this.Configuration.LS4Logging;
        if (ImGui.Checkbox("Log LS 4", ref ls4Logging))
        {
            this.Configuration.LS4Logging = ls4Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        // Col 1 Row 2
        var ls5Logging = this.Configuration.LS5Logging;
        if (ImGui.Checkbox("Log LS 5", ref ls5Logging))
        {
            this.Configuration.LS5Logging = ls5Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 2 Row 2
        var ls6Logging = this.Configuration.LS6Logging;
        if (ImGui.Checkbox("Log LS 6", ref ls6Logging))
        {
            this.Configuration.LS6Logging = ls6Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 3 Row 2
        var ls7Logging = this.Configuration.LS7Logging;
        if (ImGui.Checkbox("Log LS 7", ref ls7Logging))
        {
            this.Configuration.LS7Logging = ls7Logging;
            this.Configuration.Save();
        }

        ImGui.TableNextColumn();

        // Col 4 Row 2
        var ls8Logging = this.Configuration.LS8Logging;
        if (ImGui.Checkbox("Log LS 8", ref ls8Logging))
        {
            this.Configuration.LS8Logging = ls8Logging;
            this.Configuration.Save();
        }

        ImGui.EndTable();
        #endregion

        ImGui.Separator();

        #region Time options
        ImGui.Text("Time Prefix Options"); // Header

        var timestamp = this.Configuration.Timestamp;
        if (ImGui.Checkbox("Timestamp Log Messages", ref timestamp))
        {
            this.Configuration.Timestamp = timestamp;
            this.Configuration.Save();
        }

        var timestamp12Hour = this.Configuration.Timestamp12Hour;
        if (ImGui.Checkbox("12-Hour Timestamps (Default is 24)", ref timestamp12Hour))
        {
            this.Configuration.Timestamp12Hour = timestamp12Hour;
            this.Configuration.Save();
        }

        var datestamp = this.Configuration.Datestamp;
        if (ImGui.Checkbox("Datestamp Log Messages", ref datestamp))
        {
            this.Configuration.Datestamp = datestamp;
            this.Configuration.Save();
        }

        var monthDayYear = this.Configuration.MonthDayYear;
        if (ImGui.Checkbox("Month/Day/Year (Default is Day.Month.Year)", ref monthDayYear))
        {
            this.Configuration.MonthDayYear = monthDayYear;
            this.Configuration.Save();
        }
        #endregion

        ImGui.Separator();

        #region Tells Options
        ImGui.Text("File Structure Options"); // Header

        var separateLogs = this.Configuration.SeparateLogs;
        if (ImGui.Checkbox("Split channels into separate log files", ref separateLogs))
        {
            this.Configuration.SeparateLogs = separateLogs;
            this.Configuration.Save();
        }

        var separateTellsBySender = this.Configuration.SeparateTellsBySender;
        if (ImGui.Checkbox("Split tells log by sender", ref separateTellsBySender))
        {
            this.Configuration.SeparateTellsBySender = separateLogs;
            this.Configuration.Save();
        }
        #endregion

        ImGui.Separator();

        #region File Options
        ImGui.Text("File Options"); // Header

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
