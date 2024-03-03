using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Filmoteka_WPF
{
    public partial class MainWindow : Window
    {
        private Filmoteka filmoteka;
        private string path;
        private bool selectionChanged;
        private Settings settings;
        private List<string> vyber = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            settings = new Settings();
            if (settings.FirstTime)
            {
                FirstRun();
            }
            else
            {
                Load();
            }
        }

        private void AddFilm_Click(object sender, RoutedEventArgs e)
        {
            AddFilm addFilm = new AddFilm(filmoteka.GetZanry(), filmoteka.GetRoky());
            addFilm.Title = "Přidat film";
            if (addFilm.ShowDialog() == true && addFilm.DialogResult == true)
            {
                filmoteka.AddFilm(addFilm.FilmName, addFilm.Filename, addFilm.Describtion, addFilm.Genres, addFilm.Year);
                EmptyTextBoxes();
                listBoxFilmy.ItemsSource = filmoteka.GetFilms();
                listBoxFilmy.Items.Refresh();
            }
        }

        private void BtnEraseFilter_Click(object sender, RoutedEventArgs e)
        {
            EmptyTextBoxes();
            listBoxFilmy.ItemsSource = filmoteka.GetFilms();
            listBoxFilmy.Items.Refresh();
            foreach (string v in vyber)
            {
                foreach (ComboBoxItem it in comboBoxFilterBy.Items)
                {
                    if (it.Content == v)
                    {
                        it.Foreground = System.Windows.Media.Brushes.Black;
                    }
                }
            }
            vyber.Clear();
            btnEraseFilter.IsEnabled = false;
            comboBoxFilter.Text = string.Empty;
            comboBoxFilterBy.SelectedItem = null;
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            Film film = (Film)listBoxFilmy.SelectedItem;
            path = settings.Folder;
            try
            {
                string path_to_film = path + @"\" + film.Filename;
                MessageBoxResult res = MessageBox.Show($"Chcete pustit film:\n {film.Nazev}?", "Otázka", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    Process.Start(path_to_film);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EmptyTextBoxes();
            comboBoxFilterBy.Items.Clear();
            List<ComboBoxItem> items = new List<ComboBoxItem>();
            items.Clear();
            switch (comboBoxFilter.SelectedIndex)
            {
                case 0:
                    foreach (int rok in filmoteka.GetRoky())
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = rok;
                        items.Add(item);
                    }
                    break;

                case 1:
                    foreach (string zanr in filmoteka.GetZanry())
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = zanr;
                        items.Add(item);
                    }
                    break;
            }
            foreach (ComboBoxItem it in items)
            {
                comboBoxFilterBy.Items.Add(it);
            }
        }

        private void ComboBoxFilterBy_DropDownClosed(object sender, EventArgs e)
        {
            if (comboBoxFilterBy.SelectedIndex > -1)
            {
                FilterSelectionHandler();
            }
        }

        private void ComboBoxFilterBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EmptyTextBoxes();
            if (comboBoxFilterBy.SelectedIndex < 0)
            {
                return;
            }
            else
            {
                if (comboBoxFilter.SelectedIndex == 1)
                {
                    selectionChanged = true;
                }
            }
        }

        private void EditFilm_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxFilmy.SelectedIndex < 0)
            {
                return;
            }
            else
            {
                Film film = (Film)listBoxFilmy.SelectedItem;
                AddFilm editFilm = new AddFilm(filmoteka.GetZanry(), filmoteka.GetRoky(), film.Nazev, film.Filename, film.Rok, film.Zanr.ToArray(), film.Popis);
                editFilm.Title = "Upravit film";
                if (editFilm.ShowDialog() == true && editFilm.DialogResult == true)
                {
                    filmoteka.UpdateFilm(film, editFilm.FilmName, editFilm.Filename, editFilm.Describtion, editFilm.Genres, editFilm.Year);
                    EmptyTextBoxes();
                    listBoxFilmy.ItemsSource = filmoteka.GetFilms();
                    listBoxFilmy.Items.Refresh();
                }
            }
        }

        private void EmptyTextBoxes()
        {
            tbFilm.Text = tbPopis.Text = tbRok.Text = tbZanr.Text = string.Empty;
        }

        private void ExportFile_Click(object sender, RoutedEventArgs e)
        {
            if (settings.AllowExport)
            {
                Export export = new Export(filmoteka);
                switch (settings.ExportFileExtension)
                {
                    case ".xml":
                        export.ToXML(settings.ExportFilename);
                        break;

                    case ".json":
                        export.ToJSON(settings.ExportFilename);
                        break;
                }
                MessageBox.Show($"Data byla exportována!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Vyskytla se chyba při exportu!", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filmoteka_FilmsAutoAdded(object sender, FilmotekaEventArgs e)
        {
            MessageBox.Show("Byly automaticky přidány filmy!", String.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Filter(object sender, RoutedEventArgs e)
        {
            EmptyTextBoxes();
            switch (comboBoxFilter.SelectedIndex)
            {
                case 0:
                    listBoxFilmy.ItemsSource = filmoteka.GetFilmsByYear((int)((ComboBoxItem)comboBoxFilterBy.SelectedItem).Content);
                    break;

                case 1:
                    listBoxFilmy.ItemsSource = filmoteka.GetFilmsByGenre(vyber.ToArray());
                    break;
            }
            btnEraseFilter.IsEnabled = true;
        }

        private void FilterSelectionHandler()
        {
            string vyb = vyber.Find(x => x.Contains(((ComboBoxItem)comboBoxFilterBy.SelectedItem).Content.ToString()));
            if ((selectionChanged && string.IsNullOrEmpty(vyb)) || string.IsNullOrEmpty(vyb))
            {
                vyber.Add(((ComboBoxItem)comboBoxFilterBy.SelectedItem).Content.ToString());
                ((ComboBoxItem)comboBoxFilterBy.SelectedItem).Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                vyber.Remove(((ComboBoxItem)comboBoxFilterBy.SelectedItem).Content.ToString());
                ((ComboBoxItem)comboBoxFilterBy.SelectedItem).Foreground = System.Windows.Media.Brushes.Black;
            }
            selectionChanged = false;
        }

        private void FirstRun()
        {
            _50P.Software.Settings.Dialogs.SettingsDialog window = new _50P.Software.Settings.Dialogs.SettingsDialog(settings, new OtherOptions(), _50P.Software.Settings.Dialogs.TypeOfStorage.FileOnly);
            window.Filter = "XML File (*.xml)|*.xml|JSON File (*.json)|*.json";
            window.ShowDialog();
            if (window.DialogResult == true)
            {
                settings.FirstTime = false;
                settings.Save();
                Load(window.Filename, window.NewFile);
            }
            else
            {
                MessageBox.Show("Prosím, nastavte datové úložiště.\nBez toto nebude aplikace fungovat!", String.Empty, MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void CheckIfFileExists(string filename = null)
        {
            string _filename = (filename == null) ? settings.Filename : filename;
            if (string.IsNullOrEmpty(_filename) || !File.Exists(_filename))
            {
                MessageBox.Show("Soubor neexistuje nebo není nastavena cesta k souboru s daty!", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowSettingsDialogue();
            }
        }

        private void ListBoxFilmy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EmptyTextBoxes();
            if (listBoxFilmy.SelectedIndex < 0)
            {
                return;
            }
            else
            {
                Film film = (Film)listBoxFilmy.SelectedItem;
                tbFilm.Text = film.Nazev;
                tbRok.Text = film.Rok.ToString();
                tbPopis.Text = film.Popis;
                tbZanr.Text = string.Empty;
                foreach (string z in film.Zanr)
                {
                    if (film.Zanr.IndexOf(z) == film.Zanr.Count - 1)
                        tbZanr.Text += z;
                    else
                        tbZanr.Text += $"{z} \\ ";
                }
            }
        }

        private void Load(string filename = null, bool newFile = false)
        {
            Title = "Filmotéka";
            EmptyTextBoxes();
            settings.Reload();
            if (!newFile)
                CheckIfFileExists(filename);
            if (string.IsNullOrEmpty(filename))
            {
                filename = settings.Filename;
            }
            filmoteka = new Filmoteka(filename, newFile);
            filmoteka.FilmsAutoAdded += Filmoteka_FilmsAutoAdded;

            if (string.IsNullOrEmpty(settings.Folder))
            {
                OtherOptions window = new OtherOptions();
                if (window.ShowDialog() == true && window.DialogResult == true)
                {
                    settings.Reload();
                    filmoteka.Folder = settings.Folder;
                }
            }
            else
            {
                filmoteka.Folder = settings.Folder;
            }
            if (settings.AutoAdd)
            {
                filmoteka.AutoAddFilms();
                addFilmMenuItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                addFilmMenuItem.Visibility = Visibility.Visible;
            }

            exportFileMenuItem.Visibility = settings.AllowExport ? Visibility.Visible : Visibility.Collapsed;
            fileSeparatorMenuItem.Visibility = settings.AllowExport ? Visibility.Visible : Visibility.Collapsed;

            btnEraseFilter.IsEnabled = false;
            listBoxFilmy.ItemsSource = filmoteka.GetFilms();
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            CheckIfFileExists();
            switch (settings.FileExtension)
            {
                case ".xml":
                    new XMLFile(settings.Filename).Load(filmoteka);
                    break;

                case ".json":
                    new JSONFile(settings.Filename).Load(filmoteka);
                    break;
            }
            listBoxFilmy.ItemsSource = filmoteka.GetFilms();
            listBoxFilmy.Items.Refresh();
        }

        private void RemoveFilm_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxFilmy.SelectedIndex < 0)
            {
                MessageBox.Show("Vyberte film, který chcete smazat!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                Film filmToRemove = (Film)listBoxFilmy.SelectedItem;
                MessageBoxResult result = MessageBox.Show($"Chcete film {filmToRemove.Nazev} vymazat?", String.Empty, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    filmoteka.RemoveFilm(filmToRemove);
                    EmptyTextBoxes();
                    listBoxFilmy.ItemsSource = filmoteka.GetFilms();
                    listBoxFilmy.Items.Refresh();
                }
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            CheckIfFileExists();
            switch (settings.FileExtension)
            {
                case ".xml":
                    new XMLFile(settings.Filename).Save(filmoteka);
                    break;

                case ".json":
                    new JSONFile(settings.Filename).Save(filmoteka);
                    break;
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsDialogue();
        }

        private void ShowSettingsDialogue()
        {
            _50P.Software.Settings.Dialogs.SettingsDialog window = new _50P.Software.Settings.Dialogs.SettingsDialog(settings, new OtherOptions(), _50P.Software.Settings.Dialogs.TypeOfStorage.FileOnly);
            window.Filter = "XML File (*.xml)|*.xml|JSON File (*.json)|*.json";
            if (window.ShowDialog() == true)
            {
                Load(window.Filename, window.NewFile);
            }
        }
    }
}
