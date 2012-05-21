using System;
using System.Runtime.Serialization;
using VosSoft.ZuneLcd.Api;

namespace zpd
{
    /// <summary>
    /// Container class for track information
    /// </summary>
    [DataContract]
    public class ZpdTrack : IEquatable<ZpdTrack>
    {
        [DataMember]
        public int MediaId { get; private set; }
        [DataMember]
        public int MediaTypeId { get; private set; }
        [DataMember]
        public string Artist { get; set; }
        [DataMember]
        public string Album { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
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