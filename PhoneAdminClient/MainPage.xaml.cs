using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using PhoneAdminClient.ZpdService;

namespace PhoneAdminClient
{
    public partial class MainPage
    {
        private readonly ZPDServiceClient _client;
        // We just need to keep a reference to the timer since we never really want it to stop
// ReSharper disable NotAccessedField.Local
        private readonly Timer _timer;
// ReSharper restore NotAccessedField.Local
        // Constructor
        public MainPage()
        {
            _client = new ZPDServiceClient(new BasicHttpBinding(), new EndpointAddress("http://10.0.0.3:8000/zpd"));
            _client.PlayCompleted += PlayCompleted;
            _client.NextTrackCompleted += NextTrackCompleted;
            _client.PreviousTrackCompleted += PreviousTrackCompleted;
            _client.PauseCompleted += PauseCompleted;
            _client.GetCurrentPlayerStateCompleted += GetCurrentPlayerStateCompleted;

            InitializeComponent();
            StartUpdateCurrentTrackInfoFromServer();
            _timer = new Timer(timerCallback, this, 0, 1000);
        }

        private void timerCallback(object state)
        {
            Debug.Assert(state is MainPage);
            var page = state as MainPage;
            page.Dispatcher.BeginInvoke(page.StartUpdateCurrentTrackInfoFromServer);
        }

        private void GetCurrentPlayerStateCompleted(object sender, GetCurrentPlayerStateCompletedEventArgs e)
        {
            if (null == e.Error && null != e.Result)
            {
                CurrentTrackTitle.Text = e.Result.CurrentTrack.Name;
                CurrentTrackArtist.Text = e.Result.CurrentTrack.Artist;

                var elapsedTime = new TimeSpan(0, 0, 0, Convert.ToInt32(e.Result.CurrentTrackPosition));
                var totalTime = new TimeSpan(0, 0, 0, e.Result.CurrentTrack.Duration);
                CurrentTrackTime.Text = elapsedTime.ToString(@"m\:ss") + "/" + totalTime.ToString(@"m\:ss");
            }
            else
            {
                Debug.WriteLine("Get current player state either failed or got no data");
            }
        }

        private void PauseCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (null == e.Error)
            {
                Debug.WriteLine("Pause succeeded");
            }
            else
            {
                Debug.WriteLine("Pause failed: {0}", e.Error.Message);
            }
        }

        private void PreviousTrackCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (null == e.Error)
            {
                StartUpdateCurrentTrackInfoFromServer();
                Debug.WriteLine("Previous Track succeeded");
            }
            else
            {
                Debug.WriteLine("Previous Track failed: {0}", e.Error.Message);
            }
        }

        private void NextTrackCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (null == e.Error)
            {
                StartUpdateCurrentTrackInfoFromServer();
                Debug.WriteLine("Next Track succeeded");
            }
            else
            {
                Debug.WriteLine("Next Track failed: {0}", e.Error.Message);
            }
        }

        private void PlayCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (null == e.Error)
            {
                StartUpdateCurrentTrackInfoFromServer();
                Debug.WriteLine("Play succeeded");
            }
            else
            {
                Debug.WriteLine("Play failed: {0}", e.Error.Message);
            }
        }

        private void PlayButtonClick(object sender, EventArgs e)
        {
            _client.PlayAsync(GetAuthPacket());
        }

        private AuthPacket GetAuthPacket()
        {
            var packet = new AuthPacket {Timeout = AuthTolkenTimeout.SixtySeconds};

            var now = DateTime.UtcNow;
            var numAdjustedSeconds = 60 + (60 - (now.Second%60));
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

        private void NextTrackButtonClick(object sender, EventArgs e)
        {
            _client.NextTrackAsync(GetAuthPacket());
        }

        private void PauseButtonClick(object sender, EventArgs e)
        {
            _client.PauseAsync(GetAuthPacket());
        }

        private void PreviousTrackButtonClick(object sender, EventArgs e)
        {
            _client.PreviousTrackAsync(GetAuthPacket());
        }

        private void StartUpdateCurrentTrackInfoFromServer()
        {
            _client.GetCurrentPlayerStateAsync();
        }

        private void SettingsClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }
    }
}