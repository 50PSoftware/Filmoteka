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
using Ookii.Dialogs.Wpf;

namespace Filmoteka_WPF
{
    /// <summary>
    /// Interakční logika pro OtherOptions.xaml
    /// </summary>
    public partial class OtherOptions : Window
    {
        private Settings _settings;
        public OtherOptions()
        {
            InitializeComponent();
            this.WindowStyle = WindowStyle.ToolWindow;
            _settings = new Settings();
            chbAtuoAdd.IsChecked = _settings.AutoAdd;
            chbTryMode.IsChecked = _settings.TryMode;
            chbAllowExport.IsChecked = _settings.AllowExport;
            tbExport.IsEnabled = btnBrowse.IsEnabled = _settings.AllowExport;
            chbLocalMode.IsChecked = _settings.UseDatabase == true ? false : true;
            tbFolder.Text = !string.IsNullOrEmpty(_settings.Folder) ? _settings.Folder : string.Empty;
            tbExport.Text = !string.IsNullOrEmpty(_settings.ExportFilename) ? _settings.ExportFilename : string.Empty;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
            if(folderBrowserDialog.ShowDialog() == true)
            {
                tbFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            _settings.Folder = tbFolder.Text;
            _settings.AutoAdd = chbAtuoAdd.IsChecked == true ? true : false;
            _settings.TryMode = chbTryMode.IsChecked == true ? true : false;
            _settings.AllowExport = chbAllowExport.IsChecked == true ? true : false;
            _settings.ExportFilename = string.IsNullOrEmpty(tbExport.Text) ? null : tbExport.Text;
            _settings.UseDatabase = chbLocalMode.IsChecked == true ? false : _settings.UseDatabase;
            _settings.Save();
            this.Close();
        }

        private void Closing_Window(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Title = "Vybrat soubor k exportu";
            fileDialog.Filter = "XML soubory (*.xml)|*.xml";
            if(fileDialog.ShowDialog() == true)
            {
                tbExport.Text = fileDialog.FileName;
            }
        }

        private void chbAllowExport_CheckedChanged(object sender, RoutedEventArgs e)
        {
            /*if(chbAllowExport.IsChecked == true)
            {
                tbExport.IsEnabled = btnBrowse.IsEnabled = true;
            }
            else
            {
                tbExport.IsEnabled = btnBrowse.IsEnabled = false;
            }*/

            tbExport.IsEnabled = btnBrowse.IsEnabled = chbAllowExport.IsChecked == true ? true : false;
            if(chbAllowExport.IsChecked == false)
            {
                tbExport.Text = String.Empty;
                btnOk.IsEnabled = true;
            }
            else
            {
                btnOk.IsEnabled = false;
            }
        }

        private void chbLocalMode_CheckedChanged(object sender, RoutedEventArgs e)
        {
            
        }

        private void tbExport_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbExport.Text))
                btnOk.IsEnabled = true;
            else
                btnOk.IsEnabled = false;
        }
    }
}
