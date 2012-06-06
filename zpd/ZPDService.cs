using System.Collections.Generic;

namespace zpd
{
    class ZPDService : IZPDService
    {
        public int GetNewClientId()
        {
            return TolkenAuthenticator.GetNewClientId();
        }

        public void Play(AuthPacket authPacket)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.Play();
            }
        }

        public void Pause(AuthPacket authPacket)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.Play();
            }
        }

        public void ClosePlayer(AuthPacket authPacket)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.ClosePlayer();
            }
        }

        public void PlaySongIndex(AuthPacket authPacket, int index)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.PlaySongIndex(index);
            }
        }

        public void Stop(AuthPacket authPacket)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.Stop();
            }
        }

        public void NextTrack(AuthPacket authPacket)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.NextTrack();
            }
        }

        public void PreviousTrack(AuthPacket authPacket)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.PreviousTrack();
            }
        }

        public void ToggleFastForward(AuthPacket authPacket)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.ToggleFastForward();
            }
        }

        public void ToggleRewind(AuthPacket authPacket)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.ToggleRewind();
            }
        }

        public void ToggleShuffle(AuthPacket authPacket)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.ToggleShuffle();
            }
        }

        public void ToggleRepeat(AuthPacket authPacket)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.ToggleRepeat();
            }
        }

        public void ReIndexLibrary(AuthPacket authPacket)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.ReIndexLibrary();
            }
        }

        public IEnumerable<ZpdTrack> Search(string searchTerm)
        {
            return ZuneMediaPlayerManager.Instance.Search(searchTerm);
        }

        public void QueueTrack(int mediaId, int mediaTypeId)
        {
            ZuneMediaPlayerManager.Instance.QueueTrack(mediaId, mediaTypeId);
        }

        public void QueueTrackAtIndex(AuthPacket authPacket, int mediaId, int mediaTypeId, int index)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.QueueTrackAtIndex(mediaId, mediaTypeId, index);
            }
        }

        public void RemoveTrackAtIndex(AuthPacket authPacket, int index)
        {
            if (TolkenAuthenticator.IsValid(authPacket))
            {
                ZuneMediaPlayerManager.Instance.RemoveTrackAtIndex(index);
            }
        }

        public ZpdCurrentPlayerState GetCurrentPlayerState()
        {
            return ZuneMediaPlayerManager.Instance.GetCurrentPlayerState();
        }

        public IEnumerable<ZpdTrack> GetCurrentQueue()
        {
            return ZuneMediaPlayerManager.Instance.GetCurrentQueue();
        }
    }
}