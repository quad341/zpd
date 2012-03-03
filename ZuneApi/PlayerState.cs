namespace VosSoft.ZuneLcd.Api
{
    /// <summary>
    /// Specifies the states of the Zune player.
    /// </summary>
    /// <see cref="MicrosoftZunePlayback.MCPlayerState">ZuneDBApi.dll</see>
    public enum PlayerState
    {
        /// <summary>
        /// The player is uninitialized.
        /// </summary>
        Uninitialized = 0,

        /// <summary>
        /// The player is closed.
        /// </summary>
        Closed = 1,

        /// <summary>
        /// The player is open.
        /// </summary>
        Open = 2,

        /// <summary>
        /// The player is built.
        /// </summary>
        Built = 3,

        /// <summary>
        /// The player is inactive.
        /// </summary>
        Inactive = 4,

        /// <summary>
        /// The player is dead.
        /// </summary>
        Dead = 5
    }
}
