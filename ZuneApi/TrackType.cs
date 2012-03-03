namespace VosSoft.ZuneLcd.Api
{
    /// <summary>
    /// Specifies the type of the current track in the Zune player.
    /// </summary>
    /// <see cref="ZuneUI.MediaType">ZuneShell.dll</see>
    public enum TrackType
    {
        /// <summary>
        /// The type of the track is undefined.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// There is no current track.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the track is music.
        /// </summary>
        Music = 1,

        /// <summary>
        /// Indicates that the track is a video.
        /// </summary>
        Video = 2,

        /// <summary>
        /// Indicates that the track is a podcast.
        /// </summary>
        Podcast = 3
    }
}
