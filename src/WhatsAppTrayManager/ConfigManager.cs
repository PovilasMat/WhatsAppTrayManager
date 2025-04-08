using System;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using System.Text.Json;

namespace WhatsAppTrayManager
{
    /// <summary>
    /// Manages application configuration settings
    /// </summary>
    public class ConfigManager
    {
        // Registry key for auto-start
        private const string RunRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        
        // App name for registry
        private const string AppName = "WhatsAppTrayManager";
        
        // Path to settings file
        private readonly string _settingsFilePath;
        
        // Settings object
        private AppSettings _settings;
        
        public ConfigManager()
        {
            // Initialize settings file path in user's AppData folder
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppName);
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }
            
            _settingsFilePath = Path.Combine(appDataFolder, "settings.json");
            
            // Load settings
            LoadSettings();
        }
        
        /// <summary>
        /// Load settings from file
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json);
                    
                    // Validate poll interval (ensure it's at least 1 second)
                    if (_settings.PollIntervalSeconds < 1)
                    {
                        _settings.PollIntervalSeconds = 5;
                    }
                }
            }
            catch (JsonException)
            {
                // Malformed JSON, use default settings
                _settings = null;
            }
            catch (Exception)
            {
                // Other exceptions, use default settings
                _settings = null;
            }
            
            // Initialize with defaults if not loaded
            if (_settings == null)
            {
                _settings = new AppSettings
                {
                    AutoStartEnabled = false,
                    MinimizeToTrayOnClose = true,
                    ShowNotifications = true,
                    PollIntervalSeconds = 5 // Default to 5 seconds
                };
            }
        }
        
        /// <summary>
        /// Save settings to file
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception)
            {
                // Failed to save settings, but continue
            }
        }
        
        /// <summary>
        /// Get auto-start enabled setting
        /// </summary>
        public bool GetAutoStartEnabled()
        {
            return _settings.AutoStartEnabled;
        }
        
        /// <summary>
        /// Set auto-start enabled setting
        /// </summary>
        public void SetAutoStartEnabled(bool enabled)
        {
            _settings.AutoStartEnabled = enabled;
            SaveSettings();
            
            // Update registry for Windows startup
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true))
                {
                    if (key != null)
                    {
                        if (enabled)
                        {
                            string appPath = Assembly.GetExecutingAssembly().Location;
                            key.SetValue(AppName, appPath);
                        }
                        else
                        {
                            key.DeleteValue(AppName, false);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Failed to update registry, but continue
            }
        }
        
        /// <summary>
        /// Get minimize to tray on close setting
        /// </summary>
        public bool GetMinimizeToTrayOnClose()
        {
            return _settings.MinimizeToTrayOnClose;
        }
        
        /// <summary>
        /// Set minimize to tray on close setting
        /// </summary>
        public void SetMinimizeToTrayOnClose(bool enabled)
        {
            _settings.MinimizeToTrayOnClose = enabled;
            SaveSettings();
        }
        
        /// <summary>
        /// Get show notifications setting
        /// </summary>
        public bool GetShowNotifications()
        {
            return _settings.ShowNotifications;
        }
        
        /// <summary>
        /// Set show notifications setting
        /// </summary>
        public void SetShowNotifications(bool enabled)
        {
            _settings.ShowNotifications = enabled;
            SaveSettings();
        }

        /// <summary>
        /// Get poll interval in seconds
        /// </summary>
        public int GetPollIntervalSeconds()
        {
            return _settings.PollIntervalSeconds;
        }
        
        /// <summary>
        /// Set poll interval in seconds
        /// </summary>
        public void SetPollIntervalSeconds(int seconds)
        {
            // Ensure the value is at least 1 second
            _settings.PollIntervalSeconds = Math.Max(1, seconds);
            SaveSettings();
        }
    }
    
    /// <summary>
    /// Application settings class
    /// </summary>
    public class AppSettings
    {
        public bool AutoStartEnabled { get; set; }
        public bool MinimizeToTrayOnClose { get; set; }
        public bool ShowNotifications { get; set; }
        public int PollIntervalSeconds { get; set; } = 5; // Default to 5 seconds
    }
}