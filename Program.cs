using System.Drawing;
using System.Windows.Forms;
using Engine;
using Engine.Core;
using Engine.Meshes;

ApplicationConfiguration.Initialize();

Application.SetDefaultFont(new Font("Arial", 16));

var screen = new Screen();

Scene.Create(
    new Cube(new Point3D(0, 0, 2.5f), 5)
    // new Cube(new Point3D(10.5f, 10.5f, 10.5f), 5)
);

Application.Idle += delegate
{
    screen.Run();
};

Application.Run(screen);