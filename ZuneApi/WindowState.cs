namespace VosSoft.ZuneLcd.Api
{
    /// <summary>
    /// Specifies the states of the Zune Software window.
    /// </summary>
    /// <see cref="Microsoft.Iris.WindowState">UIX.dll</see>
    public enum WindowState
    {
        /// <summary>
        /// The window is in normal state.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The window is minimized.
        /// </summary>
        Minimized = 1,

        /// <summary>
        /// The window is maximized.
        /// </summary>
        Maximized = 2,
    }
}
