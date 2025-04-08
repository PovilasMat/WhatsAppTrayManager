using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhatsAppTrayManager
{
    /// <summary>
    /// Monitors the WhatsApp window for unread messages and updates the tray icon
    /// </summary>
    public class MessageMonitor
    {
        // Interval for checking unread messages (in milliseconds)
        private const int CheckInterval = 1000;
        
        // The window manager for accessing WhatsApp
        private WhatsAppWindowManager _windowManager;
        
        // The icon manager for updating the tray icon
        private IconManager _iconManager;
        
        // The notification icon to update
        private NotifyIcon _notifyIcon;
        
        // Cancellation token for stopping the monitoring task
        private CancellationTokenSource _cancellationTokenSource;
        
        // Last known unread count
        private int _lastUnreadCount = 0;
        
        /// <summary>
        /// Creates a new instance of the MessageMonitor
        /// </summary>
        /// <param name="windowManager">The WhatsApp window manager</param>
        /// <param name="iconManager">The icon manager</param>
        /// <param name="notifyIcon">The notification icon to update</param>
        public MessageMonitor(WhatsAppWindowManager windowManager, IconManager iconManager, NotifyIcon notifyIcon)
        {
            _windowManager = windowManager;
            _iconManager = iconManager;
            _notifyIcon = notifyIcon;
        }
        
        /// <summary>
        /// Start monitoring for unread messages
        /// </summary>
        public void Start()
        {
            // Cancel any existing monitoring
            Stop();
            
            // Create a new cancellation token
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Start the monitoring task
            Task.Run(() => MonitorMessages(_cancellationTokenSource.Token));
        }
        
        /// <summary>
        /// Stop monitoring for unread messages
        /// </summary>
        public void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
        
        /// <summary>
        /// Monitor messages in a loop
        /// </summary>
        private async Task MonitorMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Check if WhatsApp is running
                    if (_windowManager.IsWhatsAppRunning())
                    {
                        // Get unread message count
                        int unreadCount = _windowManager.GetUnreadMessageCount();
                        
                        // Update tray icon if count changed
                        if (unreadCount != _lastUnreadCount)
                        {
                            UpdateTrayIcon(unreadCount);
                            _lastUnreadCount = unreadCount;
                        }
                    }
                    else
                    {
                        // Reset unread count if WhatsApp is not running
                        if (_lastUnreadCount != 0)
                        {
                            UpdateTrayIcon(0);
                            _lastUnreadCount = 0;
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore exceptions and continue monitoring
                }
                
                // Wait for the next check interval
                await Task.Delay(CheckInterval, cancellationToken);
            }
        }
        
        /// <summary>
        /// Update the tray icon with the current unread count
        /// </summary>
        private void UpdateTrayIcon(int unreadCount)
        {
            try
            {
                string iconKey;
                
                if (!_windowManager.IsWhatsAppRunning())
                {
                    // Use gray icon if WhatsApp is not running
                    iconKey = IconManager.GRAY_ICON;
                    _notifyIcon.Text = "WhatsApp Tray Manager (not running)";
                }
                else if (unreadCount <= 0)
                {
                    // Use default icon for no unread messages
                    iconKey = IconManager.DEFAULT_ICON;
                    _notifyIcon.Text = "WhatsApp Tray Manager";
                }
                else if (unreadCount >= 1 && unreadCount <= 5)
                {
                    // Use msg-N icon for 1-5 unread messages
                    iconKey = $"msg-{unreadCount}";
                    _notifyIcon.Text = $"WhatsApp Tray Manager ({unreadCount} unread)";
                }
                else
                {
                    // Use msg-5plus icon for more than 5 unread messages
                    iconKey = IconManager.MSG_5PLUS_ICON;
                    _notifyIcon.Text = $"WhatsApp Tray Manager (5+ unread)";
                }
                
                // Update the tray icon
                _notifyIcon.Icon = _iconManager.GetIcon(iconKey);
            }
            catch (Exception)
            {
                // Ignore exceptions when updating tray icon
            }
        }
    }
}