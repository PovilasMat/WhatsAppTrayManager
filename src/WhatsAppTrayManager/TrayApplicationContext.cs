using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhatsAppTrayManager
{
    /// <summary>
    /// The main application context that manages the tray icon and application lifecycle
    /// </summary>
    public class TrayApplicationContext : ApplicationContext
    {
        // The system tray icon
        private NotifyIcon _notifyIcon;
        
        // The context menu for the tray icon
        private ContextMenuStrip _contextMenu;
        
        // Icon manager to handle tray icon with badge counts
        private IconManager _iconManager;
        
        // Window manager to handle WhatsApp window operations
        private WhatsAppWindowManager _windowManager;
        
        // Configuration manager to handle application settings
        private ConfigManager _configManager;
        
        // Message monitor to check for unread messages
        private MessageMonitor _messageMonitor;
        
        public TrayApplicationContext()
        {
            // Initialize managers first
            _configManager = new ConfigManager();
            _iconManager = new IconManager();
            _windowManager = new WhatsAppWindowManager();
            
            // Then initialize components that use the managers
            InitializeComponents();
            SetupEventHandlers();
            
            // Set up message monitoring
            _messageMonitor = new MessageMonitor(_windowManager, _iconManager, _notifyIcon);
            
            // Update the auto-start menu item based on current setting
            UpdateAutoStartMenuItem();
            
            // Start monitoring for unread messages
            StartMessageMonitoring();
        }
        
        private void InitializeComponents()
        {
            // Create context menu
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Open WhatsApp", null, OnOpenWhatsApp);
            _contextMenu.Items.Add("-"); // Separator
            _contextMenu.Items.Add("Start with Windows", null, OnToggleAutoStart);
            _contextMenu.Items.Add("-"); // Separator
            _contextMenu.Items.Add("Exit", null, OnExit);
            
            // Create tray icon
            _notifyIcon = new NotifyIcon
            {
                Icon = _iconManager.GetIcon(IconManager.DEFAULT_ICON),
                ContextMenuStrip = _contextMenu,
                Text = "WhatsApp Tray Manager",
                Visible = true
            };
        }
        
        private void SetupEventHandlers()
        {
            // Set up double-click handler to open WhatsApp
            _notifyIcon.DoubleClick += OnOpenWhatsApp;
            
            // Set up application exit handler
            Application.ApplicationExit += OnApplicationExit;
            
            // Set up message handler for intercepting WhatsApp close events
            Application.AddMessageFilter(new WhatsAppMessageFilter(this));
        }
        
        private void UpdateAutoStartMenuItem()
        {
            var menuItem = _contextMenu.Items[2] as ToolStripMenuItem;
            if (menuItem != null)
            {
                menuItem.Checked = _configManager.GetAutoStartEnabled();
            }
        }
        
        private void StartMessageMonitoring()
        {
            _messageMonitor.Start();
        }
        
        private void OnOpenWhatsApp(object sender, EventArgs e)
        {
            _windowManager.ShowWhatsAppWindow();
        }
        
        private void OnToggleAutoStart(object sender, EventArgs e)
        {
            bool currentSetting = _configManager.GetAutoStartEnabled();
            _configManager.SetAutoStartEnabled(!currentSetting);
            
            // Update menu item with checkmark
            UpdateAutoStartMenuItem();
        }
        
        private void OnExit(object sender, EventArgs e)
        {
            // Stop message monitoring
            _messageMonitor.Stop();
            
            // Hide tray icon
            _notifyIcon.Visible = false;
            
            // Close WhatsApp if it's running
            _windowManager.HideWhatsAppWindow();
            
            // Dispose components
            _notifyIcon.Dispose();
            
            // Exit application
            Application.Exit();
        }
        
        private void OnApplicationExit(object sender, EventArgs e)
        {
            // Clean up resources
            _messageMonitor?.Stop();
            _notifyIcon?.Dispose();
        }
        
        /// <summary>
        /// Called when WhatsApp is trying to close
        /// </summary>
        public void OnWhatsAppClosing()
        {
            // Check if we should minimize instead of close
            if (_configManager.GetMinimizeToTrayOnClose())
            {
                _windowManager.HideWhatsAppWindow();
            }
        }
        
        /// <summary>
        /// Message filter to intercept WhatsApp window messages
        /// </summary>
        private class WhatsAppMessageFilter : IMessageFilter
        {
            private TrayApplicationContext _context;
            
            public WhatsAppMessageFilter(TrayApplicationContext context)
            {
                _context = context;
            }
            
            public bool PreFilterMessage(ref Message m)
            {
                // TODO: Implement message filtering to catch WhatsApp close events
                return false;
            }
        }
    }
}