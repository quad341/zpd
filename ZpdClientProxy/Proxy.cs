using System.Collections.Generic;
using System.ServiceModel;
using ZpdContract;
using zpd;

namespace ZpdClientProxy
{
    public class Proxy : ClientBase<IZPDService>, IZPDService
    {
        public int GetNewClientId()
        {
            return Channel.GetNewClientId();
        }

        public void Play(AuthPacket authPacket)
        {
            Channel.Play(authPacket);
        }

        public void PlaySongIndex(AuthPacket authPacket, int index)
        {
            Channel.PlaySongIndex(authPacket, index);
        }

        public void Pause(AuthPacket authPacket)
        {
            Channel.Pause(authPacket);
        }

        public void Stop(AuthPacket authPacket)
        {
            Channel.Stop(authPacket);
        }

        public void NextTrack(AuthPacket authPacket)
        {
            Channel.NextTrack(authPacket);
        }

        public void PreviousTrack(AuthPacket authPacket)
        {
            Channel.PreviousTrack(authPacket);
        }

        public void ToggleFastForward(AuthPacket authPacket)
        {
            Channel.ToggleFastForward(authPacket);
        }

        public void ToggleRewind(AuthPacket authPacket)
        {
            Channel.ToggleRewind(authPacket);
        }

        public void ToggleShuffle(AuthPacket authPacket)
        {
            Channel.ToggleShuffle(authPacket);
        }

        public void ToggleRepeat(AuthPacket authPacket)
        {
            Channel.ToggleRepeat(authPacket);
        }

        public void ReIndexLibrary(AuthPacket authPacket)
        {
            Channel.ReIndexLibrary(authPacket);
        }

        public IEnumerable<ZpdTrack> Search(string searchTerm)
        {
            return Channel.Search(searchTerm);
        }

        public void QueueTrack(int mediaId, int mediaTypeId)
        {
            Channel.QueueTrack(mediaId, mediaTypeId);
        }

        public void QueueTrackAtIndex(AuthPacket authPacket, int mediaId, int mediaTypeId, int index)
        {
            Channel.QueueTrackAtIndex(authPacket, mediaId, mediaTypeId, index);
        }

        public void RemoveTrackAtIndex(AuthPacket authPacket, int index)
        {
            Channel.RemoveTrackAtIndex(authPacket, index);
        }

        public ZpdCurrentPlayerState GetCurrentPlayerState()
        {
            return Channel.GetCurrentPlayerState();
        }

        public IEnumerable<ZpdTrack> GetCurrentQueue()
        {
            return Channel.GetCurrentQueue();
        }

        public void ClosePlayer(AuthPacket authPacket)
        {
            Channel.ClosePlayer(authPacket);
        }
    }
}
