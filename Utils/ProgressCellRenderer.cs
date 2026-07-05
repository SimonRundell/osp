/**
 * ProgressCellRenderer — paints a percent-used progress bar into a
 * DataGridView cell (green &lt;80%, amber 80-99%, red &gt;=100%), matching
 * the colour thresholds used throughout the app's HTML reports.
 *
 * Wire it up from a grid's CellPainting event for whichever column holds
 * the percentage:
 *
 *   _grid.CellPainting += (s, e) => {
 *       if (e.RowIndex >= 0 && _grid.Columns[e.ColumnIndex].Name == "pct")
 *           ProgressCellRenderer.Paint(e, Convert.ToInt32(e.Value));
 *   };
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace OSPTracker.Utils
{
    public static class ProgressCellRenderer
    {
        /// <summary>Returns the threshold colour for a given percentage (green &lt;80, amber 80-99, red &gt;=100).</summary>
        public static Color ColorFor(int percent) =>
            percent >= 100 ? Theme.Danger : percent >= 80 ? Theme.Accent : Theme.Success;

        /// <summary>Paints a percent-used bar with a centred "N%" label into the cell, then marks the paint as handled.</summary>
        public static void Paint(DataGridViewCellPaintingEventArgs e, int percent)
        {
            e.PaintBackground(e.CellBounds, true);

            const int barHeight = 14;
            var rect = e.CellBounds;
            var barRect = new Rectangle(rect.Left + 4, rect.Top + (rect.Height - barHeight) / 2, rect.Width - 8, barHeight);
            if (barRect.Width < 1) { e.Handled = true; return; }

            using (var trackBrush = new SolidBrush(Color.FromArgb(226, 226, 226)))
                e.Graphics.FillRectangle(trackBrush, barRect);

            int clamped   = Math.Max(0, Math.Min(percent, 100));
            int fillWidth = barRect.Width * clamped / 100;
            Color barColor = ColorFor(percent);
            if (fillWidth > 0)
                using (var fillBrush = new SolidBrush(barColor))
                    e.Graphics.FillRectangle(fillBrush, barRect.Left, barRect.Top, fillWidth, barRect.Height);

            using (var pen = new Pen(Color.FromArgb(180, 180, 180)))
                e.Graphics.DrawRectangle(pen, barRect);

            string label = $"{percent}%";
            var font = e.CellStyle?.Font ?? SystemFonts.DefaultFont;
            var textSize = e.Graphics.MeasureString(label, font);
            var textPos = new PointF(
                barRect.Left + (barRect.Width - textSize.Width) / 2,
                barRect.Top + (barRect.Height - textSize.Height) / 2 - 1);

            // Text sits over the fill only once it covers the mid-point — use white there for contrast.
            bool overFillArea = fillWidth >= barRect.Width / 2;
            using (var textBrush = new SolidBrush(overFillArea ? Color.White : Color.Black))
                e.Graphics.DrawString(label, font, textBrush, textPos);

            e.Handled = true;
        }
    }
}
