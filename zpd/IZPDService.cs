using System.Collections.Generic;
using System.ServiceModel;

namespace zpd
{
    
    //TODO: many methods should support authentication in some way
    [ServiceContract(Namespace = "http://zpd")]
    public interface IZPDService
    {
        [OperationContract]
        void Play();

        [OperationContract]
        void PlaySongIndex(int index);

        [OperationContract]
        void Pause();

        [OperationContract]
        void Stop();

        [OperationContract]
        void NextTrack();

        [OperationContract]
        void PreviousTrack();

        [OperationContract]
        void ToggleFastForward();

        [OperationContract]
        void ToggleRewind();

        [OperationContract]
        void ToggleShuffle();

        [OperationContract]
        void ToggleRepeat();

        [OperationContract]
        void ReIndexLibrary();

        [OperationContract]
        IEnumerable<ZpdTrack> Search(string searchTerm);

        [OperationContract]
        void QueueTrack(int mediaId, int mediaTypeId);

        [OperationContract]
        void QueueTrackAtIndex(int mediaId, int mediaTypeId, int index);

        [OperationContract]
        void RemoveTrackAtIndex(int index);

        [OperationContract]
        ZpdCurrentPlayerState GetCurrentPlayerState();

        [OperationContract]
        IEnumerable<ZpdTrack> GetCurrentQueue();

        [OperationContract]
        void ClosePlayer();
    }
}