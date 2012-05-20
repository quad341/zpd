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
    public class MediaPlayerManager
    {
        private readonly ZuneApi _zune;
        private readonly Thread _zuneThread;
        private static MediaPlayerManager _instance;

        static MediaPlayerManager Instance
        {
            get
            {
                // Because the service should not be available until after EnsureInstance() returns, we are
                // not locking here. Also, this is likely to be called a lot and we don't want to pay the cost here.
                // We do have to pay the locking cost for individual calls though.
                Debug.Assert(null != _instance, "EnsureInstance should have already been called");
                return _instance;
            }
        }

        public static void EnsureInstance()
        {
            lock(typeof(MediaPlayerManager))
            {
                if (null == _instance)
                {
                    _instance = new MediaPlayerManager();
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
                var searchTracks = _zune.Search(query);
                // need to convert the search tracks to ZpdTracks
                var zpdTracks = new List<ZpdTrack>();
                foreach (var track in searchTracks)
                {
                    zpdTracks.Add(new ZpdTrack(track));
                }

                return zpdTracks;
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
                        new ZpdTrack(0, // Invalid mediaId since this should not be used
                                     0, // Invalid mediaTypeId since this should not be used
                                     _zune.CurrentTrack.Title,
                                     _zune.CurrentTrack.Artist,
                                     _zune.CurrentTrack.Album,
                                     Convert.ToSingle(_zune.CurrentTrack.Duration.TotalSeconds)),
                        Convert.ToSingle(_zune.CurrentTrack.Position.TotalSeconds),
                        _zune.Volume,
                        _zune.TrackState == TrackState.Playing);
            }
        }

        public IEnumerable<ZpdTrack> GetCurrentQueue()
        {
            throw new NotImplementedException();
        }

        public static void ClosePlayer()
        {
            lock(typeof(MediaPlayerManager))
            {
                if (null != _instance)
                {
                    _instance._zune.Close();
                    _instance._zuneThread.Join();
                    _instance = null;
                }
            }
        }

        private MediaPlayerManager()
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
    }
}