using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

namespace WhatsAppTrayManager.Tests;

public class Step4Tests
{
    private readonly string _projectRootPath = "/Users/B246654/vscode_storage/ssi-dk/WhatsAppTrayManager";
    
    [Fact]
    public void VerifyIconGenerationFunctionality()
    {
        string basePath = Path.Combine(_projectRootPath, "src", "WhatsAppTrayManager");
        string iconManagerPath = Path.Combine(basePath, "IconManager.cs");
        
        // Check for IconManager.cs file
        Assert.True(File.Exists(iconManagerPath), "IconManager.cs file should exist");
        
        // Check file contents to verify they meet step 4 requirements
        string iconManagerContent = File.ReadAllText(iconManagerPath);
        
        // Verify the GenerateIcon method exists
        Assert.Contains("private Icon GenerateIcon(string key)", iconManagerContent);
        
        // Verify we're handling all the required icon types
        Assert.Contains("case DEFAULT_ICON:", iconManagerContent);
        Assert.Contains("case GRAY_ICON:", iconManagerContent);
        Assert.Contains("case MSG_1_ICON:", iconManagerContent);
        Assert.Contains("case MSG_5PLUS_ICON:", iconManagerContent);
        
        // Verify we're creating different color circles for different states
        Assert.Contains("Color.FromArgb(37, 211, 102)", iconManagerContent); // WhatsApp green
        Assert.Contains("Color.FromArgb(200, 200, 200)", iconManagerContent); // Gray
        
        // Verify we're drawing text for the message count icons
        Assert.Contains("DrawString(count.ToString()", iconManagerContent);
        Assert.Contains("DrawString(\"5+\"", iconManagerContent);
    }
    
    [Fact]
    public void VerifyIconCaching()
    {
        // File-based verification of caching mechanism
        string basePath = Path.Combine(_projectRootPath, "src", "WhatsAppTrayManager");
        string iconManagerPath = Path.Combine(basePath, "IconManager.cs");
        string iconManagerContent = File.ReadAllText(iconManagerPath);
        
        // Check for cache implementation
        Assert.Contains("Dictionary<string, Icon> _iconCache", iconManagerContent);
        Assert.Contains("_iconCache.ContainsKey(key)", iconManagerContent);
        Assert.Contains("_iconCache[key] = icon", iconManagerContent);
    }
}