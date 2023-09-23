using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using moddingSuite.Geometry;
using moddingSuite.Model.Scenario;
using moddingSuite.ZoneEditor.Markers;
using moddingSuite.ZoneEditor.ScenarioItems;

namespace ZoneEditor;

public class Outline
{
    private readonly List<CreaterMarker> creaters = new();
    private readonly List<VertexMarker> markers = new();

    private readonly List<Point> nodes = new();
    private readonly PaintEventHandler paintEvent;
    private Control parent;
    public Possession possession;

    public Outline(List<Point> nodes)
    {
        this.nodes = nodes;
        foreach (Point n in nodes)
        {
            VertexMarker marker = new();
            marker.setPosition(n);
            marker.MouseClick += deleteMarker;
            markers.Add(marker);

            marker.BringToFront();

            CreaterMarker c = new();
            c.MouseClick += createMarker;
            //parent.Controls.Add(c);
            creaters.Add(c);
        }

        paintEvent = paint;
    }

    public Outline(Point center)
    {
        int sideLength = 50;
        center.Offset(-sideLength / 2, -sideLength / 2);
        nodes.Add(PanAndZoom.fromLocalToGlobal(center));


        center.Offset(0, sideLength);
        nodes.Add(PanAndZoom.fromLocalToGlobal(center));

        center.Offset(sideLength, 0);
        nodes.Add(PanAndZoom.fromLocalToGlobal(center));

        center.Offset(0, -sideLength);
        nodes.Add(PanAndZoom.fromLocalToGlobal(center));

        //parent.Controls.Add(this);
        //BringToFront();


        foreach (Point n in nodes)
        {
            VertexMarker marker = new();
            marker.setPosition(n);
            marker.MouseClick += deleteMarker;
            markers.Add(marker);

            marker.BringToFront();

            CreaterMarker c = new();
            c.MouseClick += createMarker;
            //parent.Controls.Add(c);
            creaters.Add(c);
        }

        paintEvent = paint;
    }

    public void attachTo(Control c)
    {
        parent = c;
        parent.Paint += paintEvent;
        c.Controls.AddRange(markers.ToArray());
        c.Controls.AddRange(creaters.ToArray());
    }

    public void detachFrom(Control c)
    {
        parent.Paint -= paintEvent;
        foreach (VertexMarker m in markers) c.Controls.Remove(m);
        foreach (CreaterMarker cr in creaters) c.Controls.Remove(cr);
        parent = null;
    }

    public void paint(object sen, PaintEventArgs e)
    {
        List<Point> pos = markers.Select(x => x.getPosition()).ToList();
        pos.Add(pos.First());

        for (int i = 0; i < pos.Count - 1; i++)
        {
            Point p1 = pos.ElementAt(i);
            Point p2 = pos.ElementAt(i + 1);
            Point p = new((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
            creaters.ElementAt(i).setPosition(p);
        }

        //e.Graphics.DrawArc(Pens.Red, new Rectangle(20, 20, 400, 200), 10, 170);
        //e.Graphics.DrawLines(Pens.AliceBlue, x.ToArray());
        //Console.WriteLine(pos.ToList());
        Color c = new();
        switch (possession)
        {
            case Possession.Redfor:
                c = Color.FromArgb(80, 255, 0, 0);
                break;
            case Possession.Bluefor:
                c = Color.FromArgb(80, 0, 0, 255);
                break;
            case Possession.Neutral:
                c = Color.FromArgb(80, 255, 255, 255);
                break;
        }

        Brush b = new SolidBrush(c);
        PanAndZoom.Transform(e);
        e.Graphics.FillPolygon(b, pos.ToArray());
    }

    public void deleteMarker(object obj, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right || markers.Count <= 3) return;
        VertexMarker m = (VertexMarker)obj;
        parent.Controls.Remove(m);

        int i = markers.IndexOf(m);
        markers.RemoveAt(i);

        if (creaters.Count == i - 1)
        {
        }

        parent.Controls.Remove(creaters.ElementAt(i));
        creaters.RemoveAt(i);
        parent.Invalidate();
    }

    public void createMarker(object obj, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        CreaterMarker creater = (CreaterMarker)obj;


        VertexMarker marker = new();
        marker.setPosition(creater.getPosition());
        marker.MouseClick += deleteMarker;
        markers.Insert(creaters.IndexOf(creater) + 1, marker);
        parent.Controls.Add(marker);
        marker.BringToFront();

        CreaterMarker c = new();
        c.MouseClick += createMarker;
        parent.Controls.Add(c);
        creaters.Insert(creaters.IndexOf(creater) + 1, c);

        parent.Invalidate();
    }

    public void setSelected(bool selected)
    {
        markers.ForEach(x => x.Visible = selected);
        creaters.ForEach(x => x.Visible = selected);
    }

    public List<AreaVertex> getOutline()
    {
        List<AreaVertex> list = new();
        foreach (VertexMarker marker in markers)
        {
            Point3D p = Geometry.convertPoint(marker.getPosition());
            AreaVertex av = new();
            av.X = (float)p.X;
            av.Y = (float)p.Y;
            av.Z = (float)Geometry.groundLevel;
            av.W = (float)Geometry.groundLevel;
            list.Add(av);
        }

        return list;
    }
}
