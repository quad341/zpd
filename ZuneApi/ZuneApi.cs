using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Microsoft.Iris;
using Microsoft.Zune.Shell;
using MicrosoftZuneLibrary;
using MicrosoftZunePlayback;
using ZuneUI;
using System.Diagnostics;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using System.IO;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace VosSoft.ZuneLcd.Api
{
    /// <summary>
    /// Defines the Zune LCD API class.
    /// <para>This class can be used to access the Microsoft Zune software.</para>
    /// </summary>
    public sealed class ZuneApi
    {
        #region Constants

        /// <summary>
        /// The maximum track count for the <see cref="GetTracks(int, int, TrackListCallback)"/> method.
        /// </summary>
        public const int MAX_TRACKS = 32;

        // transport controls constants for better performance
        private const string TC_CurrentTrack = "CurrentTrack";
        private const string TC_CurrentPlaylist = "CurrentPlaylist";

        private readonly string LUCENE_INDEX_DIRECTORY;

        #endregion

        #region Fields

        private string zuneArgs;

        private Thread zuneThread;
        private Thread initThread;

        // window properties
        private WindowState windowState;
        private bool showInTaskbar;

        // transport controls properties
        private float volume;
        private bool muted, repeating, shuffling;

        // keep track of the last track rating
        private TrackRating lastTrackRating = TrackRating.Invalid;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton instance of this <see cref="ZuneApi"/> class.
        /// <para>
        /// <remarks>This value can be <c>null</c> if the class has never been instantiated before.</remarks>
        /// </para>
        /// </summary>
        /// <value>The singleton instance of this class or <c>null</c>.</value>
        public static ZuneApi Instance { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the Zune software is running.
        /// </summary>
        /// <value>
        /// <c>true</c> if the Zune software is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the Zune software is ready for use.
        /// </summary>
        /// <value><c>true</c> if the Zune software is ready for use; otherwise, <c>false</c>.</value>
        /// <seealso cref="Ready"/>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Gets or sets the state of the Zune Software window.
        /// </summary>
        /// <value>The state of the window.</value>
        /// <seealso cref="WindowStatusChanged"/>
        public WindowState WindowState
        {
            get { return windowState; }
            set
            {
                if (!IsReady)
                    return;

                Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
                {
                    Application.Window.WindowState = (Microsoft.Iris.WindowState)value;
                }), DeferredInvokePriority.Normal);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Zune software window will be visible in the taskbar.
        /// </summary>
        /// <value><c>true</c> if the Zune Software window is visible in the taskbar; otherwise, <c>false</c>.</value>
        public bool ShowInTaskbar
        {
            get { return showInTaskbar; }
            set
            {
                if (!IsReady)
                    return;

                Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
                {
                    Application.Window.ShowInTaskbar = value;
                }), DeferredInvokePriority.Normal);
            }
        }

        /// <summary>
        /// Gets the current state of the Zune player.
        /// </summary>
        /// <value>The current state of the Zune player.</value>
        /// <seealso cref="PlayerStatusChanged"/>
        public PlayerState PlayerState { get; private set; }

        /// <summary>
        /// Gets the state of the current track.
        /// </summary>
        /// <value>The state of the current track.</value>
        /// <seealso cref="TrackStatusChanged"/>
        public TrackState TrackState { get; private set; }

        /// <summary>
        /// Gets the current track.
        /// <para>
        /// <remarks>The track informations are only valid if <see cref="TrackState"/>
        /// is not <c>TrackState.Invalid</c>.</remarks>
        /// </para>
        /// </summary>
        /// <value>The current track.</value>
        /// <seealso cref="TrackChanged"/>
        public Track CurrentTrack { get; private set; }

        /// <summary>
        /// Gets the type of the current track.
        /// </summary>
        /// <value>The type of the current track.</value>
        /// <seealso cref="TrackTypeChanged"/>
        public TrackType TrackType { get; private set; }

        /// <summary>
        /// Gets the track count of the current playlist.
        /// </summary>
        /// <value>The track count of the current playlist.</value>
        public int TrackCount { get; private set; }

        /// <summary>
        /// Gets a value indicating whether fast forwarding is currently active.
        /// </summary>
        /// <value><c>true</c> if fast forwarding is active; otherwise, <c>false</c>.</value>
        /// <seealso cref="FastForwardChanged"/>
        public bool FastForwarding { get; private set; }

        /// <summary>
        /// Gets a value indicating whether rewinding is currently active.
        /// </summary>
        /// <value><c>true</c> if rewinding is active; otherwise, <c>false</c>.</value>
        /// <seealso cref="FastForwardChanged"/>
        public bool Rewinding { get; private set; }

        /// <summary>
        /// Gets or sets the volume of the Zune player.
        /// <para>
        /// <remarks>The value must be between <c>0.0f</c> and <c>100.0f</c>.</remarks>
        /// </para>
        /// </summary>
        /// <value>The volume.</value>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        /// <seealso cref="VolumeChanged"/>
        public float Volume
        {
            get { return volume; }
            set
            {
                if (!IsReady)
                    return;

                // TODO: find a better way for the invocation inside the Zune dispatcher thread
                Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
                {
                    if (value < TransportControls.Instance.Volume.MinValue || value > TransportControls.Instance.Volume.MaxValue)
                        throw new ArgumentOutOfRangeException("value", value, "The volume must be between " + TransportControls.Instance.Volume.MinValue
                            + " and " + TransportControls.Instance.Volume.MaxValue + ".");
                    TransportControls.Instance.Volume.Value = value;
                }), DeferredInvokePriority.Normal);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Zune player is muted.
        /// </summary>
        /// <value><c>true</c> if muted; otherwise, <c>false</c>.</value>
        /// <seealso cref="MuteChanged"/>
        public bool Muted
        {
            get { return muted; }
            set
            {
                if (!IsReady)
                    return;

                Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
                {
                    TransportControls.Instance.Muted.Value = value;
                }), DeferredInvokePriority.Normal);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the playlist within the Zune player is repeating.
        /// </summary>
        /// <value><c>true</c> if repeating; otherwise, <c>false</c>.</value>
        /// <seealso cref="RepeatChanged"/>
        public bool Repeating
        {
            get { return repeating; }
            set
            {
                if (!IsReady)
                    return;

                Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
                {
                    TransportControls.Instance.Repeating.Value = value;
                }), DeferredInvokePriority.Normal);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the playlist within the Zune player is shuffling.
        /// </summary>
        /// <value><c>true</c> if shuffling; otherwise, <c>false</c>.</value>
        /// <seealso cref="ShuffleChanged"/>
        public bool Shuffling
        {
            get { return shuffling; }
            set
            {
                if (!IsReady)
                    return;

                Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
                {
                    if (TransportControls.Instance.PlaylistSupportsShuffle)
                        TransportControls.Instance.Shuffling.Value = value;
                }), DeferredInvokePriority.Normal);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the Zune software is starting up.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="Launch(string)"/>
        public event EventHandler Starting;

        /// <summary>
        /// Occurs when the Zune software is ready for use.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="IsReady"/>
        public event EventHandler Ready;

        /// <summary>
        /// Occurs when the Zune software just closed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="Close"/>
        public event EventHandler Closed;

        /// <summary>
        /// Occurs after the window state of the Zune Software has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="WindowState"/>
        public event EventHandler WindowStatusChanged;

        /// <summary>
        /// Occurs after the state of the Zune player has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="PlayerState"/>
        public event EventHandler PlayerStatusChanged;

        /// <summary>
        /// Occurs after the state of the current track has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="TrackState"/>
        public event EventHandler TrackStatusChanged;

        /// <summary>
        /// Occurs after the current track has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="CurrentTrack"/>
        public event EventHandler TrackChanged;

        /// <summary>
        /// Occurs after the type of the current track has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="TrackType"/>
        public event EventHandler TrackTypeChanged;

        /// <summary>
        /// Occurs after the playing position of the current track has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="CurrentTrack">Position</seealso>
        public event EventHandler TrackPositionChanged;

        /// <summary>
        /// Occurs after the user rating of the current track has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="Track.Rating"/>
        /// <seealso cref="Track.RatingChanged"/>
        public event EventHandler TrackRatingChanged;

        /// <summary>
        /// Occurs after the current playlist has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="GetTracks(int, int, TrackListCallback)"/>
        public event EventHandler TrackListChanged;

        /// <summary>
        /// Occurs after the fast forward value has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="FastForwarding"/>
        public event EventHandler FastForwardChanged;

        /// <summary>
        /// Occurs after the rewind value has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="Rewinding"/>
        public event EventHandler RewindChanged;

        /// <summary>
        /// Occurs after the volume of the Zune player has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="Volume"/>
        public event EventHandler VolumeChanged;

        /// <summary>
        /// Occurs after the muted value of the Zune player has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="Muted"/>
        public event EventHandler MuteChanged;

        /// <summary>
        /// Occurs after the repeating value of the playlist within the Zune player has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="Repeating"/>
        public event EventHandler RepeatChanged;

        /// <summary>
        /// Occurs after the shuffling value of the playlist within the Zune player has changed.
        /// <para>
        /// <remarks>Note that this event is not thread safe, please take the necessary precautions.</remarks>
        /// </para>
        /// </summary>
        /// <seealso cref="Shuffling"/>
        public event EventHandler ShuffleChanged;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="ZuneApi"/> class.
        /// <para>
        /// <remarks>Note that you have to call the <see cref="Launch(string)"/>-method to start the Zune software.</remarks>
        /// </para>
        /// </summary>
        public ZuneApi()
        {
            IsRunning = IsReady = false;
            PlayerState = PlayerState.Uninitialized;
            TrackState = TrackState.Invalid;
            CurrentTrack = new Track();
            TrackType = TrackType.None;

            LUCENE_INDEX_DIRECTORY = Environment.GetEnvironmentVariable("localappdata") + "\\VosSoft\\ZuneApi";

            ZuneApi.Instance = this;
        }

        /// <summary>
        /// Launches the Zune software with the specified command line arguments.
        /// </summary>
        /// <param name="args">The command line arguments for starting the Zune software.</param>
        /// <returns><c>true</c> if the Zune software is not already running; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <seealso cref="Starting"/>
        public bool Launch(string args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            if (IsRunning)
                return false;

            zuneArgs = args;
            zuneThread = new Thread(new ThreadStart(Zune));
            zuneThread.Start();

            initThread = new Thread(new ThreadStart(Init));
            initThread.Start();

            return (IsRunning = true);
        }

        /// <summary>
        /// Launches the Zune software.
        /// </summary>
        /// <returns><c>true</c> if the Zune software is not already running; otherwise, <c>false</c>.</returns>
        /// <seealso cref="Starting"/>
        public bool Launch()
        {
            return Launch(string.Empty);
        }

        /// <summary>
        /// Closes the Zune software.
        /// </summary>
        /// <seealso cref="Closed"/>
        public void Close()
        {
            if (IsRunning)
                Application.DeferredInvoke(new DeferredInvokeHandler(CloseZune), null, DeferredInvokePriority.Normal);
        }

        /// <summary>
        /// Plays the current track.
        /// </summary>
        public void Play()
        {
            if (!IsReady || TrackState == TrackState.Playing)
                return;

            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                if (TransportControls.Instance.Play.Available)
                {
                    if (FastForwarding)
                        ToggleFastForward();
                    else
                        TransportControls.Instance.Play.Invoke(InvokePolicy.AsynchronousNormal);
                }
            }), DeferredInvokePriority.Normal);
        }

        /// <summary>
        /// Plays the track with the specified track index.
        /// </summary>
        /// <param name="trackIndex">Index of the track.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        public void PlayAt(int trackIndex)
        {
            if (!IsReady || !TransportControls.Instance.HasPlaylist)
                return;

            if (trackIndex < 0 || trackIndex >= TrackCount)
                throw new ArgumentOutOfRangeException("trackIndex", trackIndex,
                    "The track index must be between 0 and less then the TrackCount (" + TrackCount + ")!");

            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                TransportControls.Instance.StartPlayingAt(trackIndex);
            }), DeferredInvokePriority.Normal);
        }

        /// <summary>
        /// Pauses the current track.
        /// </summary>
        public void Pause()
        {
            if (!IsReady || TrackState == TrackState.Paused)
                return;

            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                if (TransportControls.Instance.Pause.Available)
                    TransportControls.Instance.Pause.Invoke(InvokePolicy.AsynchronousNormal);
            }), DeferredInvokePriority.Normal);
        }

        /// <summary>
        /// Stops the current track.
        /// </summary>
        public void Stop()
        {
            if (!IsReady || TrackState == TrackState.Stopped)
                return;

            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                if (TransportControls.Instance.Stop.Available)
                    TransportControls.Instance.Stop.Invoke(InvokePolicy.AsynchronousNormal);
            }), DeferredInvokePriority.Normal);
        }

        /// <summary>
        /// Goes to the next track (if any).
        /// </summary>
        public void Forward()
        {
            if (!IsReady)
                return;

            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                if (TransportControls.Instance.Forward.Available)
                    TransportControls.Instance.Forward.Invoke(InvokePolicy.AsynchronousNormal);
            }), DeferredInvokePriority.Normal);
        }

        /// <summary>
        /// Jumps to the beginning of the current track or goes to the previous track (if any).
        /// </summary>
        public void Back()
        {
            if (!IsReady)
                return;

            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                if (TransportControls.Instance.Back.Available)
                    TransportControls.Instance.Back.Invoke(InvokePolicy.AsynchronousNormal);
            }), DeferredInvokePriority.Normal);
        }

        /// <summary>
        /// Gets an amount of tracks from the current track list. The result is received through an asynchronous callback.
        /// </summary>
        /// <param name="startIndex">The start index of the current track list.</param>
        /// <param name="count">The track count (0 for all tracks).</param>
        /// <param name="callback">The callback when the track data arrives.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.IndexOutOfRangeException"/>
        /// <seealso cref="TrackListChanged"/>
        /// <seealso cref="MAX_TRACKS"/>
        public void GetTracks(int startIndex, int count, TrackListCallback callback)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", startIndex, "The start index must be >= 0!");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", count, "Count must be >= 0!");
            if (callback == null)
                throw new ArgumentNullException("callback");

            if (!IsReady || !TransportControls.Instance.HasPlaylist)
                return;

            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                ArrayListDataSet playlist = TransportControls.Instance.CurrentPlaylist;

                if (playlist == null)
                    return;

                if (count == 0)
                    count = playlist.Count - startIndex;

                if (count > ZuneApi.MAX_TRACKS)
                    count = ZuneApi.MAX_TRACKS;

                if (startIndex + count > playlist.Count)
                    throw new IndexOutOfRangeException("Start index (" + startIndex + ") + count ("
                        + count + ") must be <= TrackCount (" + playlist.Count + ")!");

                Track[] tracks = new Track[count];
                for (int i = 0; i < count; i++)
                {
                    int playlistIndex = i + startIndex;
                    object playlistItem = playlist[playlistIndex];
                    if (playlistItem is PlaybackTrack)
                        tracks[i] = new Track((PlaybackTrack)playlistItem, playlistIndex);
                    else
                        tracks[i] = new Track();
                }

                callback(tracks);
            }), DeferredInvokePriority.Low); // low priority to save performance
        }

        /// <summary>
        /// Given an input (lucene compatable) query, searches for matching tracks with a similar artist, album, or title in the index
        /// </summary>
        /// <param name="queryString">The lucene compatible query</param>
        /// <returns>The list of matching tracks</returns>
        /// <seealso cref="ReIndexMusic"/>
        public IEnumerable<SearchTrack> Search(String queryString)
        {
            var query = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_29, new String[]{"artist", "album", "title"}, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29)).Parse(queryString);
            var searcher = new IndexSearcher(IndexReader.Open(new SimpleFSDirectory(new DirectoryInfo(LUCENE_INDEX_DIRECTORY)), true));
            var collector = TopScoreDocCollector.Create(100/*numHits*/, true/*docsSortedInOrder*/);
            searcher.Search(query, collector);
            var hits = collector.TopDocs().ScoreDocs;

            var result = new List<SearchTrack>(hits.Length);

            for (var i = 0; i < hits.Length; i++ )
            {
                var docId = hits[i].Doc;
                var doc = searcher.Doc(docId);
                result.Add(new SearchTrack(Int32.Parse(doc.GetField("mediaId").StringValue()),
                                           Int32.Parse(doc.GetField("mediaTypeId").StringValue()),
                                           doc.GetField("title").StringValue(),
                                           doc.GetField("artist").StringValue(),
                                           doc.GetField("album").StringValue(),
                                           float.Parse(doc.GetField("duration").StringValue())));
            }

            searcher.Close();

            return result;
        }

        // Using http://blog.ctaggart.com/2010/08/query-zune-music-collection-with-f.html#!/2010/08/query-zune-music-collection-with-f.html as a base
        // Also found some help at http://averagedeveloper.blogspot.com/2012/04/querying-zunedbapidll.html
        /// <summary>
        /// Reindexes the music by iterating through all music in the Zune Library and saving it in the local Lucene database. Required for search to work correctly
        /// </summary>
        /// <seealso cref="Search"/>
        public void ReIndexMusic()
        {
            var library = new ZuneLibrary();
            var dbReloaded = false;
            int returnValue = library.Initialize(null, out dbReloaded);
            if (returnValue >= 0)
            {
                library.Phase2Initialization(out returnValue);
                if (returnValue >= 0)
                {
                    library.CleanupTransientMedia();
                    
                    ZuneQueryList searchResult = library.QueryDatabase(EQueryType.eQueryTypeAllTracks, 0, EQuerySortType.eQuerySortOrderNone, 0, null);
                    if (null != searchResult)
                    {
                        // we have results, index them
                        var writer = new IndexWriter(new SimpleFSDirectory(new DirectoryInfo(LUCENE_INDEX_DIRECTORY)), new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), true, IndexWriter.MaxFieldLength.UNLIMITED);
                        for (uint i = 0; i < searchResult.Count; i++)
                        {
                            // add a document for each song. We will only store the mediaId and mediaTypeId and store+index the title, artist, and album
                            var doc = new Document();

                            var mediaId = (UInt32)searchResult.GetFieldValue(i, typeof(UInt32), (uint)MicrosoftZuneLibrary.SchemaMap.kiIndex_MediaID);
                            var mediaTypeId = (UInt32)searchResult.GetFieldValue(i, typeof(UInt32), (uint)MicrosoftZuneLibrary.SchemaMap.kiIndex_MediaType);
                            var duration = (float)searchResult.GetFieldValue(i, typeof(float), (uint)MicrosoftZuneLibrary.SchemaMap.kiIndex_Duration);
                            var artist = (String)searchResult.GetFieldValue(i, typeof(String), (uint)MicrosoftZuneLibrary.SchemaMap.kiIndex_DisplayArtist);
                            var title = (String)searchResult.GetFieldValue(i, typeof(String), (uint)MicrosoftZuneLibrary.SchemaMap.kiIndex_Title);
                            var album = (String)searchResult.GetFieldValue(i, typeof(String), (uint)MicrosoftZuneLibrary.SchemaMap.kiIndex_WMAlbumTitle);

                            doc.Add(new Field("mediaId", mediaId.ToString(), Field.Store.YES, Field.Index.NO));
                            doc.Add(new Field("mediaTypeId", mediaTypeId.ToString(), Field.Store.YES, Field.Index.NO));
                            doc.Add(new Field("duration", duration.ToString(), Field.Store.YES, Field.Index.NO));
                            doc.Add(new Field("artist", artist, Field.Store.YES, Field.Index.ANALYZED));
                            doc.Add(new Field("title", title, Field.Store.YES, Field.Index.ANALYZED));
                            doc.Add(new Field("album", album, Field.Store.YES, Field.Index.ANALYZED));
                            
                            writer.AddDocument(doc);
                        }
                        writer.Commit();
                        writer.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Toggles the fast forward value.
        /// </summary>
        /// <seealso cref="FastForwarding"/>
        public void ToggleFastForward()
        {
            if (!IsReady || TrackState != TrackState.Playing)
                return;

            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                if (!TransportControls.Instance.Fastforwarding.IsDisposed)
                    TransportControls.Instance.Fastforwarding.Value = !TransportControls.Instance.Fastforwarding.Value;
            }), DeferredInvokePriority.Normal);
        }

        /// <summary>
        /// Toggles the rewind value.
        /// </summary>
        /// <seealso cref="FastForwarding"/>
        public void ToggleRewind()
        {
            if (!IsReady || TrackState != TrackState.Playing)
                return;

            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                if (!TransportControls.Instance.Fastforwarding.IsDisposed)
                    TransportControls.Instance.Rewinding.Value = !TransportControls.Instance.Rewinding.Value;
            }), DeferredInvokePriority.Normal);
        }

        /// <summary>
        /// Jumps to the specified position in the current track (if the track is searchable).
        /// <para>
        /// <remarks>The value must be between <c>0.0f</c> and the duration of the current track.</remarks>
        /// </para>
        /// </summary>
        /// <param name="seconds">The seconds.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        /// <seealso cref="CurrentTrack">Position</seealso>
        /// <seealso cref="CurrentTrack">Duration</seealso>
        /// <seealso cref="TrackPositionChanged"/>
        public void SeekToPosition(float seconds)
        {
            if (!IsReady)
                return;

            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                if (!TransportControls.Instance.IsSeekEnabled)
                    return;

                if (seconds < 0.0f || seconds > TransportControls.Instance.CurrentTrackDuration)
                    throw new ArgumentOutOfRangeException("seconds", seconds, "The seek position must be between 0.0f and the duration of the current track ("
                        + TransportControls.Instance.CurrentTrackDuration + ").");

                TransportControls.Instance.SeekToPosition(seconds);
            }), DeferredInvokePriority.Normal);
        }

        /// <summary>
        /// Toggles the muted value.
        /// </summary>
        /// <returns><c>true</c> if the Zune player is muted after this call; otherwise, <c>false</c>.</returns>
        /// <seealso cref="Muted"/>
        public bool ToggleMute()
        {
            return (Muted = !muted);
        }

        /// <summary>
        /// Toggles the repeating value.
        /// </summary>
        /// <returns><c>true</c> if the playlist within the Zune player is repeating after this call; otherwise, <c>false</c>.</returns>
        /// <seealso cref="Repeating"/>
        public bool ToggleRepeat()
        {
            return (Repeating = !repeating);
        }

        /// <summary>
        /// Toggles the shuffling value.
        /// </summary>
        /// <returns><c>true</c> if the playlist in the Zune player is shuffling after this call; otherwise, <c>false</c>.</returns>
        /// <seealso cref="Shuffling"/>
        public bool ToggleShuffle()
        {
            return (Shuffling = !shuffling);
        }

        [CLSCompliant(false)]
        public void AddTrackToCurrentPlaylist(int mediaId, MediaType mediaType)
        {
            // create a list of tracks to append
            var track = new LibraryPlaybackTrack(mediaId, mediaType, null);
            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                TransportControls.Instance.CurrentPlaylist.Add(track);
            }), DeferredInvokePriority.Normal);
        }



        [CLSCompliant(false)]
        public void AddTrackToCurrentPlaylistAtIndex(int mediaId, MediaType mediaType, int index)
        {
            // create a list of tracks to append
            var track = new LibraryPlaybackTrack(mediaId, mediaType, null);
            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                TransportControls.Instance.CurrentPlaylist.Insert(index, track);
            }), DeferredInvokePriority.Normal);
        }

        public void RemoveTrackAtIndexFromCurrentPlaylist(int index)
        {
            if (index < 0 || index >= TrackCount)
            {
                throw new ArgumentOutOfRangeException("index", "index must be >=0 and < TrackCount");
            }

            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                TransportControls.Instance.CurrentPlaylist.RemoveAt(index);
            }), DeferredInvokePriority.Normal);
        }

        public void GetCurrentTrackPosition(GetCurrentPositionCallback callback)
        {
            // Because this is not synchronous, this might actually be a little off.
            Application.DeferredInvoke(new DeferredInvokeHandler(delegate(object sender)
            {
                callback(TransportControls.Instance.CurrentTrackPosition);
            }), DeferredInvokePriority.Normal);
        }

        [CLSCompliant(false)]
        public static MediaType MediaTypeFromInt(int mediaTypeId)
        {
            var mt = (MediaType)mediaTypeId;
#if DEBUG
            var mtDebug = MediaType.Undefined;
            var uMediaTypeId = Convert.ToUInt32(mediaTypeId);
            switch (uMediaTypeId)
            {
                case 3:
                    mtDebug = MediaType.Track;
                    break;
                case 4:
                    mtDebug = MediaType.Video;
                    break;
                case 5:
                    mtDebug = MediaType.Photo;
                    break;
                case 9:
                    mtDebug = MediaType.Playlist;
                    break;
                case 11:
                    mtDebug = MediaType.Album;
                    break;
                case 17:
                    mtDebug = MediaType.PodcastEpisode;
                    break;
                case 18:
                    mtDebug = MediaType.Podcast;
                    break;
                case 20:
                    mtDebug = MediaType.MediaFolder;
                    break;
                case 21:
                    mtDebug = MediaType.Genre;
                    break;
                case 32:
                    mtDebug = MediaType.AudioMP4;
                    break;
                case 33:
                    mtDebug = MediaType.AudioMP3;
                    break;
                case 34:
                    mtDebug = MediaType.AudioWMA;
                    break;
                case 35:
                    mtDebug = MediaType.AudioWAV;
                    break;
                case 36:
                    mtDebug = MediaType.ImageJPEG;
                    break;
                case 37:
                    mtDebug = MediaType.VideoMP4;
                    break;
                case 38:
                    mtDebug = MediaType.VideoMPG;
                    break;
                case 39:
                    mtDebug = MediaType.VideoWMV;
                    break;
                case 40:
                    mtDebug = MediaType.VideoQT;
                    break;
                case 41:
                    mtDebug = MediaType.AudioQT;
                    break;
                case 42:
                    mtDebug = MediaType.VideoDVRMS;
                    break;
                case 43:
                    mtDebug = MediaType.VideoMBR;
                    break;
                case 44:
                    mtDebug = MediaType.VideoAVI;
                    break;
                case 50:
                    mtDebug = MediaType.PlaylistChannel;
                    break;
                case 56:
                    mtDebug = MediaType.PlaylistContentItem;
                    break;
                case 65:
                    mtDebug = MediaType.Artist;
                    break;
                case 96:
                    mtDebug = MediaType.UserCard;
                    break;
                case 110:
                    mtDebug = MediaType.Application;
                    break;
                case 4294967295:
                default:
                    mtDebug = MediaType.Undefined;
                    break;
            }
            Debug.Assert(mt == mtDebug);
#endif //DEBUG
            return mt;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The Zune thread.
        /// </summary>
        private void Zune()
        {
            if (Starting != null)
                Starting(this, EventArgs.Empty);

            ZuneApplication.Launch(zuneArgs, IntPtr.Zero);

            IsRunning = IsReady = false;
            if (Closed != null)
                Closed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Closes the Zune software.
        /// </summary>
        /// <param name="sender">The sender is required for the <see cref="Microsoft.Iris.DeferredInvokeHandler"/> 
        /// delegate used with the Zune software.</param>
        private void CloseZune(object sender)
        {
            Application.Window.Close();
        }

        /// <summary>
        /// The Zune init thread is used to initialize all event handlers used within this API.
        /// The thread will wait until the Zune software is ready for use.
        /// </summary>
        /// <see cref="MicrosoftZunePlayback.PlayerInterop">ZuneDBApi.dll</see>
        private void Init()
        {
            while (ZuneShell.DefaultInstance == null || PlayerInterop.Instance == null)
            {
                Thread.Sleep(100);
            }

            PlayerInterop.Instance.StatusChanged += new EventHandler(StatusChanged);
            PlayerInterop.Instance.TransportStatusChanged += new EventHandler(TransportStatusChanged);
            PlayerInterop.Instance.TransportPositionChanged += new EventHandler(TransportPositionChanged);

            Application.DeferredInvoke(new DeferredInvokeHandler(ZuneSetup), DeferredInvokePriority.Normal);
        }

        /// <summary>
        /// Hooks up to all event handlers which must be called from the Zune's dispatcher thread.
        /// </summary>
        /// <param name="sender">The sender is required for the <see cref="Microsoft.Iris.DeferredInvokeHandler"/> 
        /// delegate used with the Zune software.</param>
        /// <see cref="ZuneUI.TransportControls">ZuneShell.dll</see>
        private void ZuneSetup(object sender)
        {
            // Zune Software Window
            windowState = (WindowState)Application.Window.WindowState;
            showInTaskbar = Application.Window.ShowInTaskbar;

            Application.Window.PropertyChanged += new PropertyChangedEventHandler(Window_PropertyChanged);

            // Zune Software TransportControls
            FastForwarding = false;
            volume = TransportControls.Instance.Volume.Value;

            muted = TransportControls.Instance.Muted.Value;
            repeating = TransportControls.Instance.Repeating.Value;
            shuffling = TransportControls.Instance.Shuffling.Value;

            TransportControls.Instance.Fastforwarding.ChosenChanged += new EventHandler(Fastforwarding_ChosenChanged);
            TransportControls.Instance.Rewinding.ChosenChanged += new EventHandler(Rewinding_ChosenChanged);
            TransportControls.Instance.Volume.PropertyChanged += new PropertyChangedEventHandler(Volume_PropertyChanged);

            TransportControls.Instance.Muted.ChosenChanged += new EventHandler(Muted_ChosenChanged);
            TransportControls.Instance.Repeating.ChosenChanged += new EventHandler(Repeating_ChosenChanged);
            TransportControls.Instance.Shuffling.ChosenChanged += new EventHandler(Shuffling_ChosenChanged);

            TransportControls.Instance.PropertyChanged += new PropertyChangedEventHandler(TransportControls_PropertyChanged);

            IsReady = true;
            if (Ready != null)
                Ready(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the PropertyChanged event of the Zune Window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Window_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("WindowState"))
            {
                windowState = (WindowState)Application.Window.WindowState;

                if (WindowStatusChanged != null)
                    WindowStatusChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// This method will be invoked after the state of the Zune player has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void StatusChanged(object sender, EventArgs e)
        {
            PlayerState = (PlayerState)PlayerInterop.Instance.State;

            if (PlayerStatusChanged != null)
                PlayerStatusChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// This method will be invoked after the transport state of the current track has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void TransportStatusChanged(object sender, EventArgs e)
        {
            TrackState = (TrackState)PlayerInterop.Instance.TransportState;

            if (TrackStatusChanged != null)
                TrackStatusChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// This method will be invoked after the transport position of the current track has changed.
        /// <para>
        /// <remarks>Usually this will be every 100ms if the track is playing.
        /// See <see cref="PlayerInterop.PositionEventInterval"/> for more details.
        /// </remarks>
        /// </para>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void TransportPositionChanged(object sender, EventArgs e)
        {
            CurrentTrack.Position = TimeSpan.FromSeconds(TransportControls.Instance.CurrentTrackPosition);

            if (TrackPositionChanged != null)
                TrackPositionChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the ChosenChanged event of the Fastforwarding control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Fastforwarding_ChosenChanged(object sender, EventArgs e)
        {
            FastForwarding = TransportControls.Instance.Fastforwarding.Value;

            if (FastForwardChanged != null)
                FastForwardChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the ChosenChanged event of the Rewinding control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Rewinding_ChosenChanged(object sender, EventArgs e)
        {
            Rewinding = TransportControls.Instance.Rewinding.Value;

            if (RewindChanged != null)
                RewindChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the PropertyChanged event of the Volume control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Volume_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            volume = TransportControls.Instance.Volume.Value;

            if (VolumeChanged != null)
                VolumeChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the ChosenChanged event of the Muted control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Muted_ChosenChanged(object sender, EventArgs e)
        {
            muted = TransportControls.Instance.Muted.Value;

            if (MuteChanged != null)
                MuteChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the ChosenChanged event of the Repeating control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Repeating_ChosenChanged(object sender, EventArgs e)
        {
            repeating = TransportControls.Instance.Repeating.Value;

            if (RepeatChanged != null)
                RepeatChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the ChosenChanged event of the Shuffling control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Shuffling_ChosenChanged(object sender, EventArgs e)
        {
            shuffling = TransportControls.Instance.Shuffling.Value;

            if (ShuffleChanged != null)
                ShuffleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the PropertyChanged event of the TransportControls of the Zune player.
        /// This is used for low level access on the TransportControls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void TransportControls_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(ZuneApi.TC_CurrentTrack)) // the current track has changed
            {
                TrackType oldTrackType = TrackType;

                // remove old track rating event handler
                if (CurrentTrack != null)
                    CurrentTrack.RatingChanged -= CurrentTrack_RatingChanged;

                if (TransportControls.Instance.CurrentTrack == null) // playlist removed
                {
                    CurrentTrack = new Track(); // the track is always valid for compatibility
                    TrackType = TrackType.None;
                }
                else
                {
                    PlaybackTrack playbackTrack = TransportControls.Instance.CurrentTrack;
                    CurrentTrack = new Track(playbackTrack, TransportControls.Instance.CurrentTrackIndex);

                    switch (playbackTrack.MediaType)
                    {
                        case MediaType.Track:
                        case MediaType.AudioMP4:
                        case MediaType.AudioMP3:
                        case MediaType.AudioWMA:
                        case MediaType.AudioWAV:
                        case MediaType.AudioQT:
                            TrackType = TrackType.Music;
                            break;
                        case MediaType.Video:
                        case MediaType.VideoMP4:
                        case MediaType.VideoMPG:
                        case MediaType.VideoWMV:
                        case MediaType.VideoQT:
                        case MediaType.VideoDVRMS:
                        case MediaType.VideoMBR:
                        case MediaType.VideoAVI:
                            TrackType = TrackType.Video;
                            break;
                        case MediaType.PodcastEpisode:
                        case MediaType.Podcast:
                            TrackType = TrackType.Podcast;
                            break;
                        default:
                            TrackType = TrackType.Undefined;
                            break;
                    }
                }

                if (TrackChanged != null)
                    TrackChanged(this, EventArgs.Empty);

                if (TrackTypeChanged != null && TrackType != oldTrackType)
                    TrackTypeChanged(this, EventArgs.Empty);

                // user rating, fire change event immediately and hook up event handler
                CurrentTrack_RatingChanged(this, EventArgs.Empty);
                CurrentTrack.RatingChanged += CurrentTrack_RatingChanged;
            }
            else if (e.PropertyName.Equals(ZuneApi.TC_CurrentPlaylist)) // the current playlist has changed
            {
                if (TransportControls.Instance.CurrentPlaylist == null) // playlist removed
                {
                    TrackCount = 0;

                    if (TrackListChanged != null)
                        TrackListChanged(this, EventArgs.Empty);

                    return;
                }

                TransportControls.Instance.CurrentPlaylist.ContentsChanged += CurrentPlaylist_ContentsChanged;

                GetTrackCount();

                // TODO: the current track index might have changed!
            }
        }

        /// <summary>
        /// Handles the RatingChanged event of the CurrentTrack.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void CurrentTrack_RatingChanged(object sender, EventArgs e)
        {
            // only fire if the rating has changed
            if (TrackRatingChanged != null && CurrentTrack.Rating != lastTrackRating)
            {
                TrackRatingChanged(this, EventArgs.Empty);
                lastTrackRating = CurrentTrack.Rating;
            }
        }

        /// <summary>
        /// Handles the ContentsChanged event of the CurrentPlaylist.
        /// </summary>
        /// <param name="senderList">The sender list.</param>
        /// <param name="args">The args.</param>
        private void CurrentPlaylist_ContentsChanged(IList senderList, ListContentsChangedArgs args)
        {
            GetTrackCount();
        }

        /// <summary>
        /// Gets the track count and invokes the <see cref="TrackListChanged"/> event.
        /// </summary>
        private void GetTrackCount()
        {
            TrackCount = TransportControls.Instance.CurrentPlaylist.Count;

            if (TrackListChanged != null)
                TrackListChanged(this, EventArgs.Empty);
        }

        #endregion
    }

    /// <summary>
    /// The callback for the <see cref="ZuneApi.GetTracks(int, int, TrackListCallback)"/> method.
    /// <para>
    /// <remarks>Note that this delegate is not thread safe, please take the necessary precautions.</remarks>
    /// </para>
    /// </summary>
    /// <seealso cref="ZuneApi.GetTracks(int, int, TrackListCallback)"/>
    public delegate void TrackListCallback(Track[] tracks);


    /// <summary>
    /// The callback for the <see cref="ZuneApi.GetCurrentPosition(GetCurrentPositionCallback)"/> method.
    /// <para>
    /// <remarks>Note that this delegate is not thread safe, please take the necessary precautions.</remarks>
    /// </para>
    /// </summary>
    /// <seealso cref="ZuneApi.GetCurrentPosition(GetCurrentPositionCallback)"/>
    public delegate void GetCurrentPositionCallback(float currentPosition);
}
