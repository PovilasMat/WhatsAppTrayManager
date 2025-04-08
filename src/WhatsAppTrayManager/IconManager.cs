using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace WhatsAppTrayManager
{
    /// <summary>
    /// Manages the tray icon and its badge count overlay
    /// </summary>
    public class IconManager
    {
        // Icon key constants
        public const string DEFAULT_ICON = "default";
        public const string GRAY_ICON = "gray";
        public const string MSG_1_ICON = "msg-1";
        public const string MSG_2_ICON = "msg-2";
        public const string MSG_3_ICON = "msg-3";
        public const string MSG_4_ICON = "msg-4";
        public const string MSG_5_ICON = "msg-5";
        public const string MSG_5PLUS_ICON = "msg-5plus";
        
        // Path to the default icon
        private static readonly string DefaultIconPath = "Resources\\WhatsAppTray.ico";
        
        // Path to the badge icon
        private static readonly string BadgeIconPath = "Resources\\WhatsAppTray_Badge.ico";
        
        // Cached icons to avoid repeatedly loading from disk
        private static Icon _defaultIcon;
        private static Icon _badgeIcon;
        private Dictionary<string, Icon> _iconCache = new Dictionary<string, Icon>();
        
        /// <summary>
        /// Get the default WhatsApp tray icon
        /// </summary>
        public static Icon GetDefaultIcon()
        {
            if (_defaultIcon == null)
            {
                string iconPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    DefaultIconPath);
                
                if (File.Exists(iconPath))
                {
                    _defaultIcon = new Icon(iconPath);
                }
                else
                {
                    // Fallback to system default if icon is missing
                    _defaultIcon = SystemIcons.Application;
                }
            }
            
            return _defaultIcon;
        }
        
        /// <summary>
        /// Get the badge icon for displaying unread counts
        /// </summary>
        public static Icon GetBadgeIcon()
        {
            if (_badgeIcon == null)
            {
                string iconPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    BadgeIconPath);
                
                if (File.Exists(iconPath))
                {
                    _badgeIcon = new Icon(iconPath);
                }
                else
                {
                    // Fallback to system default if icon is missing
                    _badgeIcon = SystemIcons.Information;
                }
            }
            
            return _badgeIcon;
        }
        
        /// <summary>
        /// Creates an icon with a number badge overlay
        /// </summary>
        /// <param name="count">The count to display</param>
        /// <returns>Icon with badge count</returns>
        public Icon CreateBadgeIcon(int count)
        {
            // Generate the icon with the count
            return GenerateIcon(count > 5 ? MSG_5PLUS_ICON : $"msg-{count}");
        }

        /// <summary>
        /// Generates a placeholder icon based on the specified key
        /// </summary>
        /// <param name="key">The key identifying which icon to generate</param>
        /// <returns>A dynamically generated icon</returns>
        private Icon GenerateIcon(string key)
        {
            // Standard icon size
            const int iconSize = 16;
            
            // Create a new bitmap for the icon
            using (Bitmap bitmap = new Bitmap(iconSize, iconSize))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                // Clear the background (transparent)
                graphics.Clear(Color.Transparent);
                
                // Set up smoothing for better quality
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                
                // Draw the icon based on the key
                switch (key)
                {
                    case DEFAULT_ICON:
                        // Green circle for default icon
                        using (Brush brush = new SolidBrush(Color.FromArgb(37, 211, 102))) // WhatsApp green
                        {
                            graphics.FillEllipse(brush, 1, 1, iconSize - 2, iconSize - 2);
                        }
                        break;
                        
                    case GRAY_ICON:
                        // Gray circle for inactive state
                        using (Brush brush = new SolidBrush(Color.FromArgb(200, 200, 200)))
                        {
                            graphics.FillEllipse(brush, 1, 1, iconSize - 2, iconSize - 2);
                        }
                        break;
                        
                    case MSG_1_ICON:
                    case MSG_2_ICON:
                    case MSG_3_ICON:
                    case MSG_4_ICON:
                    case MSG_5_ICON:
                        // Extract the number
                        if (int.TryParse(key.Substring(4, 1), out int count))
                        {
                            // Green circle background
                            using (Brush brush = new SolidBrush(Color.FromArgb(37, 211, 102)))
                            {
                                graphics.FillEllipse(brush, 1, 1, iconSize - 2, iconSize - 2);
                            }
                            
                            // Draw the number in white
                            using (Font font = new Font("Arial", 8, FontStyle.Bold))
                            using (Brush textBrush = new SolidBrush(Color.White))
                            using (StringFormat format = new StringFormat())
                            {
                                format.Alignment = StringAlignment.Center;
                                format.LineAlignment = StringAlignment.Center;
                                
                                graphics.DrawString(count.ToString(), font, textBrush, 
                                    new RectangleF(0, 0, iconSize, iconSize), format);
                            }
                        }
                        break;
                        
                    case MSG_5PLUS_ICON:
                        // Green circle background
                        using (Brush brush = new SolidBrush(Color.FromArgb(37, 211, 102)))
                        {
                            graphics.FillEllipse(brush, 1, 1, iconSize - 2, iconSize - 2);
                        }
                        
                        // Draw "5+" in white
                        using (Font font = new Font("Arial", 6, FontStyle.Bold))
                        using (Brush textBrush = new SolidBrush(Color.White))
                        using (StringFormat format = new StringFormat())
                        {
                            format.Alignment = StringAlignment.Center;
                            format.LineAlignment = StringAlignment.Center;
                            
                            graphics.DrawString("5+", font, textBrush, 
                                new RectangleF(0, 0, iconSize, iconSize), format);
                        }
                        break;
                        
                    default:
                        // Fall back to default green circle
                        using (Brush brush = new SolidBrush(Color.FromArgb(37, 211, 102)))
                        {
                            graphics.FillEllipse(brush, 1, 1, iconSize - 2, iconSize - 2);
                        }
                        break;
                }
                
                // Convert the bitmap to an icon
                IntPtr hIcon = bitmap.GetHicon();
                Icon icon = Icon.FromHandle(hIcon);
                
                // Clone the icon so we can safely destroy the handle
                Icon clonedIcon = (Icon)icon.Clone();
                
                // Clean up the icon handle
                DestroyIcon(hIcon);
                icon.Dispose();
                
                return clonedIcon;
            }
        }
        
        /// <summary>
        /// P/Invoke method to destroy an icon handle to prevent resource leaks
        /// </summary>
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        /// <summary>
        /// Get an icon based on the specified key
        /// </summary>
        /// <param name="key">The key identifying which icon to return (default, gray, msg-1, etc.)</param>
        /// <returns>The icon corresponding to the key</returns>
        public Icon GetIcon(string key)
        {
            // Check if we already have this icon cached
            if (_iconCache.ContainsKey(key))
            {
                return _iconCache[key];
            }
            
            // Generate the icon dynamically and add it to the cache
            Icon icon = GenerateIcon(key);
            _iconCache[key] = icon;
            
            return icon;
        }
    }
}