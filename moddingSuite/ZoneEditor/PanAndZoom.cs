﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

internal delegate void Redraw();

namespace ZoneEditor
{
    internal class PanAndZoom
    {
        public static Redraw redraw;
        private static Point mouseDown;
        private static int startx; // offset of image when mouse was pressed
        private static int starty;
        private static int imgx; // current offset of image
        private static int imgy;

        private static bool mousepressed; // true as long as left mousebutton is pressed
        private static float zoom = 1;

        public static MouseEventHandler MouseDown => OnMouseDown;
        public static MouseEventHandler MouseUp => OnMouseUp;
        public static MouseEventHandler MouseMove => OnMouseMove;
        public static MouseEventHandler MouseWheel => OnMouseWheel;

        public static Point fromLocalToGlobal(Point pl)
        {
            float locX = pl.X / zoom - imgx;
            float locY = pl.Y / zoom - imgy;

            return new Point((int)locX, (int)locY);
        }

        public static Point fromGlobalToLocal(Point pg)
        {
            float x = (pg.X + imgx) * zoom;
            float y = (pg.Y + imgy) * zoom;
            return new Point((int)x, (int)y);
        }

        public static void setZoom(float z)
        {
            zoom = z;
        }

        public static void Transform(PaintEventArgs e)
        {
            e.Graphics.ResetTransform();
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.ScaleTransform(zoom, zoom);
            e.Graphics.TranslateTransform(imgx, imgy);
        }

        private static void OnMouseDown(object sender, MouseEventArgs e)
        {
            MouseEventArgs mouse = e;

            if (mouse.Button == MouseButtons.Left)
                if (!mousepressed)
                {
                    mousepressed = true;
                    mouseDown = mouse.Location;
                    startx = imgx;
                    starty = imgy;
                }
        }

        private static void OnMouseUp(object sender, MouseEventArgs e)
        {
            mousepressed = false;
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            MouseEventArgs mouse = e;

            if (mouse.Button == MouseButtons.Left)
            {
                Point mousePosNow = mouse.Location;

                // the distance the mouse has been moved since mouse was pressed
                int deltaX = mousePosNow.X - mouseDown.X;
                int deltaY = mousePosNow.Y - mouseDown.Y;

                // calculate new offset of image based on the current zoom factor
                imgx = (int)(startx + deltaX / zoom);
                imgy = (int)(starty + deltaY / zoom);
                redraw();
                /*foreach (var c in pictureBox1.Controls)
                {
                    if (c is Marker)
                    {
                        var a = c as Marker;
                        a.UpdateMarker();
                    }
                    if (c is Creater)
                    {
                        var a = c as Creater;
                        a.UpdateCreater();
                    }
                }*/

                //pictureBox1.Refresh();
            }
        }

        private static void OnMouseWheel(object obj, MouseEventArgs e)
        {
            Control control = obj as Control;
            float oldzoom = zoom;

            /*if (e.Delta > 0)
            {
                zoom += 0.1F;
            }
            else if (e.Delta < 0)
            {
                zoom = Math.Max(zoom - 0.1F, 0.01F);
            }*/
            zoom *= (float)Math.Pow(1.001, e.Delta);
            MouseEventArgs mouse = e;
            Point mousePosNow = mouse.Location;

            // Where location of the mouse in the pictureframe
            int x = mousePosNow.X - control.Location.X;
            int y = mousePosNow.Y - control.Location.Y;

            // Where in the IMAGE is it now
            int oldimagex = (int)(x / oldzoom);
            int oldimagey = (int)(y / oldzoom);

            // Where in the IMAGE will it be when the new zoom i made
            int newimagex = (int)(x / zoom);
            int newimagey = (int)(y / zoom);

            // Where to move image to keep focus on one point
            imgx = newimagex - oldimagex + imgx;
            imgy = newimagey - oldimagey + imgy;
            redraw();
            /*foreach (var c in pictureBox1.Controls)
            {
                if (c is Marker)
                {
                    var a = c as Marker;
                    a.UpdateMarker();
                }
                if (c is Creater)
                {
                    var a = c as Creater;
                    a.UpdateCreater();
                }
            }
            pictureBox1.Refresh();  // calls imageBox_Paint*/
        }
    }
}
