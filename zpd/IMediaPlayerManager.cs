using System.Collections.Generic;

namespace zpd
{
    /// <summary>
    /// Beginning of an interface that any of the media player managers will need
    /// </summary>
    public interface IMediaPlayerManager
    {
        void Play();
        void PlaySongIndex(int index);
        void Pause();
        void Stop();
        void NextTrack();
        void PreviousTrack();
        void ToggleFastForward();
        void ToggleRewind();
        void ToggleShuffle();
        void ToggleRepeat();
        void ReIndexLibrary();
        IEnumerable<ZpdTrack> Search(string query);
        void QueueTrack(int mediaId, int mediaTypeId);
        void QueueTrackAtIndex(int mediaId, int mediaTypeId, int index);
        void RemoveTrackAtIndex(int index);
        ZpdCurrentPlayerState GetCurrentPlayerState();
        IEnumerable<ZpdTrack> GetCurrentQueue();
    }
}