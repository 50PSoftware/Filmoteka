using Microsoft.Win32;
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

namespace Filmoteka_WPF
{
    /// <summary>
    /// Interakční logika pro AddFilm.xaml
    /// </summary>
    public partial class AddFilm : Window
    {
        private List<string> _genres;
        private int[] _years;

        public string FilmName { get; private set; }
        public string Filename { get; private set; }
        public string[] Genres { get; private set; }
        public int Year{ get; private set; }
        public string Describtion { get; private set; }
        public AddFilm(string[] zanry, int[] roky)
        {
            InitializeComponent();
            this._years = roky;
            this._genres = new List<string>();
            this._genres.AddRange(zanry);
        }

        public AddFilm(string[] zanry, int[] roky, string name, string filename, int year, string[] genres, string describtion)
        {
            InitializeComponent();
            this._years = roky;
            this._genres = new List<string>();
            this._genres.AddRange(zanry);
            this.FilmName = name;
            this.Filename = filename;
            this.Year = year;
            this.Genres = genres;
            this.Describtion = describtion;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = "Procházet...";
            if (openFile.ShowDialog() == true)
            {
                tbFilename.Text = openFile.SafeFileName;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbYears.ItemsSource = this._years;
            listBoxGenres.ItemsSource = this._genres;
            tbFilename.Text = this.Filename;
            tbNazev.Text = this.FilmName;
            List<int> yrs = new List<int>();
            yrs.AddRange(_years);
            cbYears.SelectedIndex = yrs.IndexOf(this.Year);
            if (this.Genres != null)
            {
                foreach (string zanr in this.Genres)
                {
                    listBoxSelectedGenres.Items.Add(zanr);
                    _genres.Remove((string)zanr);
                    _genres.Sort();
                    listBoxGenres.Items.Refresh();
                }
            }
        }

        private void btnAddGenre_Click(object sender, RoutedEventArgs e)
        {
            if(listBoxGenres.SelectedItem != null)
            {
                listBoxSelectedGenres.Items.Add(listBoxGenres.SelectedItem);
                _genres.Remove((string)listBoxGenres.SelectedItem);
                _genres.Sort();
                listBoxGenres.Items.Refresh();
            }
        }

        private void btnRemoveGenre_Click(object sender, RoutedEventArgs e)
        {
            if(listBoxSelectedGenres.SelectedItem != null && listBoxSelectedGenres.Items.Count > 0)
            {
                _genres.Add((string)listBoxSelectedGenres.SelectedItem);
                listBoxSelectedGenres.Items.Remove(listBoxSelectedGenres.SelectedItem);
                _genres.Sort();
                listBoxGenres.Items.Refresh();
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            string filmName = tbNazev.Text;
            string filename = tbFilename.Text;
            int year = cbYears.SelectedItem != null ? (int)cbYears.SelectedItem : 0;
            string describtion = new TextRange(rtbContent.Document.ContentStart, rtbContent.Document.ContentEnd).Text;
            List<string> genres = new List<string>();
            foreach (string item in listBoxSelectedGenres.Items)
            {
                genres.Add(item);
            }
            this.Filename = filename;
            this.FilmName = filmName;
            this.Year = year;
            this.Describtion = describtion;
            this.Genres = genres.ToArray();
            this.DialogResult = true;
            this.Close();
        }
    }
}
