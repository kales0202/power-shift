using System.Drawing;
using System.Windows.Forms;

namespace PowerShift.Utils;

/// <summary>
/// Custom renderer that removes the border around check marks
/// </summary>
public class BorderlessCheckRenderer : ToolStripProfessionalRenderer
{
    protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
    {
        if (e.Image != null)
        {
            // Draw image without border
            e.Graphics.DrawImage(e.Image, e.ImageRectangle);
        }
    }

    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
    {
        // Draw check mark without border
        var g = e.Graphics;
        var rect = e.ImageRectangle;

        // Draw check mark using Unicode character
        using var textBrush = new SolidBrush(SystemColors.MenuText);
        using var font = new Font("Segoe UI", rect.Height * 0.6f, FontStyle.Regular);

        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        g.DrawString("✔", font, textBrush, rect, format);
    }
}
