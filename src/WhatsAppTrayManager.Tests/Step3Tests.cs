using System;
using System.IO;

namespace WhatsAppTrayManager.Tests;

public class Step3Tests
{
    private readonly string _projectRootPath = "/Users/B246654/vscode_storage/ssi-dk/WhatsAppTrayManager";
    
    [Fact]
    public void VerifyIconManagerStructure()
    {
        string basePath = Path.Combine(_projectRootPath, "src", "WhatsAppTrayManager");
        string iconManagerPath = Path.Combine(basePath, "IconManager.cs");
        
        // Check for IconManager.cs file
        Assert.True(File.Exists(iconManagerPath), "IconManager.cs file should exist");
        
        // Check file contents to verify they meet step 3 requirements
        string iconManagerContent = File.ReadAllText(iconManagerPath);
        
        // Verify IconManager has constant keys defined
        Assert.Contains("DEFAULT_ICON", iconManagerContent);
        Assert.Contains("GRAY_ICON", iconManagerContent);
        Assert.Contains("MSG_1_ICON", iconManagerContent);
        Assert.Contains("MSG_5PLUS_ICON", iconManagerContent);
        
        // Verify GetIcon method
        Assert.Contains("GetIcon(string key)", iconManagerContent);
        
        // Verify caching is implemented
        Assert.Contains("Dictionary<string, Icon>", iconManagerContent);
        Assert.Contains("_iconCache", iconManagerContent);
    }
    
    [Fact]
    public void VerifyIconManagerIntegration()
    {
        string basePath = Path.Combine(_projectRootPath, "src", "WhatsAppTrayManager");
        string trayAppContextPath = Path.Combine(basePath, "TrayApplicationContext.cs");
        
        // Check file contents to verify IconManager is used in TrayApplicationContext
        string trayAppContextContent = File.ReadAllText(trayAppContextPath);
        
        // Verify TrayApplicationContext uses IconManager
        Assert.Contains("_iconManager", trayAppContextContent);
        Assert.Contains("IconManager", trayAppContextContent);
        Assert.Contains("GetIcon", trayAppContextContent);
    }
}