namespace VosSoft.ZuneLcd.Api
{
    /// <summary>
    /// Specifies the user rating types of a track.
    /// </summary>
    /// <remarks>This is originally just an int value in the Zune software.</remarks>
    public enum TrackRating
    {
        /// <summary>
        /// Track can't be rated.
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// Track is unrated.
        /// </summary>
        Unrated = 0,

        /// <summary>
        /// Track rating is set to "don't like".
        /// </summary>
        DontLike = 2,

        /// <summary>
        /// Track rating is set to "like".
        /// </summary>
        Like = 8
    }
}
