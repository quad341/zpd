using System;
using VosSoft.ZuneLcd.Api;

namespace zpd
{
    /// <summary>
    /// Container class for track information
    /// </summary>
    public class ZpdTrack : IEquatable<ZpdTrack>
    {
        public int MediaId { get; private set; }
        public int MediaTypeId { get; private set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }

        public ZpdTrack (int mediaId, int mediaTypeId)
        {
            MediaId = mediaId;
            MediaTypeId = mediaTypeId;
        }

        public ZpdTrack(int mediaId, int mediaTypeId, string name, string artist, string album, int duration)
            : this(mediaId, mediaTypeId)
        {
            Name = name;
            Artist = artist;
            Album = album;
            Duration = duration;
        }

        public ZpdTrack(SearchTrack track)
            : this(track.MediaId, track.MediaTypeId, track.Name, track.Artist, track.Album, track.Duration)
        {
        }

        public bool Equals(ZpdTrack other)
        {
            return MediaId == other.MediaId;
        }
    }
}