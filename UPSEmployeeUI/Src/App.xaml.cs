using System.Windows;

namespace UPSAssessment.UPSEmployeeUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow MainWindowUPSEmployeeUI = new MainWindow();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindowUPSEmployeeUI.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
        }
    }
}