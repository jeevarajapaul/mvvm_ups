using log4net;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace UPSAssessment.UPSEmployeeUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        private LoginWindow loginWindow = new LoginWindow();

        public MainWindow()
        {
            InitializeComponent();
            // Below two lines are added to prevent backspace going back up the navigation history in the WPF Page,
            // including the "Backspace" keyboard button.
            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();

            var appender = (log4net.Appender.FileAppender)LogManager.GetRepository().GetAppenders()[0];

            string logPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), @"..\Logs");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            appender.File = Path.Combine(logPath, "ups_assessment_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".log");
            appender.ActivateOptions();

            loginWindow.ShowDialog();
            if (!loginWindow.IsConnected)
            {
                Environment.Exit(1);
            }
        }

        private void RibbonButtonGetAllEmployees_Click(object sender, RoutedEventArgs e)
        {
            ShowQueryEmployeePage(UPSEmployeeLib.ViewModel.EmplyeeViewModelQuery.SearchFilter.ByPageNumber);
        }

        private void RibbonButtonGetEmployeeById_Click(object sender, RoutedEventArgs e)
        {
            ShowQueryEmployeePage(UPSEmployeeLib.ViewModel.EmplyeeViewModelQuery.SearchFilter.ByEmployeeId);
        }

        private void RibbonButtonGetEmployeeByName_Click(object sender, RoutedEventArgs e)
        {
            ShowQueryEmployeePage(UPSEmployeeLib.ViewModel.EmplyeeViewModelQuery.SearchFilter.ByEmployeeName);
        }

        private void ShowQueryEmployeePage(UPSEmployeeLib.ViewModel.EmplyeeViewModelQuery.SearchFilter searchFilter)
        {
            QueryEmployeePage queryEmployeePage = new QueryEmployeePage(loginWindow.ConnectionAgent, searchFilter);
            NavigationService.Navigate(queryEmployeePage);
        }

        private void RibbonButtonAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            ShowUpdateEmployeePage(UPSEmployeeLib.ViewModel.EmployeeViewModelUpdate.UpdateType.AddEmployee);
        }

        private void RibbonButtonEditEmployee_Click(object sender, RoutedEventArgs e)
        {
            ShowUpdateEmployeePage(UPSEmployeeLib.ViewModel.EmployeeViewModelUpdate.UpdateType.EditEmployee);
        }

        private void ShowUpdateEmployeePage(UPSEmployeeLib.ViewModel.EmployeeViewModelUpdate.UpdateType updateType)
        {
            UpdateEmployeePage updateEmployeePage = new UpdateEmployeePage(loginWindow.ConnectionAgent, updateType);
            NavigationService.Navigate(updateEmployeePage);
        }

        private void NavigationWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NavigationWindow navigationWindow = (NavigationWindow)FindName(Content.GetType().Name);
            MessageBoxResult messageBoxResult = MessageBox.Show
                    ("Are you sure to close the application?", $"Confirmation - {Title}",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (messageBoxResult == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}