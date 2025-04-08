using System;
using System.Windows.Forms;

namespace WhatsAppTrayManager
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable visual styles for the application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Create and run the application context
            Application.Run(new TrayApplicationContext());
        }
    }
}