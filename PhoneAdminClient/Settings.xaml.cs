using System.ComponentModel;
using System.Windows;
using Microsoft.Phone.Controls;

namespace PhoneAdminClient
{
    public partial class Settings
    {
        private readonly SettingsManager _settingsManager;
        public Settings()
        {
            _settingsManager = new SettingsManager();
            InitializeComponent();
            HostBox.Text = _settingsManager.Host;
            PortBox.Text = _settingsManager.Port;
            PasswordBox.Text = _settingsManager.Password;

            BackKeyPress += BackKeyPressed;
        }

        private void BackKeyPressed(object sender, CancelEventArgs e)
        {
            UpdateSettingsAndGoBack();
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateSettingsAndGoBack();
        }

        private void UpdateSettingsAndGoBack()
        {
            _settingsManager.UpdateSettings(HostBox.Text, PortBox.Text, PasswordBox.Text);
            NavigationService.GoBack();
        }
    }
}