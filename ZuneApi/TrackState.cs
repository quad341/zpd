namespace VosSoft.ZuneLcd.Api
{
    /// <summary>
    /// Specifies the states of the current track within the Zune player.
    /// </summary>
    /// <see cref="MicrosoftZunePlayback.MCTransportState">ZuneDBApi.dll</see>
    public enum TrackState
    {
        /// <summary>
        /// The track is invalid.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// The track is stopped.
        /// </summary>
        Stopped = 1,

        /// <summary>
        /// The track is playing.
        /// </summary>
        Playing = 2,

        /// <summary>
        /// The track is paused.
        /// </summary>
        Paused = 3,

        /// <summary>
        /// The track is buffering.
        /// </summary>
        Buffering = 4
    }
}
