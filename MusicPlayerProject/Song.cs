using System.Windows.Media;
using System.Xml.Serialization;

namespace MusicPlayerProject
{
    public class Song
    {
        public string Title  { get; set; }
        public string Path { get; set; }
        public string Artist { get; set; }
        public bool IsFavourite { get; set; }
        [XmlIgnore]
        public ImageSource Brush { get; set; }
        
        public Song(string title, string path, string artist, bool favourite, ImageSource brush)
        {
            Title = title;
            Path = path;
            Artist = artist;
            IsFavourite = favourite;
            Brush = brush;
        }

        //constructor for xml serialization
        public Song()
        {

        }
    }
}
