using System.Collections.Generic;
using System.ServiceModel;

namespace zpd
{
    
    //TODO: many methods should support authentication in some way
    [ServiceContract(Namespace = "http://zpd")]
    public interface IZPDService
    {
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
        ZpdCurrentPlayerState GetCurrentPlayerState();

        [OperationContract]
        IEnumerable<ZpdTrack> GetCurrentQueue();

        [OperationContract]
        void ClosePlayer(AuthPacket authPacket);
    }
}