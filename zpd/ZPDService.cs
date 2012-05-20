using System;
using System.Collections.Generic;
using System.Threading;
using VosSoft.ZuneLcd.Api;

namespace zpd
{
    class ZPDService : IZPDService
    {
        //TODO: this shouldn't be directly calling the ZuneApi; should make intermediate for MediaPlayerManager to take care of threading, etc.
        ZuneApi _zune;
        Thread _zuneThread;

        private void EnsureZune()
        {
            if (_zune == null)
            {
                Init();
                Thread.Sleep(10000);
            }
        }

        private void Init()
        {
            _zune = new ZuneApi();
            _zuneThread = new Thread(ZuneThread);
            _zuneThread.Start();
        }

        public void Play()
        {
            EnsureZune();
            _zune.Play();
        }

        public void Pause()
        {
            EnsureZune();
            _zune.Pause();
        }

        public void Close()
        {
            EnsureZune();
            _zune.Close();
            _zuneThread.Join();
        }

        private void ZuneThread()
        {
            _zune.Launch();
        }


        public void PlaySongIndex(int index)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void NextTrack()
        {
            throw new NotImplementedException();
        }

        public void PreviousTrack()
        {
            throw new NotImplementedException();
        }

        public void ToggleFastForward()
        {
            throw new NotImplementedException();
        }

        public void ToggleRewind()
        {
            throw new NotImplementedException();
        }

        public void ToggleShuffle()
        {
            throw new NotImplementedException();
        }

        public void ToggleRepeat()
        {
            throw new NotImplementedException();
        }

        public void ReIndexLibrary()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ZpdTrack> Search(string searchTerm)
        {
            throw new NotImplementedException();
        }

        public void QueueTrack(int mediaId, int mediaTypeId)
        {
            throw new NotImplementedException();
        }

        public void QueueTrackAtIndex(int mediaId, int mediaTypeId, int index)
        {
            throw new NotImplementedException();
        }

        public void RemoveTrackAtIndex(int index)
        {
            throw new NotImplementedException();
        }

        public ZpdCurrentPlayerState GetCurrentPlayerState()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ZpdTrack> GetCurrentQueue()
        {
            throw new NotImplementedException();
        }

        public void ClosePlayer()
        {
            throw new NotImplementedException();
        }
    }
}