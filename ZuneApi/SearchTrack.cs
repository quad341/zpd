using System;

namespace VosSoft.ZuneLcd.Api
{
    public class SearchTrack
    {
        
        public int MediaId { get; private set; }
        public int MediaTypeId { get; private set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }

        public SearchTrack (int mediaId, int mediaTypeId)
        {
            MediaId = mediaId;
            MediaTypeId = mediaTypeId;
        }

        public SearchTrack(int mediaId, int mediaTypeId, string name, string artist, string album, int duration)
            : this(mediaId, mediaTypeId)
        {
            Name = name;
            Artist = artist;
            Album = album;
            Duration = duration;
        }

        public SearchTrack(Track track)
            : this(0, 0, track.Title, track.Artist, track.Album, Convert.ToInt32(track.Duration.TotalSeconds))
        {
        }

        public bool Equals(SearchTrack other)
        {
            return MediaId == other.MediaId;
        } 
    }
}