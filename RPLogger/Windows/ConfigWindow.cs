using System;
using System.IO;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.IO.Compression;
using System.Linq;
using Dalamud.Configuration;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using System.Text;
using System.Collections.Generic;

namespace RPLogger.Windows;

public class ConfigWindow : Window, IDisposable
{
    // Config var
    private Configuration config;
    private RPLogger plugin;


    // Constructor
    public ConfigWindow(RPLogger plugin) : base(
        string.Concat(plugin.Name, " Config"), // window title
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = new Vector2(500, 650);
        SizeCondition = ImGuiCond.Always;

        config = plugin.Config;
        this.plugin = plugin;
    }

    // Clean..up?
    public void Dispose() { }

    // Draw the window
    public override void Draw()
    {
        #region Emote Logging Options
        
        // Create grid for our channel logging options
        ImGui.Text("Emote Logging Options"); // Header
        ImGui.BeginTable("EmoteOptionTable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoPadOuterX); // 2 Columns
        ImGui.TableNextColumn();

        // Col 1
        var customEmoteLogging = config.CustomEmoteLogging;
        if (ImGui.Checkbox("Log Custom Emotes", ref customEmoteLogging))
        {
            config.CustomEmoteLogging = customEmoteLogging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 2
        var standardEmoteLogging = config.StandardEmoteLogging;
        if (ImGui.Checkbox("Log Standard Emotes", ref standardEmoteLogging))
        {
            config.StandardEmoteLogging = standardEmoteLogging;
            config.Save();
        }

        ImGui.EndTable();

        #endregion

        ImGui.Separator();

        #region General Channel Logging Options

        ImGui.Text("General Logging Options"); // Header
        ImGui.BeginTable("GeneralChannelsOptionTable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoPadOuterX); // 2 Columns
        ImGui.TableNextColumn();

        // Col 1
        var partyLogging = config.PartyLogging;
        if (ImGui.Checkbox("Log Party Chat", ref partyLogging))
        {
            config.PartyLogging = partyLogging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 2
        var tellsLogging = config.TellsLogging;
        if (ImGui.Checkbox("Log Tells", ref tellsLogging))
        {
            config.TellsLogging = tellsLogging;
            config.Save();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        // Col 1 Row 2
        var sayLogging = config.SayLogging;
        if (ImGui.Checkbox("Log Say Chat", ref sayLogging))
        {
            config.SayLogging = sayLogging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 2 Row 2
        var allianceLogging = config.AllianceLogging;
        if (ImGui.Checkbox("Log Alliance Chat", ref allianceLogging))
        {
            config.AllianceLogging = allianceLogging;
            config.Save();
        }

        ImGui.EndTable();
        #endregion

        ImGui.Separator();

        #region CWLS Options
        ImGui.Text("CWLS Logging Options"); // Header
        ImGui.BeginTable("CWLSOptionsTable", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoPadOuterX); // 4 Columns
        ImGui.TableNextColumn();

        // Col 1
        var cwls1Logging = config.CWLS1Logging;
        if (ImGui.Checkbox("Log CWLS 1", ref cwls1Logging))
        {
            config.CWLS1Logging = cwls1Logging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 2
        var cwls2Logging = config.CWLS2Logging;
        if (ImGui.Checkbox("Log CWLS 2", ref cwls2Logging))
        {
            config.CWLS2Logging = cwls2Logging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 3
        var cwls3Logging = config.CWLS3Logging;
        if (ImGui.Checkbox("Log CWLS 3", ref cwls3Logging))
        {
            config.CWLS3Logging = cwls3Logging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 4
        var cwls4Logging = config.CWLS4Logging;
        if (ImGui.Checkbox("Log CWLS 4", ref cwls4Logging))
        {
            config.CWLS4Logging = cwls4Logging;
            config.Save();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        // Col 1 Row 2
        var cwls5Logging = config.CWLS5Logging;
        if (ImGui.Checkbox("Log CWLS 5", ref cwls5Logging))
        {
            config.CWLS5Logging = cwls5Logging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 2 Row 2
        var cwls6Logging = config.CWLS6Logging;
        if (ImGui.Checkbox("Log CWLS 6", ref cwls6Logging))
        {
            config.CWLS6Logging = cwls6Logging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 3 Row 2
        var cwls7Logging = config.CWLS7Logging;
        if (ImGui.Checkbox("Log CWLS 7", ref cwls7Logging))
        {
            config.CWLS7Logging = cwls7Logging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 4 Row 2
        var cwls8Logging = config.CWLS8Logging;
        if (ImGui.Checkbox("Log CWLS 8", ref cwls8Logging))
        {
            config.CWLS8Logging = cwls8Logging;
            config.Save();
        }

        ImGui.EndTable();
        #endregion

        ImGui.Separator();

        #region LS Options
        ImGui.Text("LS Logging Options"); // Header
        ImGui.BeginTable("LSOptionTable", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoPadOuterX); // 4 Columns
        ImGui.TableNextColumn();

        // Col 1
        var ls1Logging = config.LS1Logging;
        if (ImGui.Checkbox("Log LS 1", ref ls1Logging))
        {
            config.LS1Logging = ls1Logging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 2
        var ls2Logging = config.LS2Logging;
        if (ImGui.Checkbox("Log LS 2", ref ls2Logging))
        {
            config.LS2Logging = ls2Logging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 3
        var ls3Logging = config.LS3Logging;
        if (ImGui.Checkbox("Log LS 3", ref ls3Logging))
        {
            config.LS3Logging = ls3Logging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 4
        var ls4Logging = config.LS4Logging;
        if (ImGui.Checkbox("Log LS 4", ref ls4Logging))
        {
            config.LS4Logging = ls4Logging;
            config.Save();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        // Col 1 Row 2
        var ls5Logging = config.LS5Logging;
        if (ImGui.Checkbox("Log LS 5", ref ls5Logging))
        {
            config.LS5Logging = ls5Logging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 2 Row 2
        var ls6Logging = config.LS6Logging;
        if (ImGui.Checkbox("Log LS 6", ref ls6Logging))
        {
            config.LS6Logging = ls6Logging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 3 Row 2
        var ls7Logging = config.LS7Logging;
        if (ImGui.Checkbox("Log LS 7", ref ls7Logging))
        {
            config.LS7Logging = ls7Logging;
            config.Save();
        }

        ImGui.TableNextColumn();

        // Col 4 Row 2
        var ls8Logging = config.LS8Logging;
        if (ImGui.Checkbox("Log LS 8", ref ls8Logging))
        {
            config.LS8Logging = ls8Logging;
            config.Save();
        }

        ImGui.EndTable();
        #endregion

        ImGui.Separator();

        #region Time options
        ImGui.Text("Time Prefix Options"); // Header

        var timestamp = config.Timestamp;
        if (ImGui.Checkbox("Timestamp Log Messages", ref timestamp))
        {
            config.Timestamp = timestamp;
            config.Save();
        }

        var timestamp12Hour = config.Timestamp12Hour;
        if (ImGui.Checkbox("12-Hour Timestamps (Default is 24)", ref timestamp12Hour))
        {
            config.Timestamp12Hour = timestamp12Hour;
            config.Save();
        }

        var datestamp = config.Datestamp;
        if (ImGui.Checkbox("Datestamp Log Messages", ref datestamp))
        {
            config.Datestamp = datestamp;
            config.Save();
        }

        var monthDayYear = config.MonthDayYear;
        if (ImGui.Checkbox("Month/Day/Year (Default is Day.Month.Year)", ref monthDayYear))
        {
            config.MonthDayYear = monthDayYear;
            config.Save();
        }
        #endregion

        ImGui.Separator();

        #region Tells Options
        ImGui.Text("File Structure Options"); // Header

        var separateLogs = config.SeparateLogs;
        if (ImGui.Checkbox("Split channels into separate log files", ref separateLogs))
        {
            config.SeparateLogs = separateLogs;
            config.Save();
        }

        var separateTellsBySender = config.SeparateTellsBySender;
        if (ImGui.Checkbox("Split tells log by sender", ref separateTellsBySender))
        {
            config.SeparateTellsBySender = separateLogs;
            config.Save();
        }
        #endregion

        ImGui.Separator();

        #region File Options
        ImGui.Text("File Options"); // Header

        var logsDir = config.LogsDirectory;
        if (ImGui.InputText("Logs Directory", ref logsDir, 4096))
        {
            if (Directory.Exists(logsDir))
            {
                config.LogsDirectory = logsDir;
                config.Save();
            }
            
        }

        ImGui.Text(Directory.Exists(logsDir) ? "" : "Error: Chosen folder does not exist. The config will not save until the folder is valid.");

        #endregion

        ImGui.Separator();

        #region Button Row
        if (ImGui.Button("Open Logs Folder"))
        {
            Process.Start("explorer.exe", $@"{config.LogsDirectory}");
        }

        ImGui.SameLine();

        switch (ImGui.GetIO().KeyShift)
        {
            case true:
                
                if (ImGui.Button("Delete Logs"))
                {
                    DeleteFiles(config.LogsDirectory);
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

        switch (ImGui.GetIO().KeyShift)
        {
            case true:

                if (ImGui.Button("Backup & Split Logs"))
                {
                    // Add all files and directories to a .zip archive
                    string zipPath = Path.Combine(config.LogsDirectory, plugin.GetTimePrefix(DateTimeOffset.Now).Replace("/","-").Replace(".","-").Replace(":", "-") + " RPLogger_Backup.zip");
                    string[] excludedExtensions = { ".zip" };

                    using (ZipArchive zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                    {
                        foreach (string filePath in Directory.EnumerateFiles(config.LogsDirectory, "*", SearchOption.AllDirectories)
                            .Where(file => !excludedExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))))
                        {
                            string entryName = Path.GetRelativePath(config.LogsDirectory, filePath);

                            // Replace backslashes with forward slashes for compatibility
                            entryName = entryName.Replace("\\", "/");

                            zipArchive.CreateEntryFromFile(filePath, entryName);
                        }
                    }

                    DeleteFiles(config.LogsDirectory);

                }

                break;
            case false:
                ImGui.BeginDisabled();

                ImGui.Button("Backup & Split Logs");

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.SetTooltip("Hold Shift to Backup/Archive and Clean any existing log files.");
                }

                ImGui.EndDisabled();

                break;
        }


        ImGui.SameLine();

        if (ImGui.Button("Close"))
        {
            IsOpen = false;
        }
        #endregion 
    }

    public void DeleteFiles(string directoryPath, string[]? excludedExtensions = null)
    {
        if (excludedExtensions == null)
        {
            excludedExtensions = new string[] { ".zip" };
        }

        DirectoryInfo directory = new DirectoryInfo(directoryPath);
        foreach (FileInfo file in directory.GetFiles())
        {
            if (excludedExtensions.Contains(file.Extension)) continue;
            file.Delete();
        }
        foreach (DirectoryInfo subDirectory in directory.GetDirectories())
        {
            DeleteFiles(subDirectory.FullName, excludedExtensions);
            subDirectory.Delete();
        }
    }

}
