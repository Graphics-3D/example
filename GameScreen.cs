using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Engine;
using Engine.Core;
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
        
        Cursor.Hide();

        this.WindowState = FormWindowState.Maximized;
        this.FormBorderStyle = FormBorderStyle.None;
        this.Controls.Add(pb);

        this.Load += delegate
        {
            cam = new Camera(new Point3D(-200, 0, 0), new Vector3(1, 1E-10f, 0), new Vector3(0, 1, 0), pb.Width, pb.Height, 1000f, 1000);
            var bmp = new Bitmap(pb.Width, pb.Height);
            g = Graphics.FromImage(bmp);
            pb.Image = bmp;

            CenterScreen = new Point(pb.Width / 2, pb.Height / 2);
        };

        this.KeyUp += KeyBindUp;
        this.KeyDown += KeyBindDown;
        this.pb.MouseMove += MouseControl;
    }

    [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
    private static extern bool SetCursorPos(int X, int Y);

    private Point CenterScreen;
    private void MouseControl(object? o, MouseEventArgs e)
    {
        var sensibility = 1000f;
        var x = -(CenterScreen.X - e.X) / sensibility;
        var y = -(CenterScreen.Y - e.Y) / sensibility;
        
        var sinZ = MathF.Sin(x);
        var cosZ = MathF.Cos(x);

        var sinY = MathF.Sin(y);
        var cosY = MathF.Cos(y);
        
        cam?.RotateGimbalLock(cosY, sinY, cosZ, sinZ);

        // cam?.RotateZ(cosZ, sinZ);
        // cam?.RotateY(cosY, sinY);
        
        SetCursorPos(CenterScreen.X, CenterScreen.Y);
    }

    private float xVel = 0;
    private float yVel = 0;
    private float zVel = 0;
    private float velocity = 2f;
    private float wVelX = 0;
    private float sVelX = 0;
    private float aVelX = 0;
    private float dVelX = 0;
    private float wVelY = 0;
    private float sVelY = 0;
    private float aVelY = 0;
    private float dVelY = 0;
    
    private bool isW = false;
    private bool isS = false;
    private bool isA = false;
    private bool isD = false;

    private void KeyBindDown(object? o, KeyEventArgs e)
    {
        var key = e.KeyCode;
        var unitNormal = Vector3.Normalize(cam.Normal);
        var unitHorizontal = Vector3.Normalize(cam.Horizontal);

        if (key == Keys.Escape)
        {
            isRunning = false;
            Application.Exit();
        }

        if (key == Keys.W)
        {
            if(isW)
            {
                xVel -= wVelX;
                yVel -= wVelY;
            }
            isW = true;

            wVelX = unitNormal.X * velocity; 
            xVel += wVelX;

            wVelY = unitNormal.Y * velocity; 
            yVel += wVelY;
        }

        else if (key == Keys.S)
        {
            if(isS)
            {
                xVel += sVelX;
                yVel += sVelY;
            }
            isS = true;

            sVelX = unitNormal.X * velocity; 
            xVel -= sVelX;

            sVelY = unitNormal.Y * velocity; 
            yVel -= sVelY;
        }

        if (key == Keys.A)
        {
            if(isA)
            {
                xVel += aVelX;
                yVel += aVelY;
            }
            isA = true;

            aVelX = unitHorizontal.X * velocity;
            xVel -= aVelX;

            aVelY = unitHorizontal.Y * velocity;
            yVel -= aVelY;
        }

        else if (key == Keys.D)
        {
            if(isD)
            {
                xVel -= dVelX;
                yVel -= dVelY;
            }
            isD = true;

            dVelX = unitHorizontal.X * velocity;
            xVel += dVelX;

            dVelY = unitHorizontal.Y * velocity;
            yVel += dVelY;
        }
        
        if (key == Keys.Space)
            Jump();
    }

    private void KeyBindUp(object? o, KeyEventArgs e)
    {
        var key = e.KeyCode;

        if (key == Keys.W)
        {
            isW = false;

            xVel -= wVelX;
            yVel -= wVelY;
        }

        if (key == Keys.S)
        {
            isS = false;
            
            xVel += sVelX;
            yVel += sVelY;
        }

        if (key == Keys.A)
        {
            isA = false;
            
            xVel += aVelX;
            yVel += aVelY;
        }
        
        if (key == Keys.D)
        {
            isD = false;
            
            xVel -= dVelX;
            yVel -= dVelY;
        }
    }

    private float minJumpZ = 0;

    private void checkMovement()
    {
        // Vector2 movementVector = new(xVel, yVel);
        // Vector2 movementVectorNormalized = new(0, 0);

        // if (movementVector != Vector2.Zero)
        //     movementVectorNormalized = Vector2.Normalize(movementVector);

        // cam.Translate(movementVectorNormalized.X, movementVectorNormalized.Y, zVel);
        
        cam.Translate(xVel, yVel, zVel);

        foreach (var mesh in Scene.Current.Meshes)
        {
            if (mesh is Cube cube)
            {
                var collisionResult = cube.Collided(cam.Location);
                if (collisionResult.result == CollidedResult.True)
                {
                    cam.Translate(-xVel, -yVel, -zVel);

                        minJumpZ = collisionResult.top;

                    break;
                }
                else
                    minJumpZ = 0;
            }
        }
    }

    public void Run()
    {
        while (isRunning)
        {
            getFPS();
            updateJump();
            checkMovement();

            cam?.Render();
            cam?.Draw(g);

            g.DrawString($"{fps} fps", DefaultFont, Brushes.Red, new PointF(50.0F, 50.0F));
            g.DrawString($"{cam?.Location}", DefaultFont, Brushes.Black, new PointF(50.0F, 100.0F));
            g.DrawString($"{minJumpZ}", DefaultFont, Brushes.Black, new PointF(50.0F, 125.0F));

            var cube = Scene.Current.Meshes[0] as Cube;
            if (cam?.Location is not null)
                g.DrawString($"Collided: {cube?.Collided(cam.Location)}", DefaultFont, Brushes.Black, new PointF(50.0F, 150.0F));



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
    void Jump()
    {
        if (isJumping)
            return;

        zVel = 10;
        isJumping = true;
    }

    void updateJump()
    {
        if (!isJumping)
            return;
        
        zVel -= 1f;

        if (cam?.Location.Z < minJumpZ)
        {
            cam.Location = cam.Location with
            {
                Z = minJumpZ
            };
            
            zVel = 0;
            isJumping = false;
        }
    }

    #endregion
}