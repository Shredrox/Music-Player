using System;
using System.Windows.Media;

namespace MusicPlayerProject
{
    public class Song
    {
        private string songTitle = "";
        private string songPath = "";
        private string songArtist = "";
        private bool isFavourite;
        private ImageSource brush;

        public string SongTitle { get => songTitle; set => songTitle = value; }
        public string SongPath { get => songPath; set => songPath = value; }
        public ImageSource Brush { get => brush; set => brush = value; }
        public String SongArtist { get => songArtist; set => songArtist = value; }
        public bool IsFavourite { get => isFavourite; set => isFavourite = value; }

        public Song()
        {

        }
    }
}
