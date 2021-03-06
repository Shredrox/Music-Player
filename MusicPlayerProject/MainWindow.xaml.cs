using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

namespace MusicPlayerProject
{
    public partial class MainWindow : Window
    {
        private List<Song> loadedSongsList = new List<Song>();
        private List<Playlist> playlists = new List<Playlist>();
        private List<Song> favourites = new List<Song>();
        private bool sliderDrag = false;
        private int currentSongIndex = -1;
        private bool shuffleOn = false;
        private bool repeatOn = false;
        private bool favListOnDisplay = false;
        private bool loadedPlaylist = false;
        private bool songsLoaded = false;
        private string artistName = string.Empty;
        private Song selectedSong;

        public MainWindow()
        {
            InitializeComponent();
            LoadPlaylists();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        //loads playlists from file
        private void LoadPlaylists()
        {
            if (!File.Exists("data"))
            {
                return;
            }

            XmlSerializer mySerializer = new XmlSerializer(typeof(List<Playlist>));

            using (var myFileStream = new FileStream("data", FileMode.Open))
            {
                playlists = (List<Playlist>)mySerializer.Deserialize(myFileStream);
            }

            foreach (Playlist playlist in playlists)
            {
                PlaylistsListBox.Items.Add(playlist);

                foreach (Song song in playlist.SongList)
                {
                    SetSongBrush(song);

                    if(song.IsFavourite == true)
                    {
                        favourites.Add(song);
                    }
                }
            }
        }

        //saves playlists to file
        private void SavePlaylists()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Playlist>));
            XmlTextWriter writer = new XmlTextWriter("data", Encoding.UTF8);
            serializer.Serialize(writer, playlists);
            writer.Close();
        }

        //slider timer tick
        private void Timer_Tick(object sender, object e)
        {
            if ((MusicPlayer.Source != null) && MusicPlayer.NaturalDuration.HasTimeSpan && (!sliderDrag))
            {
                SongDuration.Minimum = 0;
                SongDuration.Maximum = MusicPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                SongDuration.Value = MusicPlayer.Position.TotalSeconds;
            }
        }

        public static BitmapImage CreateImageFromPath(string filePath)
        {
            if (filePath.Contains("file://"))
            {
                string[] pathSplit = filePath.Split(new string[] { "///" }, StringSplitOptions.None);
                filePath = pathSplit[1];
            }

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(filePath, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        public static BitmapImage CreateImageFromStream(MemoryStream stream)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        //loads data from song metadata
        private Song LoadSongData(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            string songName = Path.GetFileName(Path.GetFileNameWithoutExtension(path));
            if (SongTitleTagGetter(path) != String.Empty)
            {
                songName = SongTitleTagGetter(path);
            }

            string songPath = Path.GetFullPath(path);
            ArtistTagGetter(songPath);

            string artist = artistName;
            TagLib.File file1 = TagLib.File.Create(path);
            BitmapImage bitmap;

            if (file1.Tag.Pictures == null || file1.Tag.Pictures.Length == 0)
            {
                bitmap = CreateImageFromPath("../../Images/songicon.png");
            }
            else
            {
                TagLib.IPicture pic1 = file1.Tag.Pictures[0];
                MemoryStream ms = new MemoryStream(pic1.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                bitmap = CreateImageFromStream(ms);
            }

            return new Song(songName, @songPath, artist, false, bitmap);
        }

        //sets the album art from song metadata
        private void AlbumArtSet(string path)
        {
            TagLib.File file = TagLib.File.Create(path);

            if (file.Tag.Pictures == null || file.Tag.Pictures.Length == 0)
            {
                AlbumArt.ImageSource = CreateImageFromPath("../../Images/songicon.png");
                return;
            }

            TagLib.IPicture pic = file.Tag.Pictures[0];
            MemoryStream ms = new MemoryStream(pic.Data.Data);
            ms.Seek(0, SeekOrigin.Begin);

            AlbumArt.ImageSource = CreateImageFromStream(ms);
        }

        //gets artist name from song medadata
        public void ArtistTagGetter(string path)
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

        //gets song title from song metadata
        public static string SongTitleTagGetter(string path)
        {
            TagLib.File file = TagLib.File.Create(path);
            string songTitle = String.Empty;
            if (file.Tag.Title != null)
            {
                songTitle = file.Tag.Title;
            }
            return songTitle;
        }

        //sets song brush after deserialization
        private void SetSongBrush(Song song)
        {
            TagLib.File file = TagLib.File.Create(song.Path);

            if (file.Tag.Pictures == null || file.Tag.Pictures.Length == 0)
            {
                song.Brush = CreateImageFromPath("../../Images/songicon.png");
                return;
            }

            TagLib.IPicture pic = file.Tag.Pictures[0];
            MemoryStream ms = new MemoryStream(pic.Data.Data);
            ms.Seek(0, SeekOrigin.Begin);

            song.Brush = CreateImageFromStream(ms);
        }

        //artist links 
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (artistName != "No artist data")
            {
                string link = "https://en.wikipedia.org/wiki/";
                link += artistName;
                Process.Start(new ProcessStartInfo(link));
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        private void LoadSongsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog musicFiles = new OpenFileDialog();
            musicFiles.Filter = "MUSIC-Files|*.mp3;*.wav;*.flac";
            musicFiles.Multiselect = true;
            musicFiles.CheckFileExists = true;
            
            if (musicFiles.ShowDialog() == true)
            {
                if(Playlist.Items.Count > 0)
                {
                    Playlist.Items.Clear();
                }    

                foreach (string filename in musicFiles.FileNames)
                {
                    Playlist.Items.Add(LoadSongData(filename));
                    loadedSongsList.Add(LoadSongData(filename));
                }
            }

            songsLoaded = true;
            loadedPlaylist = false;
            Playlist.Items.Refresh();
            CurrentPlaylistText.Text = "Loaded Songs";
        }

        //loads the loaded songs
        private void LoadedSongsButton_Click(object sender, RoutedEventArgs e)
        {
            if (loadedSongsList.Count == 0)
            {
                MessageBox.Show("No loaded songs. Load songs to use this feature.");
                return;
            }

            Playlist.Items.Clear();
            foreach (Song song in loadedSongsList)
            {
                Playlist.Items.Add(song);
            }
            Playlist.Items.Refresh();

            PlaylistsListBox.SelectedItem = null;
            songsLoaded = true;
            loadedPlaylist = false;
            CurrentPlaylistText.Text = "Loaded Songs";
        }

        //load favourites
        private void FavouritesButton_Click(object sender, RoutedEventArgs e)
        {
            if(favourites.Count == 0)
            {
                MessageBox.Show("No favourites have been added.");
                return;
            }

            favListOnDisplay = true;
            if (Playlist.Items.Count > 0)
            {
                Playlist.Items.Clear();
            }
            for (int i = 0; i < favourites.Count; i++)
            {
                Playlist.Items.Add(favourites[i]);
            }

            Playlist.Items.Refresh();
            loadedPlaylist = false;
            songsLoaded = false;
            PlaylistsListBox.SelectedItem = null;
            CurrentPlaylistText.Text = "Favourites";
        }

        //adds a song to favourites
        private void FavouriteButton_Click(object sender, RoutedEventArgs e)
        {
            var favouriteSongsIDs = favourites
                .Select(s => s.ID)
                .ToList();

            if (Playlist.SelectedItem != null)
            {
                selectedSong = (Song)Playlist.SelectedItem;
            }

            if (FavouriteButton.Content == FindResource("HeartOutline"))
            {
                for (int i = 0; i < PlaylistsListBox.Items.Count; i++)
                {
                    for (int k = 0; k < ((Playlist)PlaylistsListBox.Items[i]).SongList.Count; k++)
                    {
                        if (((Playlist)PlaylistsListBox.Items[i]).SongList[k].IsFavourite == false &&
                            ((Playlist)PlaylistsListBox.Items[i]).SongList[k].ID == selectedSong.ID &&
                            !favouriteSongsIDs.Contains(((Playlist)PlaylistsListBox.Items[i]).SongList[k].ID))
                        {
                            ((Playlist)PlaylistsListBox.Items[i]).SongList[k].IsFavourite = true;
                            favourites.Add(((Playlist)PlaylistsListBox.Items[i]).SongList[k]);
                            break;
                        }
                    }
                }

                FavouriteButton.Content = FindResource("HeartFull");
            }
            else
            {
                for (int i = 0; i < PlaylistsListBox.Items.Count; i++)
                {
                    for (int k = 0; k < ((Playlist)PlaylistsListBox.Items[i]).SongList.Count; k++)
                    {
                        if (((Playlist)PlaylistsListBox.Items[i]).SongList[k].IsFavourite == true &&
                            ((Playlist)PlaylistsListBox.Items[i]).SongList[k].ID == selectedSong.ID &&
                            favouriteSongsIDs.Contains(((Playlist)PlaylistsListBox.Items[i]).SongList[k].ID))
                        {
                            ((Playlist)PlaylistsListBox.Items[i]).SongList[k].IsFavourite = false;
                            favourites.Remove(((Playlist)PlaylistsListBox.Items[i]).SongList[k]);
                            break;
                        }
                    }
                }

                FavouriteButton.Content = FindResource("HeartOutline");
            }

            if (favListOnDisplay == true)
            {
                Playlist.Items.Clear();
                for (int i = 0; i < favourites.Count; i++)
                {
                    Playlist.Items.Add(favourites[i]);
                }
                Playlist.Items.Refresh();
            }
        }

        //button to play a song from the current playlist
        private void Playlist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox Playlist = sender as ListBox;
            if (Playlist.SelectedItems.Count > 0)
            {
                selectedSong = (Song)Playlist.SelectedItem;
                
                AlbumArtSet(selectedSong.Path);
                ArtistTagGetter(selectedSong.Path);

                if (artistName != "No artist data")
                {
                    SongArtistName.Text = artistName + " - ";
                }
                else
                {
                    SongArtistName.Text = String.Empty;
                }

                if (SongTitleTagGetter(selectedSong.Path) != String.Empty)
                {
                    SongName.Text = SongTitleTagGetter(selectedSong.Path);
                }
                else
                {
                    if (selectedSong.Title.Contains('.'))
                    {
                        string name = selectedSong.Title.Replace(".", String.Empty).TrimStart().TrimEnd();
                        SongName.Text = name;
                    }
                    else
                    {
                        SongName.Text = Path.GetFileNameWithoutExtension(selectedSong.Title);
                    }
                }

                //music player
                MusicPlayer.LoadedBehavior = MediaState.Manual;
                MusicPlayer.UnloadedBehavior = MediaState.Manual;
                MusicPlayer.Source = new Uri(selectedSong.Path);
                MusicPlayer.Play();

                PlayPauseButton.Content = FindResource("Pause");

                if (selectedSong.IsFavourite == false)
                {
                    FavouriteButton.Content = FindResource("HeartOutline");
                }
                else
                {
                    FavouriteButton.Content = FindResource("HeartFull");
                }
            }
        }

        //song timeline slider
        private void SongDuration_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            sliderDrag = true;
        }

        private void SongDuration_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            sliderDrag = false;
            MusicPlayer.Position = TimeSpan.FromSeconds(SongDuration.Value);
        }

        private void SongDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SongTimer.Text = TimeSpan.FromSeconds(SongDuration.Value).ToString(@"mm\:ss");
        }

        #region MediaControls
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayPauseButton.Content == FindResource("Pause"))
            {
                MusicPlayer.Pause();
                PlayPauseButton.Content = FindResource("Play");
            }
            else
            {
                MusicPlayer.Play();
                PlayPauseButton.Content = FindResource("Pause");
            }
        }

        private void SkipNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentSongIndex == -1)
            {
                currentSongIndex = Playlist.SelectedIndex;
            }
            currentSongIndex++;
            if (currentSongIndex < Playlist.Items.Count)
            {
                string path = ((Song)Playlist.Items[currentSongIndex]).Path;
                SongName.Text = Path.GetFileNameWithoutExtension(((Song)Playlist.Items[currentSongIndex]).Title);

                AlbumArtSet(path);
                MusicPlayer.Source = new Uri(path);
                MusicPlayer.Play();
            }
        }

        private void SkipPreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentSongIndex == -1)
            {
                currentSongIndex = Playlist.SelectedIndex;
            }
            if (currentSongIndex > 0)
            {
                currentSongIndex--;
            }

            if (currentSongIndex < Playlist.Items.Count)
            {
                string path = ((Song)Playlist.Items[currentSongIndex]).Path;
                SongName.Text = Path.GetFileNameWithoutExtension(((Song)Playlist.Items[currentSongIndex]).Title);

                AlbumArtSet(path);
                MusicPlayer.Source = new Uri(path);
                MusicPlayer.Play();
            }
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (RepeatButton.Content == FindResource("Repeat"))
            {
                repeatOn = true;
                RepeatButton.Content = FindResource("Repeat2");
            }
            else
            {
                repeatOn = false;
                RepeatButton.Content = FindResource("Repeat");
            }
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShuffleButton.Content == FindResource("Shuffle"))
            {
                shuffleOn = true;
                ShuffleButton.Content = FindResource("Shuffle2");
            }
            else
            {
                shuffleOn = false;
                ShuffleButton.Content = FindResource("Shuffle");
            }
        }
        
        //volume control
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MusicPlayer.Volume = (double)VolumeSlider.Value;
        }

        //speed control
        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MusicPlayer.SpeedRatio = (double)SpeedSlider.Value;
        }

        //skip through song
        private void SongDuration_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int SliderValue = (int)SongDuration.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
            MusicPlayer.Position = ts;
        }
        #endregion

        //automatic playing of next song in list
        private void MusicPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (currentSongIndex == -1)
            {
                currentSongIndex = Playlist.SelectedIndex;
            }

            if (shuffleOn)
            {
                Random randomNumber = new Random();
                int randomSong = randomNumber.Next(Playlist.Items.Count);
                string path = ((Song)Playlist.Items[randomSong]).Path;
                SongName.Text = Path.GetFileNameWithoutExtension(((Song)Playlist.Items[randomSong]).Title);
                currentSongIndex = randomSong;
                AlbumArtSet(path);
                MusicPlayer.Source = new Uri(path);
                MusicPlayer.Play();
            }
            else if (repeatOn)
            {
                string path = ((Song)Playlist.Items[currentSongIndex]).Path;
                SongName.Text = Path.GetFileNameWithoutExtension(((Song)Playlist.Items[currentSongIndex]).Title);
                AlbumArtSet(path);
                MusicPlayer.Source = new Uri(path);
                MusicPlayer.Play();
            }
            else
            {
                if (currentSongIndex == -1)
                {
                    currentSongIndex = Playlist.SelectedIndex;
                }
                
                currentSongIndex++;
                if (currentSongIndex < Playlist.Items.Count)
                {
                    string path = ((Song)Playlist.Items[currentSongIndex]).Path;
                    SongName.Text = Path.GetFileNameWithoutExtension(((Song)Playlist.Items[currentSongIndex]).Title);

                    AlbumArtSet(path);
                    MusicPlayer.Source = new Uri(path);
                    MusicPlayer.Play();
                }
            }
        }

        #region PlaylistControls
        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (songsLoaded && !loadedPlaylist)
            {
                if (MessageBox.Show("Do you want to create a playlist from the loaded songs?", "Creating Playlist",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var newPlaylist = new Playlist("New Playlist", loadedSongsList.Count, loadedSongsList);

                    playlists.Add(newPlaylist);
                    PlaylistsListBox.Items.Add(newPlaylist);
                    PlaylistsListBox.Items.Refresh();

                    return;
                }
            }

            CreatePlaylistWindow createPlaylistWindow = new CreatePlaylistWindow();
            createPlaylistWindow.ShowDialog();

            if (createPlaylistWindow.DialogResult == true)
            {
                playlists.Add(createPlaylistWindow.newPlaylist);
                PlaylistsListBox.Items.Add(createPlaylistWindow.newPlaylist);
                PlaylistsListBox.Items.Refresh();
            }
        }

        //playlist selection
        private void PlaylistsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(PlaylistsListBox.SelectedItem == null)
            {
                return;
            }

            favListOnDisplay = false;
            Playlist.Items.Clear();
            for (int i = 0; i < ((Playlist)PlaylistsListBox.Items[PlaylistsListBox.SelectedIndex]).SongList.Count; i++)
            {
                Playlist.Items.Add(((Playlist)PlaylistsListBox.Items[PlaylistsListBox.SelectedIndex]).SongList[i]);
            }
            Playlist.Items.Refresh();
            loadedPlaylist = true;
            songsLoaded = false;
            CurrentPlaylistText.Text = "Current Playlist: " + ((Playlist)PlaylistsListBox.SelectedItem).Name;
        }

        //playlist deletion
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(PlaylistsListBox.SelectedIndex == -1 || PlaylistsListBox.SelectedItem == null)
            {
                return;
            }

            var favouriteSongsIDs = favourites
                .Select(s => s.ID)
                .ToList();

            var playlistForDeletionSongIDs = ((Playlist)PlaylistsListBox.SelectedItem).SongList
                .Select(s => s.ID)
                .ToList();

            for (int i = 0; i < ((Playlist)PlaylistsListBox.SelectedItem).SongList.Count; i++)
            {
                if (favouriteSongsIDs.Contains(((Playlist)PlaylistsListBox.SelectedItem).SongList[i].ID))
                {
                    favourites
                        .Remove(favourites
                            .Where(s => s.ID == ((Playlist)PlaylistsListBox.SelectedItem).SongList[i].ID)
                            .SingleOrDefault());
                }
            }

            for (int i = 0; i < Playlist.Items.Count; i++)
            {
                if (playlistForDeletionSongIDs.Contains(((Song)Playlist.Items[i]).ID) && !favListOnDisplay)
                {
                    Playlist.Items.Clear();
                    Playlist.Items.Refresh();
                    CurrentPlaylistText.Text = "Current Playlist";
                    break;
                }
                else if (playlistForDeletionSongIDs.Contains(((Song)Playlist.Items[i]).ID) && favListOnDisplay)
                {
                    Playlist.Items.Clear();
                    for (int k = 0; k < favourites.Count; k++)
                    {
                        Playlist.Items.Add(favourites[k]);
                    }
                    Playlist.Items.Refresh();
                    break;
                }
            }

            playlists.RemoveAt(PlaylistsListBox.SelectedIndex);
            PlaylistsListBox.Items.RemoveAt(PlaylistsListBox.SelectedIndex);
            PlaylistsListBox.Items.Refresh();
        }

        //playlist renaming
        private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            EditPlaylistWindow editPlaylistWindow = new EditPlaylistWindow((Playlist)PlaylistsListBox.SelectedItem);
            editPlaylistWindow.ShowDialog();

            bool isRenamedPlaylistLoaded = false;

            var renamedPlaylistSongsIDs = ((Playlist)PlaylistsListBox.SelectedItem).SongList
                .Select(s => s.ID)
                .ToList();

            for (int i = 0; i < Playlist.Items.Count; i++)
            {
                if(renamedPlaylistSongsIDs.Contains(((Song)Playlist.Items[i]).ID))
                {
                    isRenamedPlaylistLoaded = true;
                    break;
                }
            }

            if (isRenamedPlaylistLoaded && loadedPlaylist)
            {
                CurrentPlaylistText.Text = "Current Playlist: " + ((Playlist)PlaylistsListBox.SelectedItem).Name;
            }

            PlaylistsListBox.Items.Refresh();
        }

        //add songs to playlist
        private void AddSongsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Playlist.Items.Count == 0 || PlaylistsListBox.SelectedItem == null || loadedPlaylist == false)
            {
                MessageBox.Show("Load a playlist.");
                return;
            }

            OpenFileDialog musicFiles = new OpenFileDialog();
            musicFiles.Filter = "MUSIC-Files|*.mp3;*.wav;*.flac";
            musicFiles.Multiselect = true;
            musicFiles.CheckFileExists = true;

            if (musicFiles.ShowDialog() == true)
            {
                foreach (string filename in musicFiles.FileNames)
                {
                    Playlist.Items.Add(LoadSongData(filename));
                    ((Playlist)PlaylistsListBox.SelectedItem).SongList.Add(LoadSongData(filename));
                }
            }

            Playlist.Items.Refresh();
        }
        #endregion

        //draggable window
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SavePlaylists();
            Close();
        }
    }
}