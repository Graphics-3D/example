using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

using Engine;
using Engine.Meshs;

var cube1 = new Cube(new Point3D(2.5f, 2.5f, 2.5f), 5);
var cube2 = new Cube(new Point3D(10.5f, 10.5f, 10.5f), 5);

Scene.Create(
    cube1,
    cube2
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
    cam = new Camera(new Point3D(-100, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0), pb.Width, pb.Height, 1000f, 1000);
    bmp = new Bitmap(pb.Width, pb.Height);
    g = Graphics.FromImage(bmp);
    pb.Image = bmp;
};

bool isJumping = false;
float zVel = 0;

form.KeyDown += (o, e) =>
{
    var key = e.KeyCode;

    if (key == Keys.Escape)
    {
        isRunning = false;
        Application.Exit();
    }
    
    if (key == Keys.H)
        rotate = !rotate;

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
    
    g.DrawString(cam?.Location.Z.ToString(), new Font("Arial", 16), new SolidBrush(Color.Black), new PointF(100, 100));
    pb?.Refresh();
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

string info = "AAAAAAAAAAAA";
pb.MouseMove += (o, e) =>
{
    info = $"{e.X} {e.Y}";
};

Application.Idle += delegate
{
    while (isRunning)
    {
        checkJump();

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
        {
            foreach (var m in Scene.Current.Meshes)
                m.Translate(-15, -1.67f, -1.67f)
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
        }

        g.DrawString(info, new Font("Arial", 16), new SolidBrush(Color.Black), new PointF(100f,100f));
        cam?.Render();
        cam?.Draw(g);
        pb?.Refresh();
        Application.DoEvents();
    }
};

Application.Run(form);