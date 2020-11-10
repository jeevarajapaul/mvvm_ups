using System.Configuration;
using System.Windows;
using UPSEmployeeLib.General;

namespace UPSAssessment.UPSEmployeeUI
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public ConnectionAgentWebServer ConnectionAgent { get; private set; }
        public bool IsConnected { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
#if DEBUG
            TextBoxUrl.Text = ConfigurationManager.AppSettings.Get("login_url");
            TextBoxApiKey.Password = ConfigurationManager.AppSettings.Get("login_api_key");
#endif
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            string loginUrl = TextBoxUrl.Text.Trim();
            string loginApiKey = TextBoxApiKey.Password.Trim();
            IsConnected = Login(loginUrl, loginApiKey);
            if (!IsConnected)
            {
                MessageBox.Show($"Failed to login. Please check the credentials.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                TextBoxUrl.Focus();
            }
            else
            {
                Close();
            }
        }

        private bool Login(string connectionUrl, string authApiKey)
        {
            bool isConnected = false;
            ConnectionAgent = ConnectionAgentWebServer.GetInstance(connectionUrl, authApiKey);
#if DEBUG
            isConnected = true;
            isConnected = ConnectionAgent.Connect(5000);
#else
            isConnected = ConnectionAgent.Connect(5000);
#endif

            return isConnected;
        }
    }
}