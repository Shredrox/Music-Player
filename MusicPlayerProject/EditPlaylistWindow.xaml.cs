using System;
using System.Windows;

namespace MusicPlayerProject
{
    public partial class EditPlaylistWindow : Window
    {
        private Playlist editedPlaylist;

        public EditPlaylistWindow(Playlist playlist)
        {
            InitializeComponent();
            NameEditBox.Text = playlist.Name;

            editedPlaylist = playlist;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameEditBox.Text == String.Empty)
            {
                MessageBox.Show("Enter a name for the playlist");
                return;
            }

            editedPlaylist.Name = NameEditBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
