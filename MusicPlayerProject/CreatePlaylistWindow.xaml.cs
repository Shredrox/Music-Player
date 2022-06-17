using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MusicPlayerProject
{
    public partial class CreatePlaylistWindow : Window
    {
        public Playlist newPlaylist;
        private List<Song> newSongList = new List<Song>();
        private string artistName;

        public CreatePlaylistWindow()
        {
            InitializeComponent();
        }

        private void ArtistTagGetter(string path)
        {
            TagLib.File file = TagLib.File.Create(path);
            if (file.Tag.AlbumArtists.Length != 0)
            {
                artistName = file.Tag.AlbumArtists[0];
            }
            else if (file.Tag.Performers.Length != 0)
            {
                artistName = file.Tag.Performers[0];
            }
            else
            {
                artistName = "No artist data";
            }
        }

        private void AddSongsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog musicFiles = new OpenFileDialog();
            musicFiles.Filter = "MUSIC-Files|*.mp3;*.wav;*.flac";
            musicFiles.Multiselect = true;
            musicFiles.CheckFileExists = true;

            if (musicFiles.ShowDialog() == true)
            {
                foreach (string filename in musicFiles.FileNames)
                {
                    PlaylistSongsListBox.Items.Add(filename); 
                    LoadSongData(filename);
                }

                PlaylistSongsListBox.Items.Refresh();
            }
        }

        private void LoadSongData(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            string songName = Path.GetFileName(Path.GetFileNameWithoutExtension(path));
            if (MainWindow.SongTitleTagGetter(path) != String.Empty)
            {
                songName = MainWindow.SongTitleTagGetter(path);
            }

            string songPath = Path.GetFullPath(path);
            ArtistTagGetter(songPath);

            string artist = artistName;
            TagLib.File file1 = TagLib.File.Create(path);
            BitmapImage bitmap;

            if (file1.Tag.Pictures == null || file1.Tag.Pictures.Length == 0)
            {
                bitmap = MainWindow.CreateImageFromPath("../../Images/songicon.png");
            }
            else
            {
                TagLib.IPicture pic1 = file1.Tag.Pictures[0];
                MemoryStream ms = new MemoryStream(pic1.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                bitmap = MainWindow.CreateImageFromStream(ms);
            }

            newSongList.Add(new Song(songName, @songPath, artist, false, bitmap));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if(NewPlaylistName.Text == String.Empty)
            {
                MessageBox.Show("Enter a name for the new playlist.");
                return;
            }
            else if(newSongList.Count == 0){
                MessageBox.Show("No songs loaded. Add songs to create the new playlist.");
                return;
            }

            newPlaylist = new Playlist(NewPlaylistName.Text, newSongList.Count, newSongList);

            DialogResult = true;
        }
    }
}
