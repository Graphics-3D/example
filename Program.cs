using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using Engine;

Scene.Create(
    new Mesh(
        new Face(
            (10, 0, 0),
            (10, 0, 5),
            (10, 5, 0)
        ),
        new Face(
            (10, 0, 0),
            (10, 0, 5),
            (20, 0, 0)
        ),
        new Face(
            (10, 0, 5),
            (20, 0, 0),
            (20, 0, 5)
        ),
        new Face(
            (20, 0, 0),
            (20, 0, 5),
            (20, 5, 0)
        ),
        new Face(
            (10, 0, 0),
            (10, 5, 0),
            (20, 0, 0)
        ),
        new Face(
            (10, 5, 0),
            (20, 0, 0),
            (20, 5, 0)
        ),
        new Face(
            (10, 0, 5),
            (10, 5, 0),
            (20, 5, 0)
        ),
        new Face(
            (10, 0, 5),
            (20, 0, 5),
            (20, 5, 0)
        )
    )
);

Camera cam = null!;
bool isRunning = true;
Bitmap bmp = null!;
Graphics g = null!;
bool rotate = false;
PointF? desloc = null;
PointF cursor = PointF.Empty;
bool isDown = false;

ApplicationConfiguration.Initialize();

var pb = new PictureBox();
pb.Dock = DockStyle.Fill;

var form = new Form();
form.WindowState = FormWindowState.Maximized;
form.FormBorderStyle = FormBorderStyle.None;
form.Controls.Add(pb);

form.Load += delegate
{
    cam = new Camera(Point3D.Empty, new Vector3(1, 0, 0), new Vector3(0, 1, 0), pb.Width, pb.Height, 20f, 1000);
    bmp = new Bitmap(pb.Width, pb.Height);
    g = Graphics.FromImage(bmp);
    pb.Image = bmp;
};

form.KeyDown += (o, e) =>
{
    switch (e.KeyCode)
    {
        case Keys.Escape:
            isRunning = false;
            Application.Exit();
            break;
        
        case Keys.Space:
            rotate = !rotate;
            break;

        case Keys.W:
            cam.Translate(1, 0, 0);
            break;

        case Keys.S:
            cam.Translate(-1, 0, 0);
            break;
    }
};

pb.MouseDown += (o, e) => isDown = true;

pb.MouseUp += (o, e) => isDown = false;

pb.MouseMove += (o, e) => cursor = e.Location;

pb.MouseWheel += (o, e) =>
{
    cam?.Zoom(e.Delta
        / SystemInformation.MouseWheelScrollLines);
    
    if (cam?.FOV > 1000)
        cam.FOV = (cam.Scale * 1000) / cam.FOV;
};

Application.Idle += delegate
{
    while (isRunning)
    {
        if (isDown && desloc is null)
            desloc = cursor;
        else if (isDown && desloc is not null)
        {
            var dx = cursor.X - desloc.Value.X;
            var dy = cursor.Y - desloc.Value.Y;

            cam?.Translate(0, dx, dy);

            desloc = cursor;
        }
        else if (!isDown)
        {
            desloc = null;
        }

        if (rotate)
            Scene.Current.Meshes[0]
                .Translate(-15, -1.67f, -1.67f)
                .RotateX(
                    MathF.Cos(0.01f),
                    MathF.Sin(0.01f)
                )
                .RotateY(
                    MathF.Cos(0.01f),
                    MathF.Sin(0.01f)
                )
                .RotateZ(
                    MathF.Cos(0.01f),
                    MathF.Sin(0.01f)
                )
                .Translate(15, 1.67f, 1.67f);


        cam?.Render();
        cam?.Draw(g);
        pb?.Refresh();
        Application.DoEvents();
    }
};

Application.Run(form);