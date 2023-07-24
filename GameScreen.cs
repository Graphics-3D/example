using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using Engine;
using Engine.Meshes;

public class Screen : Form
{
    int fps = 0;
    bool isRunning = true;
    Camera cam = null!;
    Graphics g = null!;
    Timer tm = new()
    {
        Interval = 16
    };
    PictureBox pb = new()
    {
        Dock = DockStyle.Fill
    };

    Queue<DateTime> renderTimes = new();

    public Screen()
    {
        renderTimes.Enqueue(DateTime.Now);
        Scene.Create(
            new Cube(new Point3D(2.5f, 2.5f, 2.5f), 5),
            new Cube(new Point3D(10.5f, 10.5f, 10.5f), 5)
        );
        
        this.WindowState = FormWindowState.Maximized;
        this.FormBorderStyle = FormBorderStyle.None;
        this.Controls.Add(pb);

        this.Load += delegate
        {
            cam = new Camera(new Point3D(-100, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0), pb.Width, pb.Height, 1000f, 1000);
            var bmp = new Bitmap(pb.Width, pb.Height);
            g = Graphics.FromImage(bmp);
            pb.Image = bmp;
            tm.Start();
        };

        this.KeyDown += (o, e) =>
        {
            var key = e.KeyCode;

            if (key == Keys.Escape)
            {
                isRunning = false;
                Application.Exit();
            }

            if (key == Keys.W)
                cam.Translate(1, 0, 0);

            else if (key == Keys.S)
                cam.Translate(-1, 0, 0);

            if (key == Keys.A)
                cam.Translate(0, -1, 0);

            else if (key == Keys.D)
                cam.Translate(0, 1, 0);

            if (key == Keys.Space)
                Jump();
        };

        tm.Tick += (o, e) =>
        {
            while (isRunning)
            {
                getFPS();
                checkJump();

                cam?.Render();
                cam?.Draw(g);

                var drawFont = new Font("Arial", 16);
                PointF drawPoint = new PointF(50.0F, 50.0F);
                g.DrawString($"{fps} fps", drawFont, Brushes.Black, drawPoint);
                
                pb?.Refresh();
            }
        };
    }
    
    int precisionFPS = 19;
    void getFPS()
    {
        var now = DateTime.Now;
        renderTimes.Enqueue(now);

        if (renderTimes.Count > precisionFPS)
        {
            DateTime old = renderTimes.Dequeue();
            var time = now - old;
            fps = (int)(precisionFPS / time.TotalSeconds);
        }
    }

    bool isJumping = false;
    float zVel = 0;
    void Jump()
    {
        if (isJumping)
            return;

        zVel = 10;
        isJumping = true;
    }

    void checkJump()
    {
        if (!isJumping)
            return;

        cam?.Translate(0, 0, zVel);
        
        if (cam?.Location.Z < 0)
        {
            cam.Location = cam.Location with
            {
                Z = 0
            };

            isJumping = false;
        }

        zVel -= 1;
    }
}