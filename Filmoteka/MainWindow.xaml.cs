using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
using System.Windows.Navigation;
using _50P.Software.Security.Password;
using _50P.Software.Connect.MySql;

namespace Filmoteka_WPF
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Filmoteka _filmoteka;
        private string _path;
        private Settings _settings;
        public MainWindow()
        {
            InitializeComponent();
            _settings = new Settings();
            if (_settings.FirstTime)
            {
                FirstRun();
            }
            else
            {
                Load();
            }
        }

        private void EmptyTextBoxes()
        {
            tbFilm.Text = tbPopis.Text = tbRok.Text = tbZanr.Text = string.Empty;
        }

        private void Load(string filenameOrConnectionString = null)
        {
            Title = "Filmotéka";
            EmptyTextBoxes();
            _settings.Reload();
            if (string.IsNullOrEmpty(filenameOrConnectionString))
            {
                if (_settings.UseDatabase)
                {
                    ConnectMySQL connectMySQL = new ConnectMySQL(_settings.Server, _settings.Username, SecurePassword.GetUnprotectedPassword(_settings.Password));
                    connectMySQL.setDatabase(_settings.DatabaseName);
                    filenameOrConnectionString = connectMySQL.Connection;
                }
                else
                {
                    filenameOrConnectionString = _settings.Filename;
                }
            }
            _filmoteka = new Filmoteka(filenameOrConnectionString, _settings.UseDatabase);
            _filmoteka.EditedRemotely += _filmoteka_EditedRemotely;
            _filmoteka.FilmsAutoAdded += _filmoteka_FilmsAutoAdded;

            if (string.IsNullOrEmpty(_settings.Folder))
            {
                OtherOptions window = new OtherOptions();
                if (window.ShowDialog() == true && window.DialogResult == true)
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
            fileMenuItem.Visibility = firstSeparatorMenuItem.Visibility = (_settings.AllowExport || !_settings.UseDatabase) && !_settings.TryMode ? Visibility.Visible : Visibility.Collapsed;
            loadFileMenuItem.Visibility = saveFileMenuItem.Visibility = !_settings.UseDatabase ? Visibility.Visible : Visibility.Collapsed;
            fileSeparatorMenuItem.Visibility = _settings.AllowExport && !_settings.UseDatabase ? Visibility.Visible : Visibility.Collapsed;

            btnEraseFilter.IsEnabled = false;
            listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
            
        }

        private void _filmoteka_FilmsAutoAdded(object sender, FilmotekaEventArgs e)
        {
            MessageBox.Show("Byly automaticky přidány filmy!", String.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void _filmoteka_EditedRemotely(object sender, FilmotekaEventArgs e)
        {
            MessageBox.Show("Proběhly změny v databázi!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    listBoxFilmy.ItemsSource = _filmoteka.GetFilmsByGenre(vyber.ToArray());
                    break;
            }
            btnEraseFilter.IsEnabled = true;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Film film = (Film)listBoxFilmy.SelectedItem;
            _path = _settings.Folder;
            try
            {
                string path_to_film = _path + @"\" + film.Filename;
                MessageBoxResult res = MessageBox.Show($"Chcete pustit film:\n {film.Nazev}?", "Otázka", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    Process.Start(path_to_film);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void comboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EmptyTextBoxes();
            comboBoxFilterBy.Items.Clear();
            List<ComboBoxItem> items = new List<ComboBoxItem>();
            items.Clear();
            switch (comboBoxFilter.SelectedIndex)
            {
                case 0:
                    foreach(int rok in _filmoteka.GetRoky())
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

        private void listBoxFilmy_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
        private List<string> vyber = new List<string>();
        private void comboBoxFilterBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void btnEraseFilter_Click(object sender, RoutedEventArgs e)
        {
            EmptyTextBoxes();
            listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
            listBoxFilmy.Items.Refresh();
            foreach(string v in vyber)
            {
                foreach(ComboBoxItem it in comboBoxFilterBy.Items)
                {
                    if(it.Content == v)
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

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            _50P.Software.Settings.Dialogs.MainWindow window = new _50P.Software.Settings.Dialogs.MainWindow(_settings, new OtherOptions());
            window.Filter = "XML File (*.xml)|*.xml";
            if (window.ShowDialog() == true)
            {
                if (window.UseDatabase)
                {
                    ConnectMySQL connect = new ConnectMySQL(window.Server, window.Username, SecurePassword.GetUnprotectedPassword(window.Password));
                    connect.setDatabase(window.DatabaseName);
                    Load(connect.Connection);
                }
                else
                {
                    Load(window.Filename);
                }
            }
        }

        private void AddFilm_Click(object sender, RoutedEventArgs e)
        {
            AddFilm addFilm = new AddFilm(_filmoteka.GetZanry(), _filmoteka.GetRoky());
            addFilm.Title = "Přidat film";
            if(addFilm.ShowDialog() == true && addFilm.DialogResult == true)
            {
                _filmoteka.EditRemotely = !_settings.TryMode;
                _filmoteka.AddFilm(addFilm.FilmName, addFilm.Filename, addFilm.Describtion, addFilm.Genres, addFilm.Year);
                EmptyTextBoxes();
                listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
                listBoxFilmy.Items.Refresh();
            }
        }

        private void comboBoxFilterBy_DropDownClosed(object sender, EventArgs e)
        {
            FilterSelectionHandler();
        }

        private bool selectionChanged;

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
                if(editFilm.ShowDialog() == true && editFilm.DialogResult == true)
                {
                    _filmoteka.EditRemotely = !_settings.TryMode;
                    _filmoteka.UpdateFilm(film, editFilm.FilmName, editFilm.Filename, editFilm.Describtion, editFilm.Genres, editFilm.Year);
                    EmptyTextBoxes();
                    listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
                    listBoxFilmy.Items.Refresh();
                }
            }
        }

        private void RemoveFilm_Click(object sender, RoutedEventArgs e)
        {
            if(listBoxFilmy.SelectedIndex < 0)
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
                    _filmoteka.EditRemotely = !_settings.TryMode;
                    _filmoteka.RemoveFilm(filmToRemove);
                    EmptyTextBoxes();
                    listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
                    listBoxFilmy.Items.Refresh();
                }
            }
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            XMLFile xmlFile = new XMLFile(_settings.Filename);
            xmlFile.Load(_filmoteka);
            listBoxFilmy.ItemsSource = _filmoteka.GetFilms();
            listBoxFilmy.Items.Refresh();
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            XMLFile xmlFile = new XMLFile(_settings.Filename);
            xmlFile.Save(_filmoteka);
        }

        private void ExportFile_Click(object sender, RoutedEventArgs e)
        {
            Export export = new Export(_filmoteka);
            export.ToXML(_settings.ExportFilename);
        }

        private void FirstRun()
        {
            _50P.Software.Settings.Dialogs.MainWindow window = new _50P.Software.Settings.Dialogs.MainWindow(_settings, new OtherOptions());
            window.Filter = "XML File (*.xml)|*.xml";
            window.ShowDialog();
            if (window.DialogResult == true)
            {
                _settings.FirstTime = false;
                _settings.Save();
                if (window.UseDatabase)
                {
                    ConnectMySQL connection = new ConnectMySQL(window.Server, window.Username, SecurePassword.GetUnprotectedPassword(window.Password));
                    connection.setDatabase(window.DatabaseName);
                    Load(connection.Connection);
                }
                else
                {
                    if (window.NewFile)
                    {
                        XMLFile file = new XMLFile(window.Filename);
                        _filmoteka = new Filmoteka(window.Filename, _settings.UseDatabase, true);
                    }
                    Load(window.Filename);
                }
            }
            else
            {
                MessageBox.Show("Prosím, nastavte datové úložiště.\nBez toto nebude aplikace fungovat!", String.Empty, MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }
    }
}
