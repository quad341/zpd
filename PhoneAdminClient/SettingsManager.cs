using System.IO.IsolatedStorage;

namespace PhoneAdminClient
{
    public class SettingsManager
    {
        private readonly IsolatedStorageSettings _appSettings = IsolatedStorageSettings.ApplicationSettings;
        private static SettingsManager s_instance;

        private const string DEFAULT_HOST = "10.0.0.3";
        private const string DEFAULT_PORT = "8000";
        private const string DEFAULT_PASSWORD = "d3bug";

        public string Host { get; private set; }
        public string Port { get; private set; }
        public string Password { get; private set; }
        public static SettingsManager Instance
        {
            get
            {
                if (null == s_instance)
                {
                    s_instance = new SettingsManager();
                }
                return s_instance;
            }
        }

        private SettingsManager()
        {
            Host = _appSettings.Contains("Host") ? _appSettings["Host"] as string : DEFAULT_HOST;
            Port = _appSettings.Contains("Port") ? _appSettings["Port"] as string : DEFAULT_PORT;
            Password = _appSettings.Contains("Password") ? _appSettings["Password"] as string : DEFAULT_PASSWORD;
        }

        public void UpdateSettings(string host, string port, string password)
        {
            UpdateSetting("Host", host);
            UpdateSetting("Port", port);
            UpdateSetting("Password", password);
            _appSettings.Save();

            Host = host;
            Port = port;
            Password = password;
        }

        private void UpdateSetting(string key, string value)
        {
            if (_appSettings.Contains(key))
            {
                _appSettings[key] = value;
            }
            else
            {
                _appSettings.Add(key, value);
            }
        }
    }
}