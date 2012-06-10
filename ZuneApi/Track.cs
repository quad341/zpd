using System;
using ZuneUI;
using System.ComponentModel;
using MicrosoftZuneLibrary;
using Microsoft.Iris;

namespace VosSoft.ZuneLcd.Api
{
    /// <summary>
    /// Defines a track with all important informations from the Zune player.
    /// </summary>
    /// <see cref="ZuneUI.PlaybackTrack">ZuneShell.dll</see>
    public sealed class Track
    {
        #region Fields

        private TrackRating rating = TrackRating.Invalid;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a reference to the zune track.
        /// </summary>
        internal PlaybackTrack ZuneTrack { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Track"/> is valid.
        /// </summary>
        /// <value><c>true</c> if valid; otherwise, <c>false</c>.</value>
        public bool Valid
        {
            get { return ZuneTrack != null; }
        }

        /// <summary>
        /// Gets the absolute URI of this track.
        /// </summary>
        /// <value>The URI.</value>
        public string Uri { get; private set; }

        /// <summary>
        /// Gets the zero based index position of this track in the current playlist.
        /// <para>
        /// <remarks>If the index equals <c>-1</c>, then the track has no playlist.</remarks>
        /// </para>
        /// </summary>
        /// <value>The index position.</value>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the title of this track.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the overall duration of this track.
        /// </summary>
        /// <value>The overall duration.</value>
        public TimeSpan Duration { get; private set; }

        /// <summary>
        /// Gets the current playing position of this track.
        /// </summary>
        /// <value>The current position.</value>
        public TimeSpan Position { get; internal set; }

        /// <summary>
        /// Gets the internal media id for the track as an int
        /// </summary>
        /// <value>The media id</value>
        public int MediaId { get; private set; }

        /// <summary>
        /// Gets the internal media type id for the track as an int
        /// </summary>
        /// <value>The Type of the track</value>
        public int ConvertedMediaTypeId { get; private set; }

        #endregion

        #region Optional Properties

        /// <summary>
        /// Gets the artist of this track.
        /// <para>
        /// <remarks>Because this property is optional, the value may be <c>string.Empty</c>.</remarks>
        /// </para>
        /// </summary>
        /// <value>The artist.</value>
        public string Artist { get; private set; }

        internal int ArtistId { get; private set; }

        /// <summary>
        /// Gets the album of this track.
        /// <para>
        /// <remarks>Because this property is optional, the value may be <c>string.Empty</c>.</remarks>
        /// </para>
        /// </summary>
        /// <value>The album.</value>
        public string Album { get; private set; }

        internal int AlbumId { get; private set; }

        /// <summary>
        /// Gets the release year of this track.
        /// <para>
        /// <remarks>Because this property is optional, the value may be <c>0</c> or <c>-1</c>
        /// if the release year cannot be found within the Zune library itself.</remarks>
        /// </para>
        /// </summary>
        /// <value>The release year.</value>
        public int ReleaseYear { get; private set; }

        /// <summary>
        /// Gets the absolute cover URL of this track.
        /// <para>
        /// <remarks>Because this property is optional, the value may be <c>string.Empty</c>
        /// if this track has no cover.</remarks>
        /// </para>
        /// </summary>
        /// <value>The cover URL.</value>
        public string CoverUrl { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this track can be rated by the user.
        /// </summary>
        /// <value><c>true</c> if this track can be rated; otherwise, <c>false</c>.</value>
        public bool CanRate
        {
            get { return ZuneTrack != null && ZuneTrack.CanRate; }
        }

        /// <summary>
        /// Gets or sets the user rating of this track.
        /// </summary>
        /// <seealso cref="RatingChanged"/>
        /// <seealso cref="ToggleRating"/>
        public TrackRating Rating
        {
            get { return rating; }
            set
            {
                if (rating != value && CanRate)
                {
                    rating = value;
                    Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
                    {
                        ZuneTrack.UserRating = (int)rating;
                    }), DeferredInvokePriority.Normal);
                    if (RatingChanged != null)
                        RatingChanged(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        #region Formated Properties

        /// <summary>
        /// Gets the overall duration of this track as an formated string ([h:]m:ss).
        /// </summary>
        /// <value>The formated duration.</value>
        /// <see cref="ZuneUI.Shell.TimeSpanToString(TimeSpan)">ZuneShell.dll</see>
        public string DurationFormat
        {
            get { return Shell.TimeSpanToString(Duration); }
        }

        /// <summary>
        /// Gets the current playing position of this track as an formated string ([h:]m:ss).
        /// </summary>
        /// <value>The formated position.</value>
        /// <see cref="ZuneUI.Shell.TimeSpanToString(TimeSpan)">ZuneShell.dll</see>
        public string PositionFormat
        {
            get { return Shell.TimeSpanToString(Position); }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs after the user rating of this track has changed.
        /// </summary>
        /// <seealso cref="Rating"/>
        /// <seealso cref="ToggleRating"/>
        /// <seealso cref="ZuneApi.TrackRatingChanged"/>
        public event EventHandler RatingChanged;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes an invalid instance of the <see cref="Track"/> class and sets all values to their defaults.
        /// </summary>
        /// <see cref="Valid"/>
        public Track()
        {
            Uri = string.Empty;
            Index = -1;
            Title = string.Empty;
            Duration = TimeSpan.Zero;
            Position = TimeSpan.Zero;
            Artist = string.Empty;
            ArtistId = -1;
            Album = String.Empty;
            AlbumId = -1;
            ReleaseYear = 0;
            CoverUrl = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Track"/> class with all required informations.
        /// </summary>
        /// <param name="zuneTrack">The zune track.</param>
        /// <param name="index">The zero based position of the track in the current playlist.</param>
        [CLSCompliant(false)]
        public Track(PlaybackTrack zuneTrack, int index)
            : this()
        {
            if (zuneTrack != null)
            {
                ZuneTrack = zuneTrack;
                string trackUri;
                zuneTrack.GetURI(out trackUri);
                Uri = trackUri;
                Title = zuneTrack.Title;
                Duration = zuneTrack.Duration;
                MediaId = zuneTrack.ZuneMediaId.GetHashCode();
                ConvertedMediaTypeId = (int) zuneTrack.MediaType;

                if (zuneTrack is LibraryPlaybackTrack)
                {
                    LibraryPlaybackTrack libraryPlaybackTrack = (LibraryPlaybackTrack)zuneTrack;
                    MediaId = libraryPlaybackTrack.MediaId;

                    if (libraryPlaybackTrack.AlbumLibraryId > 0)
                    {
                        AlbumMetadata album = FindAlbumInfoHelper.GetAlbumMetadata(libraryPlaybackTrack.AlbumLibraryId);

                        Artist = album.AlbumArtist;
                        ArtistId = libraryPlaybackTrack.AlbumArtistLibraryId;
                        Album = album.AlbumTitle;
                        AlbumId = libraryPlaybackTrack.AlbumLibraryId;
                        ReleaseYear = album.ReleaseYear;
                        CoverUrl = album.CoverUrl;
                    }
                }
                else if (zuneTrack is MarketplacePlaybackTrack)
                {
                    MarketplacePlaybackTrack marketplacePlaybackTrack = (MarketplacePlaybackTrack)zuneTrack;

                    Artist = marketplacePlaybackTrack.Artist;
                    Album = marketplacePlaybackTrack.Album;
                }
                //else if (zuneTrack is StreamingPlaybackTrack)
                //{
                //    StreamingPlaybackTrack streamingPlaybackTrack = (StreamingPlaybackTrack)zuneTrack;
                //}
                //else if (zuneTrack is StreamingRadioPlaybackTrack)
                //{
                //    StreamingRadioPlaybackTrack streamingRadioPlaybackTrack = (StreamingRadioPlaybackTrack)zuneTrack;
                //}
                else if (zuneTrack is VideoPlaybackTrack)
                {
                    VideoPlaybackTrack videoPlaybackTrack = (VideoPlaybackTrack)zuneTrack;
                    Artist = videoPlaybackTrack.Artist;
                }

                // user rating
                Rating = (TrackRating)zuneTrack.UserRating;
                if (zuneTrack.RatingChanged.Available)
                {
                    // anonymous event handler should take care of possible memory leaks
                    zuneTrack.RatingChanged.PropertyChanged += delegate(object ratingSender, PropertyChangedEventArgs ratingEvent)
                    {
                        Rating = (TrackRating)zuneTrack.UserRating;
                    };
                }
            }
            Index = index;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Toggles the user rating of this track.
        /// </summary>
        /// <returns>The new track rating.</returns>
        /// <seealso cref="Rating"/>
        /// <seealso cref="RatingChanged"/>
        public TrackRating ToggleRating()
        {
            if (CanRate)
            {
                switch (rating)
                {
                    case TrackRating.Unrated:
                        rating = TrackRating.Like;
                        break;
                    case TrackRating.Like:
                        rating = TrackRating.DontLike;
                        break;
                    case TrackRating.DontLike:
                        rating = TrackRating.Unrated;
                        break;
                }
                Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
                {
                    ZuneTrack.UserRating = (int)rating;
                }), DeferredInvokePriority.Normal);
                if (RatingChanged != null)
                    RatingChanged(this, EventArgs.Empty);
            }
            return rating;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this track.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this track.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0:s} ({1:s})", Title, DurationFormat);
        }

        #endregion
    }
}
