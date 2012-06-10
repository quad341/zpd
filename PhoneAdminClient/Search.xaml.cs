using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using PhoneAdminClient.ZpdService;

namespace PhoneAdminClient
{
    public partial class Search
    {
        private readonly ObservableCollection<ZpdTrack> _data;
        public Search()
        {
            InitializeComponent();

            _data = new ObservableCollection<ZpdTrack>();
            searchResultsListBox.DataContext = _data;
        }

        private void SearchKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Enter == e.Key)
            {
                PerformSearch();
            }
        }

        private void RefreshPanelRefreshRequested(object sender, EventArgs e)
        {
            // No op; just want to make sure I can use the refresh handler for searching
        }

        private void SearchImageTap(object sender, GestureEventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            Debug.Assert(null != ClientManager.Client);
            var client = ClientManager.Client;
            client.SearchCompleted += SearchCompleted;

            if (ClientManager.CanOpenMoreRequests)
            {
                ClientManager.RequestCount++;
                refreshPanel.IsRefreshing = true;
                client.SearchAsync(queryBox.Text);
            }
            else
            {
                NavigationService.GoBack();
            }

        }

        private void SearchCompleted(object sender, SearchCompletedEventArgs e)
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
                Debug.WriteLine("Search failed");
                NavigationService.GoBack();
            }
        }

        private void StackPanelTap(object sender, GestureEventArgs e)
        {
            Debug.Assert(sender is StackPanel);
            var sp = sender as StackPanel;
            Debug.Assert(sp.DataContext is ZpdTrack);
            var track = sp.DataContext as ZpdTrack;
            
            if (ClientManager.CanOpenMoreRequests)
            {
                ClientManager.RequestCount++;
                ClientManager.Client.QueueTrackAsync(track.MediaId, track.MediaTypeId);
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            else
            {
                Debug.WriteLine("Can't send a queue request");
                NavigationService.GoBack();
            }
        }
    }
}