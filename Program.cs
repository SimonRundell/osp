/**
 * OSPTracker - WinForms front-end for the OSP Hours Tracker PHP/MySQL back-end.
 *
 * Entry point. Enables visual styles and starts the login form.
 *
 * © 2026 Simon Rundell, Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace OSPTracker
{
    internal static class Program
    {
        /// <summary>Application icon extracted from the EXE at startup and shared across all forms.</summary>
        public static Icon AppIcon { get; private set; }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try { AppIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { /* non-fatal — forms fall back to default icon */ }

            Application.Run(new Forms.LoginForm());
        }
    }
}
