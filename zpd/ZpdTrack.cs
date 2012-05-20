using System;

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
        public float Duration { get; set; }

        public ZpdTrack (int mediaId, int mediaTypeId)
        {
            MediaId = mediaId;
            MediaTypeId = mediaTypeId;
        }

        public ZpdTrack(int mediaId, int mediaTypeId, string name, string artist, string album, float duration)
            : this(mediaId, mediaTypeId)
        {
            Name = name;
            Artist = artist;
            Album = album;
            Duration = duration;
        }

        public bool Equals(ZpdTrack other)
        {
            return MediaId == other.MediaId;
        }
    }
}