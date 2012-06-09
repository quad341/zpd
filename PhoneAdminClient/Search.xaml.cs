using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using PhoneAdminClient.ZpdService;

namespace PhoneAdminClient
{
    public partial class Search
    {
        private readonly ObservableCollection<string> _data;
        public Search()
        {
            InitializeComponent();

            _data = new ObservableCollection<string>();
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
                    _data.Add(track.Name + " by " + track.Artist);
                }
                refreshPanel.IsRefreshing = false;
            }
            else
            {
                Debug.WriteLine("Search failed");
                NavigationService.GoBack();
            }
        }
    }
}