﻿using Microsoft.Win32;
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

namespace MusicPlayerProject
{
    public partial class MainWindow : Window
    {
        //global variables
        private List<Song> songs = new List<Song>();
        private List<Playlist> list = new List<Playlist>();
        private List<Song> favourites = new List<Song>();
        private bool sliderDrag = false;
        private int currentSongIndex = -1;
        private bool shuffleOn = false;
        private bool repeatOn = false;
        private bool favListOnDisplay = false;
        private string artistName = string.Empty;
        private int counter = 0;
        private int lineCounter = 0;
        private string path = @"C:\Users\User\source\repos\MusicPlayerProject\MusicPlayerProject\Data\SavedPlaylists.txt";

        public MainWindow()
        {
            InitializeComponent();
            LoadPlaylists();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        //draggable window
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        //close button
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SavePlaylists();
            Close();
        }

        //loads data from song metadata
        private void LoadSongData(string path)
        {
            if (File.Exists(path))
            {
                string songName;
                if (SongTitleTagGetter(path) != String.Empty)
                {
                    songName = SongTitleTagGetter(path);
                }
                else
                {
                    songName = Path.GetFileName(Path.GetFileNameWithoutExtension(path));
                }

                string songPath = Path.GetFullPath(path);
                ArtistTagGetter(songPath);
                string artist = artistName;
                TagLib.File file1 = TagLib.File.Create(path);

                if (file1.Tag.Pictures == null || file1.Tag.Pictures.Length == 0)
                {
                    BitmapImage bitI = new BitmapImage();
                    bitI.BeginInit();
                    bitI.UriSource = new Uri("../../Images/songicon.png", UriKind.Relative);
                    bitI.EndInit();

                    if(lineCounter == 0)
                    {
                        favourites.Add(new Song() { Brush = bitI, SongTitle = songName, SongPath = @songPath, SongArtist = artist, IsFavourite = true });
                    }
                    else
                    {
                        songs.Add(new Song() { Brush = bitI, SongTitle = songName, SongPath = @songPath, SongArtist = artist, IsFavourite = false });
                    }
                }
                else
                {
                    TagLib.IPicture pic1 = file1.Tag.Pictures[0];
                    MemoryStream ms1 = new MemoryStream(pic1.Data.Data);
                    ms1.Seek(0, SeekOrigin.Begin);

                    BitmapImage bitmap1 = new BitmapImage();
                    bitmap1.BeginInit();
                    bitmap1.StreamSource = ms1;
                    bitmap1.EndInit();

                    if (lineCounter == 0)
                    {
                        favourites.Add(new Song() { Brush = bitmap1, SongTitle = songName, SongPath = @songPath, SongArtist = artist, IsFavourite = true });
                    }
                    else
                    {
                        songs.Add(new Song() { Brush = bitmap1, SongTitle = songName, SongPath = @songPath, SongArtist = artist, IsFavourite = false });
                    }
                }
            }
        }

        //sets the album art from song metadata
        private void AlbumArtSet(string path)
        {
            TagLib.File file = TagLib.File.Create(path);
            BitmapImage bitmap = new BitmapImage();

            if (file.Tag.Pictures == null || file.Tag.Pictures.Length == 0)
            {
                AlbumArt.Source = new BitmapImage(new Uri("../../Images/songicon.png", UriKind.Relative));
            }
            else
            {
                TagLib.IPicture pic = file.Tag.Pictures[0];
                MemoryStream ms = new MemoryStream(pic.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                AlbumArt.Source = bitmap;
            }
        }

        //gets artist name from song medadata
        private void ArtistTagGetter(string path)
        {
            TagLib.File file = TagLib.File.Create(path);
            if(file.Tag.AlbumArtists.Length != 0)
            {
                artistName = file.Tag.AlbumArtists[0];
            }
            else if(file.Tag.Performers.Length != 0)
            {
                artistName = file.Tag.Performers[0];
            }
            else
            {
                artistName = "No artist data";
            }
        }

        //gets song title from song metadata
        private string SongTitleTagGetter(string path)
        {
            TagLib.File file = TagLib.File.Create(path);
            string songTitle = String.Empty;
            if(file.Tag.Title != null)
            {
                songTitle = file.Tag.Title;
            }
            return songTitle;
        }

        //loads playlists from file
        private void LoadPlaylists()
        {
            string[] fileLines = File.ReadAllLines(path);
            
            for (int i = 0; i < fileLines.Length; i++)
            {
                int k = 1;
                if (i == 0)
                {
                    k = 0;
                }
                
                var lineContent = fileLines[i].Split('|');

                while(lineContent[k] != "")
                {
                    LoadSongData(lineContent[k]);
                    k++;
                }
                lineCounter++;
                if (i != 0)
                {
                    counter++;

                    list.Add(new Playlist() { PlaylistName = "Playlist " + counter, SongCount = songs.Count, SongList = songs });
                    PlaylistsMenu.Items.Add(list[counter - 1]);
                    PlaylistsMenu.SelectedValuePath = list[counter - 1].PlaylistName;
                    songs = new List<Song>();
                }
            }
        }

        //saves playlists to file
        private void SavePlaylists()
        {
            using (StreamWriter file = File.CreateText(path))
            {
                for (int i = 0; i < favourites.Count; i++)
                {
                    file.Write(favourites[i].SongPath + "|");
                }
                file.WriteLine();

                for (int i = 0; i < PlaylistsMenu.Items.Count; i++)
                {
                    file.Write(PlaylistsMenu.Items[i].ToString() + "|");
                    for (int k = 0; k < ((Playlist)PlaylistsMenu.Items[i]).SongList.Count; k++)
                    {
                        file.Write(((Playlist)PlaylistsMenu.Items[i]).SongList[k].SongPath + "|");
                    }
                    file.WriteLine();
                }
                file.Flush();
            }
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

        //button to load songs
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
                    LoadSongData(filename);
                }
                counter++;
            }
            
            list.Add(new Playlist() { PlaylistName = "Playlist " + counter, SongCount = songs.Count, SongList = songs });
            PlaylistsMenu.Items.Add(list[counter - 1]);
            PlaylistsMenu.SelectedValuePath = list[counter - 1].PlaylistName;

            for (int i = 0; i < songs.Count; i++)
            {
                Playlist.Items.Add(songs[i]);
            }
            
            songs = new List<Song>();
            Playlist.Items.Refresh();
        }

        //button to play a song from the current playlist
        private void Playlist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox Playlist = sender as ListBox;
            if (Playlist.SelectedItems.Count > 0)
            {
                Song selectedSong = (Song)Playlist.SelectedItems[0];

                AlbumArtSet(selectedSong.SongPath);
                ArtistTagGetter(selectedSong.SongPath);

                if (artistName != "No artist data")
                {
                    SongArtistName.Text = artistName + " - ";
                }
                else
                {
                    SongArtistName.Text = String.Empty;
                }

                if (SongTitleTagGetter(selectedSong.SongPath) != String.Empty)
                {
                    SongName.Text = SongTitleTagGetter(selectedSong.SongPath);
                }
                else
                {
                    if (selectedSong.SongTitle.Contains('.'))
                    {
                        string name = selectedSong.SongTitle.Replace(".", String.Empty).TrimStart().TrimEnd();
                        SongName.Text = name;
                    }
                    else
                    {
                        SongName.Text = Path.GetFileNameWithoutExtension(selectedSong.SongTitle);
                    }
                }

                //music player
                MusicPlayer.LoadedBehavior = MediaState.Manual;
                MusicPlayer.UnloadedBehavior = MediaState.Manual;
                MusicPlayer.Source = new Uri(selectedSong.SongPath);
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

        //playlists menu 
        private void PlaylistsMenu_DropDownOpened(object sender, EventArgs e)
        {
            if (PlaylistsMenu.SelectedIndex != -1)
            {
                PlaylistsMenu.Text = ((Playlist)PlaylistsMenu.Items[PlaylistsMenu.SelectedIndex]).PlaylistName;
            }
        }

        //playlists menu select item event change
        private void PlaylistsMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            favListOnDisplay = false;
            if (PlaylistsMenu.SelectedItem != null)
            {
                Playlist.Items.Clear();
                for (int i = 0; i < ((Playlist)PlaylistsMenu.Items[PlaylistsMenu.SelectedIndex]).SongList.Count; i++)
                {
                    Playlist.Items.Add(((Playlist)PlaylistsMenu.Items[PlaylistsMenu.SelectedIndex]).SongList[i]);
                }
                Playlist.Items.Refresh();
            }
        }

        //adds a song to favourites
        private void FavouriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (FavouriteButton.Content == FindResource("HeartOutline"))
            {
                for (int i = 0; i < PlaylistsMenu.Items.Count; i++)
                {
                    for (int k = 0; k < ((Playlist)PlaylistsMenu.Items[i]).SongList.Count; k++)
                    {
                        if (((Playlist)PlaylistsMenu.Items[i]).SongList[k].IsFavourite == false && ((Playlist)PlaylistsMenu.Items[i]).SongList[k].SongTitle.Contains(SongName.Text))
                        {
                            ((Playlist)PlaylistsMenu.Items[i]).SongList[k].IsFavourite = true;
                        }
                        if (((Playlist)PlaylistsMenu.Items[i]).SongList[k].IsFavourite == true && !favourites.Contains(((Playlist)PlaylistsMenu.Items[i]).SongList[k]))
                        {
                            favourites.Add(((Playlist)PlaylistsMenu.Items[i]).SongList[k]);
                        }
                    }
                }

                if(favListOnDisplay == true)
                {
                    Playlist.Items.Clear();
                    for (int i = 0; i < favourites.Count; i++)
                    {
                        Playlist.Items.Add(favourites[i]);
                    }
                    Playlist.Items.Refresh();
                }
                
                FavouriteButton.Content = FindResource("HeartFull");
            }
            else
            {
                for (int i = 0; i < PlaylistsMenu.Items.Count; i++)
                {
                    for (int k = 0; k < ((Playlist)PlaylistsMenu.Items[i]).SongList.Count; k++)
                    {
                        if (((Playlist)PlaylistsMenu.Items[i]).SongList[k].IsFavourite == true && ((Playlist)PlaylistsMenu.Items[i]).SongList[k].SongTitle.Contains(SongName.Text))
                        {
                            ((Playlist)PlaylistsMenu.Items[i]).SongList[k].IsFavourite = false;
                        }
                        if (((Playlist)PlaylistsMenu.Items[i]).SongList[k].IsFavourite == false && favourites.Contains(((Playlist)PlaylistsMenu.Items[i]).SongList[k]))
                        {
                            favourites.Remove(((Playlist)PlaylistsMenu.Items[i]).SongList[k]);
                        }
                    }
                }

                for (int i = 0; i < favourites.Count; i++)
                {
                    if (favourites[i].IsFavourite == true && favourites[i].SongTitle.Contains(SongName.Text))
                    {
                        favourites.Remove(favourites[i]);
                    }
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

                FavouriteButton.Content = FindResource("HeartOutline");
            }
        }

        //load favourites
        private void FavouritesButton_Click(object sender, RoutedEventArgs e)
        {
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

        //media controls//

        //skip to next song
        private void SkipNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentSongIndex == -1)
            {
                currentSongIndex = Playlist.SelectedIndex;
            }
            currentSongIndex++;
            if (currentSongIndex < Playlist.Items.Count)
            {
                string path = ((Song)Playlist.Items[currentSongIndex]).SongPath;
                SongName.Text = Path.GetFileNameWithoutExtension(((Song)Playlist.Items[currentSongIndex]).SongTitle);

                AlbumArtSet(path);
                MusicPlayer.Source = new Uri(path);
                MusicPlayer.Play();
            }
            else
            {
                // last song in listbox has been played
            }
        }

        //pause/play
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

        //skip to previous song
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
                string path = ((Song)Playlist.Items[currentSongIndex]).SongPath;
                SongName.Text = Path.GetFileNameWithoutExtension(((Song)Playlist.Items[currentSongIndex]).SongTitle);

                AlbumArtSet(path);
                MusicPlayer.Source = new Uri(path);
                MusicPlayer.Play();
            }
        }

        //repeat button
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

        //shuffle button
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
                string path = ((Song)Playlist.Items[randomSong]).SongPath;
                SongName.Text = Path.GetFileNameWithoutExtension(((Song)Playlist.Items[randomSong]).SongTitle);
                currentSongIndex = randomSong;
                AlbumArtSet(path);
                MusicPlayer.Source = new Uri(path);
                MusicPlayer.Play();
            }
            else if (repeatOn)
            {
                string path = ((Song)Playlist.Items[currentSongIndex]).SongPath;
                SongName.Text = Path.GetFileNameWithoutExtension(((Song)Playlist.Items[currentSongIndex]).SongTitle);
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
                    string path = ((Song)Playlist.Items[currentSongIndex]).SongPath;
                    SongName.Text = Path.GetFileNameWithoutExtension(((Song)Playlist.Items[currentSongIndex]).SongTitle);

                    AlbumArtSet(path);
                    MusicPlayer.Source = new Uri(path);
                    MusicPlayer.Play();
                }
            }
        }
    }
}