/**
 * Shared colour palette, matching the original OSP Hours Tracker web app's
 * CSS custom properties (App.css), so the desktop client keeps the same
 * visual identity.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Drawing;

namespace OSPTracker.Utils
{
    public static class Theme
    {
        public static readonly Color Primary   = Color.FromArgb(26, 82, 118);   // #1a5276
        public static readonly Color Secondary = Color.FromArgb(41, 128, 185);  // #2980b9
        public static readonly Color Accent    = Color.FromArgb(230, 126, 34);  // #e67e22
        public static readonly Color Success   = Color.FromArgb(39, 174, 96);   // #27ae60
        public static readonly Color Danger    = Color.FromArgb(192, 57, 43);   // #c0392b
        public static readonly Color Bg        = Color.FromArgb(244, 246, 249); // #f4f6f9
        public static readonly Color Surface   = Color.White;

        public const string FontFamily = "Trebuchet MS";
    }
}
