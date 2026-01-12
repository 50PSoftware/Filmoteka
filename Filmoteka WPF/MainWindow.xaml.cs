using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using _50P.Software.Settings.Dialogs;

namespace Filmoteka_WPF
{
    public partial class MainWindow : Window
    {
        private Filmoteka _filmoteka;
        private string _path;
        private bool _selectionChanged;
        private Settings _settings;
        private List<string> _selections = new List<string>();

        public MainWindow()
        {
            _settings = new Settings();
            if (_settings.FirstTime)
            {
                FirstRun();
            }
            else
            {
                InitializeComponent();
                Load();
            }
        }

        private void AddFilm_Click(object sender, RoutedEventArgs e)
        {
            AddFilm addFilm = new AddFilm(_filmoteka.GetZanry(), _filmoteka.GetRoky());
            addFilm.Title = "Přidat film";
            if (addFilm.ShowDialog() == true && addFilm.DialogResult == true)
            {
                _filmoteka.AddFilm(addFilm.FilmName, addFilm.Filename, addFilm.Describtion, addFilm.Genres, addFilm.Year);
                EmptyTextBoxes();
                listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
                listBoxFilmy.Items.Refresh();
            }
        }

        private void BtnEraseFilter_Click(object sender, RoutedEventArgs e)
        {
            EmptyTextBoxes();
            listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
            listBoxFilmy.Items.Refresh();
            foreach (string v in _selections)
            {
                foreach (ComboBoxItem it in comboBoxFilterBy.Items)
                {
                    if ((string)it.Content == v)
                    {
                        it.Foreground = System.Windows.Media.Brushes.Black;
                    }
                }
            }
            _selections.Clear();
            btnEraseFilter.IsEnabled = false;
            comboBoxFilter.Text = string.Empty;
            comboBoxFilterBy.SelectedItem = null;
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            Film film = (Film)listBoxFilmy.SelectedItem;
            _path = _settings.Folder;
            try
            {
                string pathToFilm = _path + @"\" + film.Filename;
                MessageBoxResult res = MessageBox.Show($"Chcete pustit film:\n {film.Nazev}?", "Otázka", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    Process.Start(pathToFilm);
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
                    foreach (int rok in _filmoteka.GetRoky())
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = rok;
                        items.Add(item);
                    }
                    break;

                case 1:
                    foreach (string zanr in _filmoteka.GetZanry())
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
                    _selectionChanged = true;
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
                AddFilm editFilm = new AddFilm(_filmoteka.GetZanry(), _filmoteka.GetRoky(), film.Nazev, film.Filename, film.Rok, film.Zanr.ToArray(), film.Popis);
                editFilm.Title = "Upravit film";
                if (editFilm.ShowDialog() == true && editFilm.DialogResult == true)
                {
                    _filmoteka.UpdateFilm(film, editFilm.FilmName, editFilm.Filename, editFilm.Describtion, editFilm.Genres, editFilm.Year);
                    EmptyTextBoxes();
                    listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
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
            if (_settings.AllowExport)
            {
                Export export = new Export(_filmoteka);
                switch (_settings.ExportFileExtension)
                {
                    case ".xml":
                        export.ToXML(_settings.ExportFilename);
                        break;

                    case ".json":
                        export.ToJSON(_settings.ExportFilename);
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
                    listBoxFilmy.ItemsSource = _filmoteka.GetFilmsByYear((int)((ComboBoxItem)comboBoxFilterBy.SelectedItem).Content);
                    break;

                case 1:
                    listBoxFilmy.ItemsSource = _filmoteka.GetFilmsByGenre(_selections.ToArray());
                    break;
            }
            btnEraseFilter.IsEnabled = true;
        }

        private void FilterSelectionHandler()
        {
            string vyb = _selections.Find(x => x.Contains(((ComboBoxItem)comboBoxFilterBy.SelectedItem).Content.ToString()));
            if ((_selectionChanged && string.IsNullOrEmpty(vyb)) || string.IsNullOrEmpty(vyb))
            {
                _selections.Add(((ComboBoxItem)comboBoxFilterBy.SelectedItem).Content.ToString());
                ((ComboBoxItem)comboBoxFilterBy.SelectedItem).Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                _selections.Remove(((ComboBoxItem)comboBoxFilterBy.SelectedItem).Content.ToString());
                ((ComboBoxItem)comboBoxFilterBy.SelectedItem).Foreground = System.Windows.Media.Brushes.Black;
            }
            _selectionChanged = false;
        }

        private void FirstRun()
        {
            SettingsDialog window = new SettingsDialog(_settings, null, TypeOfStorage.FileOnly);
            window.Filter = "XML File (*.xml)|*.xml|JSON File (*.json)|*.json";
            if (window.ShowDialog() == true)
            {
                _settings.FirstTime = false;
                _settings.Save();
                InitializeComponent();
                Load(window.Filename, window.NewFile);
            }
            else
            {
                MessageBox.Show("Prosím, nastavte datové úložiště.\nBez toto nebude aplikace fungovat!", String.Empty, MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void CheckIfFileExists(string filename = null)
        {
            string _filename = (filename == null) ? _settings.Filename : filename;
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
                    {
                        tbZanr.Text += z;
                    }
                    else
                    {
                        tbZanr.Text += $"{z} \\ ";
                    }
                }
            }
        }

        private void Load(string filename = null, bool newFile = false)
        {
            Title = "Filmotéka";
            EmptyTextBoxes();
            _settings.Reload();
            if (!newFile) CheckIfFileExists(filename);
            if (string.IsNullOrEmpty(filename))
            {
                filename = _settings.Filename;
            }

            _filmoteka = new Filmoteka(filename, newFile);
            _filmoteka.FilmsAutoAdded += Filmoteka_FilmsAutoAdded;

            if (string.IsNullOrEmpty(_settings.Folder))
            {
                OtherOptions window = new OtherOptions();
                if (window.ShowDialog() == true)
                {
                    _settings.Reload();
                    _filmoteka.Folder = _settings.Folder;
                }
            }
            else
            {
                _filmoteka.Folder = _settings.Folder;
            }

            if (_settings.AutoAdd)
            {
                _filmoteka.AutoAddFilms();
                addFilmMenuItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                addFilmMenuItem.Visibility = Visibility.Visible;
            }

            exportFileMenuItem.Visibility = _settings.AllowExport ? Visibility.Visible : Visibility.Collapsed;
            fileSeparatorMenuItem.Visibility = _settings.AllowExport ? Visibility.Visible : Visibility.Collapsed;

            btnEraseFilter.IsEnabled = false;
            listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            CheckIfFileExists();
            switch (_settings.FileExtension)
            {
                case ".xml":
                    new XMLFile(_settings.Filename).Load(_filmoteka);
                    break;

                case ".json":
                    new JSONFile(_settings.Filename).Load(_filmoteka);
                    break;
            }

            listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
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
                MessageBoxResult result = MessageBox.Show($"Chcete film {filmToRemove.Nazev} vymazat?", string.Empty, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _filmoteka.RemoveFilm(filmToRemove);
                    EmptyTextBoxes();
                    listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
                    listBoxFilmy.Items.Refresh();
                }
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            CheckIfFileExists();
            switch (_settings.FileExtension)
            {
                case ".xml":
                    new XMLFile(_settings.Filename).Save(_filmoteka);
                    break;

                case ".json":
                    new JSONFile(_settings.Filename).Save(_filmoteka);
                    break;
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsDialogue();
        }

        private void ShowSettingsDialogue()
        {
            SettingsDialog window = new SettingsDialog(_settings, new OtherOptions(), TypeOfStorage.FileOnly);
            window.Filter = "XML File (*.xml)|*.xml|JSON File (*.json)|*.json";
            if (window.ShowDialog() == true)
            {
                Load(window.Filename, window.NewFile);
            }
        }
    }
}
