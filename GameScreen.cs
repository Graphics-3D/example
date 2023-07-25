using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using Engine;
using Engine.Meshes;

public class Screen : Form
{
    private bool isRunning = true;
    private Camera cam = null!;
    private Graphics g = null!;
    private PictureBox pb = new()
    {
        Dock = DockStyle.Fill
    };

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
            cam = new Camera(new Point3D(-100, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), pb.Width, pb.Height, 1000f, 1000);
            var bmp = new Bitmap(pb.Width, pb.Height);
            g = Graphics.FromImage(bmp);
            pb.Image = bmp;
        };

        this.KeyDown += (o, e) =>
        {
            var key = e.KeyCode;

            if (key == Keys.Escape)
            {
                isRunning = false;
                Application.Exit();
            }

            if (key == Keys.M)
            {
                var rad = 0.1f;
                var sin = MathF.Sin(rad);
                var cos = MathF.Cos(rad);
                cam.RotateZ(cos, sin);
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
    }
    
    public void Run()
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
            Application.DoEvents();
        }
    }

    #region FPS

    private int fps = 0;
    private int precisionFPS = 19;
    private Queue<DateTime> renderTimes = new();
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

    #endregion

    #region Jump

    private bool isJumping = false;
    private float zVel = 0;
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

    #endregion
}