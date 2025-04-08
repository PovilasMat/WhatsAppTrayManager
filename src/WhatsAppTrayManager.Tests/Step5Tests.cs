using System;
using System.IO;
using System.Text.Json;

namespace WhatsAppTrayManager.Tests;

public class Step5Tests
{
    private readonly string _projectRootPath = "/Users/B246654/vscode_storage/ssi-dk/WhatsAppTrayManager";
    
    [Fact]
    public void VerifyConfigFileHandlingImplementation()
    {
        string basePath = Path.Combine(_projectRootPath, "src", "WhatsAppTrayManager");
        string configManagerPath = Path.Combine(basePath, "ConfigManager.cs");
        
        // Check for ConfigManager.cs file
        Assert.True(File.Exists(configManagerPath), "ConfigManager.cs file should exist");
        
        // Check file contents to verify step 5 requirements
        string configManagerContent = File.ReadAllText(configManagerPath);
        
        // Verify Settings class exists with required properties
        Assert.Contains("public class AppSettings", configManagerContent);
        Assert.Contains("public bool AutoStartEnabled { get; set; }", configManagerContent);
        Assert.Contains("public int PollIntervalSeconds { get; set; }", configManagerContent);
        
        // Verify Load/Save methods
        Assert.Contains("private void LoadSettings()", configManagerContent);
        Assert.Contains("private void SaveSettings()", configManagerContent);
        
        // Verify error handling for malformed files
        Assert.Contains("catch (JsonException)", configManagerContent);
        Assert.Contains("// Malformed JSON, use default settings", configManagerContent);
        
        // Verify getters and setters
        Assert.Contains("public bool GetAutoStartEnabled()", configManagerContent);
        Assert.Contains("public void SetAutoStartEnabled(bool enabled)", configManagerContent);
        Assert.Contains("public int GetPollIntervalSeconds()", configManagerContent);
        Assert.Contains("public void SetPollIntervalSeconds(int seconds)", configManagerContent);
        
        // Verify default values are set
        Assert.Contains("PollIntervalSeconds = 5", configManagerContent);
    }
    
    [Fact]
    public void VerifyValidationLogic()
    {
        string basePath = Path.Combine(_projectRootPath, "src", "WhatsAppTrayManager");
        string configManagerPath = Path.Combine(basePath, "ConfigManager.cs");
        string configManagerContent = File.ReadAllText(configManagerPath);
        
        // Verify validation of poll interval
        Assert.Contains("if (_settings.PollIntervalSeconds < 1)", configManagerContent);
        Assert.Contains("_settings.PollIntervalSeconds = Math.Max(1, seconds);", configManagerContent);
    }
}