using System.Runtime.Serialization;

namespace zpd
{
    [DataContract]
    public class ZpdCurrentPlayerState
    {
        [DataMember]
        public ZpdTrack CurrentTrack { get; private set; }
        [DataMember]
        public float CurrentTrackPosition { get; private set; }
        [DataMember]
        public float Volume { get; private set; }
        [DataMember]
        public bool IsPlaying { get; private set; }

        public ZpdCurrentPlayerState(ZpdTrack currentTrack, float currentTrackPosition, float volume, bool isPlaying)
        {
            CurrentTrack = currentTrack;
            CurrentTrackPosition = currentTrackPosition;
            Volume = volume;
            IsPlaying = isPlaying;
        }
    }
}