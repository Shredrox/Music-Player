using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if(NameEditBox.Text == String.Empty)
            {
                MessageBox.Show("Enter a name for the playlist");
                return;
            }

            editedPlaylist.Name = NameEditBox.Text;
            DialogResult = true;
        }
    }
}
