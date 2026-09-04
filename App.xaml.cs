using System.Windows;

namespace StudentManagementApp
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Launch the Login Window first on startup!
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}