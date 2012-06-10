using System.Collections.Generic;
using System.ServiceModel;
using zpd;

namespace ZpdContract
{
    [ServiceContract(Namespace = "http://zpd")]
    public interface IZPDService
    {
        [OperationContract]
        int GetNewClientId();

        [OperationContract]
        void Play(AuthPacket authPacket);

        [OperationContract]
        void PlaySongIndex(AuthPacket authPacket, int index);

        [OperationContract]
        void Pause(AuthPacket authPacket);

        [OperationContract]
        void Stop(AuthPacket authPacket);

        [OperationContract]
        void NextTrack(AuthPacket authPacket);

        [OperationContract]
        void PreviousTrack(AuthPacket authPacket);

        [OperationContract]
        void ToggleFastForward(AuthPacket authPacket);

        [OperationContract]
        void ToggleRewind(AuthPacket authPacket);

        [OperationContract]
        void ToggleShuffle(AuthPacket authPacket);

        [OperationContract]
        void ToggleRepeat(AuthPacket authPacket);

        [OperationContract]
        void ReIndexLibrary(AuthPacket authPacket);

        [OperationContract]
        IEnumerable<ZpdTrack> Search(string searchTerm);

        [OperationContract]
        void QueueTrack(int mediaId, int mediaTypeId);

        [OperationContract]
        void QueueTrackAtIndex(AuthPacket authPacket, int mediaId, int mediaTypeId, int index);

        [OperationContract]
        void RemoveTrackAtIndex(AuthPacket authPacket, int index);

        [OperationContract]
        void MoveSongAtIndexToNewIndex(AuthPacket authPacket, int startIndex, int mediaId, int mediaTypeId, int destinationIndex);

        [OperationContract]
        ZpdCurrentPlayerState GetCurrentPlayerState();

        [OperationContract]
        IEnumerable<ZpdTrack> GetCurrentQueue();

        [OperationContract]
        void ClosePlayer(AuthPacket authPacket);
    }
}