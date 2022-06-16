using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace MusicPlayerProject
{
    [XmlInclude(typeof(Song))]
    public class Playlist
    {
        public string Name { get; set; }
        public int SongCount { get; set; }
        public List<Song> SongList { get; set; }

        public Playlist(string name, int songCount, List<Song> songList)
        {
            Name = name;
            SongCount = songCount;
            SongList = songList;
        }

        //constructor for xml serialization
        public Playlist()
        {

        }

        public override string ToString()
        {
            return Name;
        }
    }
}
