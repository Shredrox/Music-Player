using System.Collections.Generic;

namespace MusicPlayerProject
{
    public class Playlist
    {
        private string playlistName;
        private int songCount;
        private List<Song> songList;

        public string PlaylistName { get { return playlistName; } set { playlistName = value; } }
        public int SongCount { get { return songCount; } set { songCount = value;} }
        public List<Song> SongList { get { return songList; } set { songList = value; } }

        public Playlist()
        {

        }

        public override string ToString()
        {
            return playlistName;
        }
    }
}
