using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace Filmoteka_WPF
{
    public partial class AddFilm : Window
    {
        private List<string> genres;
        private int[] years;

        public AddFilm(string[] zanry, int[] roky)
        {
            InitializeComponent();
            this.years = roky;
            this.genres = new List<string>();
            this.genres.AddRange(zanry);
        }

        public AddFilm(string[] zanry, int[] roky, string name, string filename, int year, string[] genres, string describtion)
        {
            InitializeComponent();
            this.years = roky;
            this.genres = new List<string>();
            this.genres.AddRange(zanry);
            this.FilmName = name;
            this.Filename = filename;
            this.Year = year;
            this.Genres = genres;
            this.Describtion = describtion;
        }

        public string Describtion { get; private set; }
        public string Filename { get; private set; }
        public string FilmName { get; private set; }
        public string[] Genres { get; private set; }
        public int Year { get; private set; }

        private void BtnAddGenre_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxGenres.SelectedItem != null)
            {
                listBoxSelectedGenres.Items.Add(listBoxGenres.SelectedItem);
                genres.Remove((string)listBoxGenres.SelectedItem);
                genres.Sort();
                listBoxGenres.Items.Refresh();
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = "Procházet...";
            if (openFile.ShowDialog() == true)
            {
                tbFilename.Text = openFile.SafeFileName;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
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
            bool isNullOrEmpty = string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(filmName) || year == 0 || genres.Count == 0;
            if (isNullOrEmpty)
            {
                MessageBox.Show("Potřebná pole nejdou vyplněna!", String.Empty, MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else
            {
                this.Filename = filename;
                this.FilmName = filmName;
                this.Year = year;
                this.Describtion = describtion;
                this.Genres = genres.ToArray();
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnRemoveGenre_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxSelectedGenres.SelectedItem != null && listBoxSelectedGenres.Items.Count > 0)
            {
                genres.Add((string)listBoxSelectedGenres.SelectedItem);
                listBoxSelectedGenres.Items.Remove(listBoxSelectedGenres.SelectedItem);
                genres.Sort();
                listBoxGenres.Items.Refresh();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbYears.ItemsSource = this.years;
            listBoxGenres.ItemsSource = this.genres;
            tbFilename.Text = this.Filename;
            tbNazev.Text = this.FilmName;
            List<int> yrs = new List<int>();
            yrs.AddRange(years);
            cbYears.SelectedIndex = yrs.IndexOf(this.Year);
            if (this.Genres != null)
            {
                foreach (string zanr in this.Genres)
                {
                    listBoxSelectedGenres.Items.Add(zanr);
                    genres.Remove((string)zanr);
                    genres.Sort();
                    listBoxGenres.Items.Refresh();
                }
            }
        }
    }
}
