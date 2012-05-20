using System;
using System.Collections.Generic;

namespace zpd
{
    class ZPDService : IZPDService
    {
        public void Play()
        {
            MediaPlayerManager.Instance.Play();
        }

        public void Pause()
        {
            MediaPlayerManager.Instance.Play();
        }

        public void ClosePlayer()
        {
            MediaPlayerManager.ClosePlayer();
        }

        public void PlaySongIndex(int index)
        {
            MediaPlayerManager.Instance.PlaySongIndex(index);
        }

        public void Stop()
        {
            MediaPlayerManager.Instance.Stop();
        }

        public void NextTrack()
        {
            MediaPlayerManager.Instance.NextTrack();
        }

        public void PreviousTrack()
        {
            MediaPlayerManager.Instance.PreviousTrack();
        }

        public void ToggleFastForward()
        {
            MediaPlayerManager.Instance.ToggleFastForward();
        }

        public void ToggleRewind()
        {
            MediaPlayerManager.Instance.ToggleRewind();
        }

        public void ToggleShuffle()
        {
            MediaPlayerManager.Instance.ToggleShuffle();
        }

        public void ToggleRepeat()
        {
            MediaPlayerManager.Instance.ToggleRepeat();
        }

        public void ReIndexLibrary()
        {
            MediaPlayerManager.Instance.ReIndexLibrary();
        }

        public IEnumerable<ZpdTrack> Search(string searchTerm)
        {
            return MediaPlayerManager.Instance.Search(searchTerm);
        }

        public void QueueTrack(int mediaId, int mediaTypeId)
        {
            MediaPlayerManager.Instance.QueueTrack(mediaId, mediaTypeId);
        }

        public void QueueTrackAtIndex(int mediaId, int mediaTypeId, int index)
        {
            MediaPlayerManager.Instance.QueueTrackAtIndex(mediaId, mediaTypeId, index);
        }

        public void RemoveTrackAtIndex(int index)
        {
            MediaPlayerManager.Instance.RemoveTrackAtIndex(index);
        }

        public ZpdCurrentPlayerState GetCurrentPlayerState()
        {
            return MediaPlayerManager.Instance.GetCurrentPlayerState();
        }

        public IEnumerable<ZpdTrack> GetCurrentQueue()
        {
            return MediaPlayerManager.Instance.GetCurrentQueue();
        }
    }
}