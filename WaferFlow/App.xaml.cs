using System.Windows;
using WaferFlow.Views.Windows;

namespace WaferFlow
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Открываем окно входа
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}