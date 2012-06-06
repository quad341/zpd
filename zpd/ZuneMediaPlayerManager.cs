using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using VosSoft.ZuneLcd.Api;

namespace zpd
{
    /// <summary>
    /// Singleton management class for actually preforming media player operations
    /// </summary>
    public class ZuneMediaPlayerManager : IMediaPlayerManager
    {
        private readonly ZuneApi _zune;
        private readonly Thread _zuneThread;
        private static ZuneMediaPlayerManager s_instance;

        public static ZuneMediaPlayerManager Instance
        {
            get
            {
                lock (typeof(ZuneMediaPlayerManager))
                {
                    Debug.Assert(null != s_instance, "EnsureInstance should have already been called and this should not be closed");
                    return s_instance;
                }
            }
        }

        public static void EnsureInstance()
        {
            lock(typeof(ZuneMediaPlayerManager))
            {
                if (null == s_instance)
                {
                    s_instance = new ZuneMediaPlayerManager();
                }
            }
        }

        public void Play()
        {
            lock(this)
            {
                _zune.Play();
            }
        }

        public void PlaySongIndex(int index)
        {
            lock(this)
            {
                _zune.PlayAt(index);
            }
        }

        public void Pause()
        {
            lock(this)
            {
                _zune.Pause();
            }
        }

        public void Stop()
        {
            lock(this)
            {
                _zune.Stop();
            }
        }

        public void NextTrack()
        {
            lock(this)
            {
                _zune.Forward();
            }
        }

        public void PreviousTrack()
        {
            lock(this)
            {
                _zune.Back();
            }
        }

        public void ToggleFastForward()
        {
            lock(this)
            {
                _zune.ToggleFastForward();
            }
        }

        public void ToggleRewind()
        {
            lock(this)
            {
                _zune.ToggleRewind();
            }
        }

        public void ToggleShuffle()
        {
            lock(this)
            {
                _zune.ToggleShuffle();
            }
        }

        public void ToggleRepeat()
        {
            lock(this)
            {
                _zune.ToggleRepeat();
            }
        }

        public void ReIndexLibrary()
        {
            lock(this)
            {
                _zune.ReIndexMusic();
            }
        }

        public IEnumerable<ZpdTrack> Search(string query)
        {
            lock(this)
            {
                return  ConvertSearchTracksToZpdTracks(_zune.Search(query));
            }
        }

        public void QueueTrack(int mediaId, int mediaTypeId)
        {
            lock(this)
            {
                _zune.AddTrackToCurrentPlaylist(mediaId, ZuneApi.MediaTypeFromInt(mediaTypeId));
            }
        }

        public void QueueTrackAtIndex(int mediaId, int mediaTypeId, int index)
        {
            lock(this)
            {
                _zune.AddTrackToCurrentPlaylistAtIndex(mediaId, ZuneApi.MediaTypeFromInt(mediaTypeId), index);
            }
        }

        public void RemoveTrackAtIndex(int index)
        {
            lock(this)
            {
                _zune.RemoveTrackAtIndexFromCurrentPlaylist(index);
            }
        }

        public ZpdCurrentPlayerState GetCurrentPlayerState()
        {
            lock(this)
            {
                return
                    new ZpdCurrentPlayerState(
                        new ZpdTrack(_zune.CurrentTrack.Uri.GetHashCode(), // Invalid mediaId since this should not be used; it can be used as a differentiator though
                                     0, // Invalid mediaTypeId since this should not be used
                                     _zune.CurrentTrack.Title,
                                     _zune.CurrentTrack.Artist,
                                     _zune.CurrentTrack.Album,
                                     Convert.ToInt32(_zune.CurrentTrack.Duration.TotalSeconds)),
                        _zune.GetCurrentTrackPositionSynchronous(),
                        _zune.Volume,
                        _zune.TrackState == TrackState.Playing);
            }
        }

        public IEnumerable<ZpdTrack> GetCurrentQueue()
        {
            lock(this)
            {
                return
                    ConvertSearchTracksToZpdTracks(_zune.GetTracksAsSearchTrackSynchronous(
                        _zune.CurrentTrack.Index + 1 /*startIndex*/, 0
                                                       /*count, 0=all*/));
            }
        }

        public static void ClosePlayer()
        {
            lock(typeof(ZuneMediaPlayerManager))
            {
                if (null != s_instance)
                {
                    s_instance._zune.Close();
                    s_instance._zuneThread.Join();
                    s_instance = null;
                }
            }
        }

        private ZuneMediaPlayerManager()
        {
            _zune = new ZuneApi();
            _zuneThread = new Thread(ZuneThread);
            _zuneThread.Start();

            // Wait at least 10s to ensure zune actually launches
            Thread.Sleep(10000);

            _zune.ReIndexMusic();
        }

        private void ZuneThread()
        {
            _zune.Launch();
        }

        private static IEnumerable<ZpdTrack> ConvertSearchTracksToZpdTracks(IEnumerable<SearchTrack> tracks)
        {
            var searchTracks = new List<ZpdTrack>();
            foreach (var track in tracks)
            {
                searchTracks.Add(new ZpdTrack(track));
            }

            return searchTracks;
        }
    }
}