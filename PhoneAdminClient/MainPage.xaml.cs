using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using Microsoft.Phone.Controls;
using PhoneAdminClient.ZpdService;

namespace PhoneAdminClient
{
    public partial class MainPage : PhoneApplicationPage
    {
        private readonly ZPDServiceClient _client;
        // Constructor
        public MainPage()
        {
            _client = new ZPDServiceClient(new BasicHttpBinding(), new EndpointAddress("http://10.0.0.3:8000/zpd"));
            _client.PlayCompleted += PlayCompleted;
            InitializeComponent();
        }

        private void PlayCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (null == e.Error)
            {
                Debug.WriteLine("Play succeeded?");
            }
        }

        private void PlayButtonClick(object sender, EventArgs e)
        {
            _client.PlayAsync(GetAuthPacket());
        }

        private AuthPacket GetAuthPacket()
        {
            var packet = new AuthPacket();
            packet.Timeout = AuthTolkenTimeout.FiveSeconds;

            var now = DateTime.UtcNow;
            var numAdjustedSeconds = 5 + (5 - (now.Second%5));
            var adjustedTime = now.AddSeconds(numAdjustedSeconds);
            // need granularity down to seconds
            var adjustedTimeToSecond = new DateTime(adjustedTime.Year,
                                                        adjustedTime.Month,
                                                        adjustedTime.Day,
                                                        adjustedTime.Hour,
                                                        adjustedTime.Minute,
                                                        adjustedTime.Second);

            const string authString = "d3bug";
            var computedAuthTolken =
                    Sha1HashOfString(adjustedTimeToSecond.ToString("yyyy-MM-dd:HH:mm:ss") + authString);


            packet.Offset = numAdjustedSeconds;
            packet.AuthTolken = computedAuthTolken;
            return packet;
        }


        // Taken from http://dotnetpulse.blogspot.com/2007/12/sha1-hash-calculation-in-c.html
        private static string Sha1HashOfString(string input)
        {
            var buffer = Encoding.Unicode.GetBytes(input);
            var crypto = new SHA256Managed();

            return BitConverter.ToString(crypto.ComputeHash(buffer)).Replace("-", "");
        }
    }
}