using System;
using System.Drawing;
using System.Windows.Forms;

namespace moddingSuite.ZoneEditor.Markers;

internal class CreaterMarker : Marker
{
    public CreaterMarker()
    {
        Size = new Size(5, 5);
        Console.WriteLine("creater create");
    }

    public override void paint(object obj, PaintEventArgs e)
    {
        Console.WriteLine("paint creater");
        e.Graphics.FillRectangle(Brushes.Red, new Rectangle(0, 0, 5, 5));
    }
}
