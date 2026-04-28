using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        private readonly List<string> _selections = new List<string>();

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
            var addFilm = new AddFilm(_filmoteka.GetGenres(), _filmoteka.GetYears());
            addFilm.Title = "Přidat film";
            if (addFilm.ShowDialog() == true && addFilm.DialogResult == true)
            {
                _filmoteka.AddFilm(addFilm.FilmName, addFilm.Filename, addFilm.Describtion, addFilm.Genres, addFilm.Year);
                EmptyTextBoxes();
                listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
                listBoxFilmy.Items.Refresh();
                CheckFilmCollectionEmptiness();
            }
        }

        private void BtnEraseFilter_Click(object sender, RoutedEventArgs e)
        {
            EmptyTextBoxes();
            listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
            listBoxFilmy.Items.Refresh();
            foreach (var selection in _selections)
            {
                foreach (ComboBoxItem filter in comboBoxFilterBy.Items)
                {
                    if (filter.Content.Equals(selection))
                    {
                        filter.Foreground = System.Windows.Media.Brushes.Black;
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
            var film = (Film)listBoxFilmy.SelectedItem;
            _path = _settings.Folder;
            try
            {
                var pathToFilm = Path.Combine(_path, film.Filename);
                var res = MessageBox.Show($"Chcete pustit film:\n {film.Name}?", "Otázka", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes && !string.IsNullOrEmpty(pathToFilm))
                {
                    Process.Start(pathToFilm);
                }
            }
            catch (FilmException ex)
            {
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Vyskytla se chyba při spouštění filmu!\n{ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EmptyTextBoxes();
            comboBoxFilterBy.Items.Clear();
            var items = new List<ComboBoxItem>();
            items.Clear();
            switch (comboBoxFilter.SelectedIndex)
            {
                case 0:
                    foreach (int year in _filmoteka.GetYears())
                    {
                        var item = new ComboBoxItem();
                        item.Content = year;
                        items.Add(item);
                    }
                    break;

                case 1:
                    foreach (string genre in _filmoteka.GetGenres())
                    {
                        var item = new ComboBoxItem();
                        item.Content = genre;
                        items.Add(item);
                    }
                    break;
            }

            foreach (var item in items)
            {
                comboBoxFilterBy.Items.Add(item);
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
                return;

            var film = (Film)listBoxFilmy.SelectedItem;
            var editFilm = new AddFilm(_filmoteka.GetGenres(), _filmoteka.GetYears(), film.Name, film.Filename, film.Year, film.Genres.ToArray(), film.Description);
            editFilm.Title = "Upravit film";
            if (editFilm.ShowDialog() == true && editFilm.DialogResult == true)
            {
                _filmoteka.UpdateFilm(film, editFilm.FilmName, editFilm.Filename, editFilm.Describtion, editFilm.Genres, editFilm.Year);
                EmptyTextBoxes();
                listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
                listBoxFilmy.Items.Refresh();
                CheckFilmCollectionEmptiness();
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
                var export = new Export(_filmoteka);
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
            CheckFileEmptines();
            CheckFilmCollectionEmptiness();
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
            var selection = _selections.Find(x => x.Contains(((ComboBoxItem)comboBoxFilterBy.SelectedItem).Content.ToString()));
            if ((_selectionChanged && string.IsNullOrEmpty(selection)) || string.IsNullOrEmpty(selection))
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
            var window = new SettingsDialog(_settings, null, TypeOfStorage.FileOnly);
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
                MessageBox.Show("Prosím, nastavte datové úložiště.\nBez tohoto nebude aplikace fungovat!", String.Empty, MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void CheckIfFileExists(string filename = null)
        {
            var tempFilename = (filename == null) ? _settings.Filename : filename;
            if (string.IsNullOrEmpty(tempFilename) || !File.Exists(tempFilename))
            {
                MessageBox.Show("Soubor neexistuje nebo není nastavena cesta k souboru s daty!", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowSettingsDialogue();
            }
        }

        /// <summary>
        /// Checks if file is empty. If so, then set file menu items.
        /// </summary>
        /// <param name="filename"></param>
        private void CheckFileEmptines(string filename = null)
        {
            filename = filename == null ? _settings.Filename : filename;
            var localfilms = new Filmoteka(filename).GetFilms().Length;
            if (localfilms == 0)
            {
                loadFileMenuItem.IsEnabled = false;
            }
            else
            {
                loadFileMenuItem.IsEnabled = true;
            }
        }

        private void CheckFilmCollectionEmptiness()
        {
            if (_filmoteka.GetFilms().Length == 0)
            {
                editFilmMeniItem.IsEnabled = false;
                removeFilmMeniItem.IsEnabled = false;
                saveFileMenuItem.IsEnabled = false;
            }
            else
            {
                editFilmMeniItem.IsEnabled = true;
                removeFilmMeniItem.IsEnabled = true;
                saveFileMenuItem.IsEnabled = true;
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
                var film = (Film)listBoxFilmy.SelectedItem;
                tbFilm.Text = film.Name;
                tbRok.Text = film.Year.ToString();
                tbPopis.Text = film.Description;
                tbZanr.Text = string.Empty;
                var sb = new StringBuilder();
                foreach (var genre in film.Genres)
                {
                    if (film.Genres.IndexOf(genre) == film.Genres.Count - 1)
                    {
                        sb.Append(genre);
                    }
                    else
                    {
                        sb.Append(genre).Append("\\");
                    }
                }

                tbZanr.Text = sb.ToString();
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

            CheckFileEmptines(filename);

            _filmoteka = new Filmoteka(filename, newFile);
            _filmoteka.FilmsAutoAdded += Filmoteka_FilmsAutoAdded;

            if (string.IsNullOrEmpty(_settings.Folder))
            {
                var window = new OtherOptions();
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

            CheckFilmCollectionEmptiness();
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
            CheckFilmCollectionEmptiness();
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
                var filmToRemove = (Film)listBoxFilmy.SelectedItem;
                var result = MessageBox.Show($"Chcete film {filmToRemove.Name} vymazat?", string.Empty, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _filmoteka.RemoveFilm(filmToRemove);
                    EmptyTextBoxes();
                    listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
                    listBoxFilmy.Items.Refresh();
                    CheckFilmCollectionEmptiness();
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
            var window = new SettingsDialog(_settings, new OtherOptions(), TypeOfStorage.FileOnly);
            window.Filter = "XML File (*.xml)|*.xml|JSON File (*.json)|*.json";
            if (window.ShowDialog() == true)
            {
                Load(window.Filename, window.NewFile);
            }
        }
    }
}
