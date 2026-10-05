using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Win32;
using CoBu.Models;

namespace CoBu.Services
{
    public class SettingsService
    {
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "CoBu";

        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
        private readonly string _settingsFilePath;

        public AppSettings Current { get; private set; } = new();

        public event Action? SettingsChanged;

        public SettingsService(string baseStoragePath)
        {
            _settingsFilePath = Path.Combine(baseStoragePath, "settings.json");
            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                    if (loaded != null)
                    {
                        Current = loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load settings: {ex.Message}");
                Current = new AppSettings();
            }

            // Sync Windows Autostart state with registry
            CheckRegistryAutostart();
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(Current, JsonOptions);
                File.WriteAllText(_settingsFilePath, json);
                ApplyAutostart(Current.StartWithWindows);
                SettingsChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        // Autostart launches silently into the tray (see App.OnStartup)
        private static string? BuildAutostartCommand()
        {
            string? exePath = Environment.ProcessPath;
            return string.IsNullOrEmpty(exePath) ? null : $"\"{exePath}\" --tray";
        }

        private void CheckRegistryAutostart()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
                var val = key?.GetValue(AppName) as string;
                Current.StartWithWindows = val != null;

                // Self-heal an entry written by an older version (no --tray) or pointing to a moved/deleted exe
                string? expected = BuildAutostartCommand();
                if (key != null && val != null && expected != null && val != expected)
                {
                    string oldExe = val.Split('"', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
                    bool sameExe = string.Equals(oldExe, Environment.ProcessPath, StringComparison.OrdinalIgnoreCase);
                    if (sameExe || !File.Exists(oldExe))
                    {
                        key.SetValue(AppName, expected);
                    }
                }
            }
            catch { }
        }

        private void ApplyAutostart(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
                if (key == null) return;

                if (enable)
                {
                    string? command = BuildAutostartCommand();
                    if (command != null)
                    {
                        key.SetValue(AppName, command);
                    }
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying autostart: {ex.Message}");
            }
        }
    }
}
