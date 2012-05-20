namespace zpd
{
    public class ZpdCurrentPlayerState
    {
        public ZpdTrack CurrentTrack { get; private set; }
        public float CurrentTrackPosition { get; private set; }
        public float Volume { get; private set; }
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