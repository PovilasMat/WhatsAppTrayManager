using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WhatsAppTrayManager
{
    /// <summary>
    /// Manages WhatsApp Desktop window operations
    /// </summary>
    public class WhatsAppWindowManager
    {
        // WhatsApp process name
        private const string WhatsAppProcessName = "WhatsApp";
        
        // Window handle of the WhatsApp window
        private IntPtr _whatsAppWindowHandle = IntPtr.Zero;
        
        // Store window position and state for restoration
        private WindowPlacement _lastWindowPlacement;
        
        // Native methods for window operations
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);
        
        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);
        
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        
        // Constants for window operations
        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_RESTORE = 9;
        
        // Delegate for window enumeration
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        
        // Structure for window placement
        [StructLayout(LayoutKind.Sequential)]
        public struct WindowPlacement
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }
        
        public WhatsAppWindowManager()
        {
            // Initialize the window placement structure
            _lastWindowPlacement.length = Marshal.SizeOf(_lastWindowPlacement);
        }
        
        /// <summary>
        /// Find the WhatsApp Desktop window handle
        /// </summary>
        public bool FindWhatsAppWindow()
        {
            // Clear previous handle
            _whatsAppWindowHandle = IntPtr.Zero;
            
            // Try to find WhatsApp window by enumerating windows
            EnumWindows(EnumWindowsCallback, IntPtr.Zero);
            
            return _whatsAppWindowHandle != IntPtr.Zero;
        }
        
        /// <summary>
        /// Callback for EnumWindows to find WhatsApp window
        /// </summary>
        private bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            // Get window title
            StringBuilder sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            string windowTitle = sb.ToString();
            
            // Check if this is a WhatsApp window
            if (windowTitle.Contains("WhatsApp"))
            {
                _whatsAppWindowHandle = hWnd;
                return false; // Stop enumeration
            }
            
            return true; // Continue enumeration
        }
        
        /// <summary>
        /// Show the WhatsApp window and bring it to the foreground
        /// </summary>
        public void ShowWhatsAppWindow()
        {
            if (_whatsAppWindowHandle == IntPtr.Zero && !FindWhatsAppWindow())
            {
                // WhatsApp isn't running, try to start it
                StartWhatsApp();
                return;
            }
            
            // Restore window placement if available
            if (_lastWindowPlacement.length > 0)
            {
                SetWindowPlacement(_whatsAppWindowHandle, ref _lastWindowPlacement);
            }
            else if (IsIconic(_whatsAppWindowHandle))
            {
                // Restore if minimized
                ShowWindow(_whatsAppWindowHandle, SW_RESTORE);
            }
            else
            {
                // Show normal
                ShowWindow(_whatsAppWindowHandle, SW_SHOWNORMAL);
            }
            
            // Bring to foreground
            SetForegroundWindow(_whatsAppWindowHandle);
        }
        
        /// <summary>
        /// Hide the WhatsApp window
        /// </summary>
        public void HideWhatsAppWindow()
        {
            if (_whatsAppWindowHandle == IntPtr.Zero && !FindWhatsAppWindow())
            {
                return;
            }
            
            // Save current window placement for later restoration
            GetWindowPlacement(_whatsAppWindowHandle, ref _lastWindowPlacement);
            
            // Hide the window
            ShowWindow(_whatsAppWindowHandle, SW_HIDE);
        }
        
        /// <summary>
        /// Start the WhatsApp Desktop application
        /// </summary>
        private void StartWhatsApp()
        {
            try
            {
                // Try to start WhatsApp via Start Menu
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = "shell:AppsFolder\\5319275A.WhatsAppDesktop_cv1g1gvanyjgm!App"
                });
            }
            catch (Exception)
            {
                // Fallback to direct executable path if Start Menu approach fails
                try
                {
                    // Try to find the WhatsApp executable in Program Files
                    string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    string whatsAppPath = $"{programFiles}\\WindowsApps\\WhatsApp\\WhatsApp.exe";
                    
                    if (System.IO.File.Exists(whatsAppPath))
                    {
                        Process.Start(whatsAppPath);
                    }
                    else
                    {
                        // Try the x86 Program Files folder
                        programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                        whatsAppPath = $"{programFiles}\\WindowsApps\\WhatsApp\\WhatsApp.exe";
                        
                        if (System.IO.File.Exists(whatsAppPath))
                        {
                            Process.Start(whatsAppPath);
                        }
                    }
                }
                catch (Exception)
                {
                    // Final fallback: try to run from Start Menu using WhatsApp name
                    Process.Start("WhatsApp");
                }
            }
        }
        
        /// <summary>
        /// Check if WhatsApp is running
        /// </summary>
        public bool IsWhatsAppRunning()
        {
            return Process.GetProcessesByName(WhatsAppProcessName).Length > 0 || 
                   (_whatsAppWindowHandle != IntPtr.Zero || FindWhatsAppWindow());
        }
        
        /// <summary>
        /// Get the unread message count from WhatsApp window title
        /// </summary>
        public int GetUnreadMessageCount()
        {
            if (_whatsAppWindowHandle == IntPtr.Zero && !FindWhatsAppWindow())
            {
                return 0;
            }
            
            // Get window title
            StringBuilder sb = new StringBuilder(256);
            GetWindowText(_whatsAppWindowHandle, sb, sb.Capacity);
            string windowTitle = sb.ToString();
            
            // Parse unread count from window title, format is typically "WhatsApp (3)"
            try
            {
                if (windowTitle.Contains("(") && windowTitle.Contains(")"))
                {
                    int startIndex = windowTitle.LastIndexOf("(") + 1;
                    int endIndex = windowTitle.LastIndexOf(")");
                    string countStr = windowTitle.Substring(startIndex, endIndex - startIndex);
                    
                    if (int.TryParse(countStr, out int count))
                    {
                        return count;
                    }
                }
            }
            catch (Exception)
            {
                // Parsing failed, assume no unread messages
            }
            
            return 0;
        }
    }
}