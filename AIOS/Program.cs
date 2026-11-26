// Application entry point: configures Windows Forms and opens the login screen.
using AIOS.Forms;

namespace AIOS
{
    // Contains the process-level startup method for the AIOS desktop application.
    internal static class Program
    {
        [STAThread]
        // Initializes WinForms defaults and starts the application's first form.
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Application.Run(new LoginForm());
        }
    }
}