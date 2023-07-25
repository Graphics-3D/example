using System.Windows.Forms;

ApplicationConfiguration.Initialize();

var screen = new Screen();

Application.Idle += delegate
{
    screen.Run();
};

Application.Run(screen);