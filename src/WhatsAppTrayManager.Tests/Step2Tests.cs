using System;
using System.IO;

namespace WhatsAppTrayManager.Tests;

public class Step2Tests
{
    private readonly string _projectRootPath = "/Users/B246654/vscode_storage/ssi-dk/WhatsAppTrayManager";
    
    [Fact]
    public void VerifyStep2FilesExist()
    {
        string basePath = Path.Combine(_projectRootPath, "src", "WhatsAppTrayManager");
        
        // Check for TrayApplicationContext.cs file
        string trayAppContextPath = Path.Combine(basePath, "TrayApplicationContext.cs");
        Assert.True(File.Exists(trayAppContextPath), "TrayApplicationContext.cs file should exist");
        
        // Check for Program.cs file
        string programPath = Path.Combine(basePath, "Program.cs");
        Assert.True(File.Exists(programPath), "Program.cs file should exist");
        
        // Check file contents to verify they meet step 2 requirements
        string trayAppContextContent = File.ReadAllText(trayAppContextPath);
        string programContent = File.ReadAllText(programPath);
        
        // Verify TrayApplicationContext inherits from ApplicationContext
        Assert.Contains("class TrayApplicationContext : ApplicationContext", trayAppContextContent);
        
        // Verify NotifyIcon is initialized
        Assert.Contains("_notifyIcon", trayAppContextContent);
        Assert.Contains("NotifyIcon", trayAppContextContent);
        Assert.Contains("Visible = true", trayAppContextContent);
        
        // Verify context menu with Exit is created
        Assert.Contains("_contextMenu", trayAppContextContent);
        Assert.Contains("ContextMenuStrip", trayAppContextContent);
        Assert.Contains("Exit", trayAppContextContent);
        
        // Verify that Program.cs runs TrayApplicationContext
        Assert.Contains("Application.Run", programContent);
        Assert.Contains("TrayApplicationContext", programContent);
    }
}