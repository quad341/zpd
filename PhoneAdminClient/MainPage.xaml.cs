﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using PhoneAdminClient.ReorderListBox;
using PhoneAdminClient.ZpdService;

namespace PhoneAdminClient
{
    public partial class MainPage
    {
        private ZPDServiceClient _client;
        private int _clientGeneration;
        private int _clientId;
        private int _packetCount;
        private int _currentTrackIdentifier;
        private readonly SettingsManager _settings;

        private readonly ObservableCollection<ZpdTrack> _data;
        // We just need to keep a reference to the timer since we never really want it to stop
        private readonly Timer _timer;
        // Constructor
        public MainPage()
        {
            _currentTrackIdentifier = -1;
            _clientId = -1;
            _packetCount = 0;
            _settings = SettingsManager.Instance;

            _data = new ObservableCollection<ZpdTrack>();

            InitializeComponent();
            if (null == ClientManager.Client)
            {
                ClientManager.InitAndGetClient();
            }

            InitClient();

            _timer = new Timer(TimerCallback, this, 0, 1000);

            trackQueueListBox.DataContext = _data;
            trackQueueListBox.ItemsReordered += ItemsReorderedHandler;
        }

        private void ItemsReorderedHandler(object sender, ReorderedItemEventArgs re)
        {
            Debug.Assert(re.ReorderedItem is ZpdTrack);
            var track = re.ReorderedItem as ZpdTrack;

            StartClientRequest(() => _client.MoveSongAtIndexToNewIndexAsync(GetAuthPacket(), track.QueueIndex, track.MediaId, track.MediaTypeId, re.DestinationIndex));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            InitClient();
        }

        private void InitClient()
        {
            if (ClientManager.ClientGeneration > _clientGeneration)
            {
                // we don't want the timer to call back while we change this
                if (null != _timer)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
                _client = ClientManager.Client;
                _clientGeneration = ClientManager.ClientGeneration;

                if (null != _timer)
                {
                    _timer.Change(0, 1000);
                }
            }

            _client.PlayCompleted += PlayCompleted;
            _client.NextTrackCompleted += NextTrackCompleted;
            _client.PreviousTrackCompleted += PreviousTrackCompleted;
            _client.PauseCompleted += PauseCompleted;
            _client.GetCurrentPlayerStateCompleted += GetCurrentPlayerStateCompleted;
            _client.GetNewClientIdCompleted += GetNewClientIdCompleted;
            _client.GetCurrentQueueCompleted += GetCurrentQueueCompleted;
            _client.QueueTrackCompleted += QueueTrackCompleted;
            _client.RemoveTrackAtIndexCompleted += RemoveTrackCompleted;
            _client.MoveSongAtIndexToNewIndexCompleted += MoveSongCompleted;
            _client.GetNewClientIdAsync();
            StartUpdateCurrentTrackInfoFromServer();
            StartRefreshTrackList();
        }

        private void MoveSongCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ClientManager.RequestCount--;
            if (null == e.Error)
            {
                StartRefreshTrackList();
            }
            else
            {
                Debug.WriteLine("Move song failed");
            }
        }

        private void RemoveTrackCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ClientManager.RequestCount--;
            if (null == e.Error)
            {
                StartRefreshTrackList();
            }
            else
            {
                Debug.WriteLine("Remove Track Failed");
            }
        }

        private void QueueTrackCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ClientManager.RequestCount--;
            if (null == e.Error)
            {
                StartRefreshTrackList();
            }
            else
            {
                Debug.WriteLine("Queue track failed");
            }
        }

        private void GetCurrentQueueCompleted(object sender, GetCurrentQueueCompletedEventArgs e)
        {
            ClientManager.RequestCount--;
            if (null == e.Error)
            {
                _data.Clear();
                foreach (var track in e.Result)
                {
                    _data.Add(track);
                }

                refreshPanel.IsRefreshing = false;
            }
            else
            {
                Debug.WriteLine("Error getting current queue");
            }
        }

        private void GetNewClientIdCompleted(object sender, GetNewClientIdCompletedEventArgs e)
        {
            ClientManager.RequestCount--;
            if (null == e.Error)
            {
                _clientId = e.Result;
            }
            else
            {
                Debug.WriteLine("Failed to get client id");
            }
        }

        private static void TimerCallback(object state)
        {
            Debug.Assert(state is MainPage);
            var page = state as MainPage;
            page.Dispatcher.BeginInvoke(page.StartUpdateCurrentTrackInfoFromServer);
        }

        private void GetCurrentPlayerStateCompleted(object sender, GetCurrentPlayerStateCompletedEventArgs e)
        {
            ClientManager.RequestCount--;
            if (null == e.Error && null != e.Result)
            {
                CurrentTrackTitle.Text = e.Result.CurrentTrack.Name;
                CurrentTrackArtist.Text = e.Result.CurrentTrack.Artist;

                var elapsedTime = new TimeSpan(0, 0, 0, Convert.ToInt32(e.Result.CurrentTrackPosition));
                var totalTime = new TimeSpan(0, 0, 0, e.Result.CurrentTrack.Duration);
                CurrentTrackTime.Text = elapsedTime.ToString(@"m\:ss") + "/" + totalTime.ToString(@"m\:ss");

                if (_currentTrackIdentifier != e.Result.CurrentTrack.MediaId)
                {
                    StartRefreshTrackList();
                    _currentTrackIdentifier = e.Result.CurrentTrack.MediaId;
                }

            }
            else
            {
                Debug.WriteLine("Get current player state either failed or got no data");
            }
        }

        private void PauseCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ClientManager.RequestCount--;
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
            ClientManager.RequestCount--;
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
            ClientManager.RequestCount--;
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
            ClientManager.RequestCount--;
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
            StartClientRequest(() => _client.PlayAsync(GetAuthPacket()));
        }

        private AuthPacket GetAuthPacket()
        {
            Debug.Assert(-1 != _clientId);
            var packet = new AuthPacket {Timeout = AuthTolkenTimeout.SixtySeconds, ClientId = _clientId};

            DateTime now = DateTime.UtcNow;
            int numAdjustedSeconds = 60 + (60 - (now.Second%60));
            DateTime adjustedTime = now.AddSeconds(numAdjustedSeconds);
            // need granularity down to seconds
            var adjustedTimeToSecond = new DateTime(adjustedTime.Year,
                                                    adjustedTime.Month,
                                                    adjustedTime.Day,
                                                    adjustedTime.Hour,
                                                    adjustedTime.Minute,
                                                    adjustedTime.Second);

            string computedAuthTolken =
                Sha1HashOfString(adjustedTimeToSecond.ToString("yyyy-MM-dd:HH:mm:ss") + _settings.Password +
                                 _packetCount++);

            packet.Offset = numAdjustedSeconds;
            packet.AuthTolken = computedAuthTolken;
            return packet;
        }

        // Taken from http://dotnetpulse.blogspot.com/2007/12/sha1-hash-calculation-in-c.html
        private static string Sha1HashOfString(string input)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(input);
            var crypto = new SHA256Managed();

            return BitConverter.ToString(crypto.ComputeHash(buffer)).Replace("-", "");
        }

        private void NextTrackButtonClick(object sender, EventArgs e)
        {
            StartClientRequest(() => _client.NextTrackAsync(GetAuthPacket()));
        }

        private void PauseButtonClick(object sender, EventArgs e)
        {
            StartClientRequest(() => _client.PauseAsync(GetAuthPacket()));
        }

        private void PreviousTrackButtonClick(object sender, EventArgs e)
        {
            StartClientRequest(() => _client.PreviousTrackAsync(GetAuthPacket()));
        }

        private void StartUpdateCurrentTrackInfoFromServer()
        {
            if (CommunicationState.Opened == _client.State)
            {
                StartClientRequest(() => _client.GetCurrentPlayerStateAsync());
            }
        }

        private void SettingsClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void RefreshPanelRefreshRequested(object sender, EventArgs e)
        {
            StartRefreshTrackList();
        }

        private void StartRefreshTrackList()
        {
            refreshPanel.IsRefreshing = true;
            StartClientRequest(() => _client.GetCurrentQueueAsync());
        }

        private void StartClientRequest(Action a)
        {
            if (ClientManager.CanOpenMoreRequests)
            {
                ClientManager.RequestCount++;
                a.Invoke();
            }
            else
            {
                ResetUiState();
            }
        }

        private void ResetUiState()
        {
            CurrentTrackArtist.Text = "";
            CurrentTrackTitle.Text = "No connection..";
            CurrentTrackTime.Text = "0:00/0:00";
            _data.Clear();
        }

        private void SearchClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Search.xaml", UriKind.Relative));
        }

        private void RemoveTrackTap(object sender, GestureEventArgs e)
        {
            Debug.Assert(sender is Image);
            var img = sender as Image;
            Debug.Assert(img.DataContext is ZpdTrack);
            var track = img.DataContext as ZpdTrack;

            StartClientRequest(() => _client.RemoveTrackAtIndexAsync(GetAuthPacket(), track.QueueIndex));
        }
    }
}