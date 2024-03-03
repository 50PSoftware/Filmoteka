using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Filmoteka_WPF
{
    /// <summary>
    /// Interakční logika pro OtherOptions.xaml
    /// </summary>
    public partial class OtherOptions : Window
    {
        private string extension;
        private Settings settings;

        public OtherOptions()
        {
            InitializeComponent();
            this.WindowStyle = WindowStyle.ToolWindow;
            settings = new Settings();
            chbAtuoAdd.IsChecked = settings.AutoAdd;
            chbAllowExport.IsChecked = settings.AllowExport;
            tbExport.IsEnabled = btnBrowse.IsEnabled = settings.AllowExport;
            tbFolder.Text = !string.IsNullOrEmpty(settings.Folder) ? settings.Folder : string.Empty;
            tbExport.Text = !string.IsNullOrEmpty(settings.ExportFilename) ? settings.ExportFilename : string.Empty;
            extension = !string.IsNullOrEmpty(settings.ExportFileExtension) ? settings.ExportFileExtension : string.Empty;
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Title = "Vybrat soubor k exportu";
            fileDialog.Filter = "XML soubory (*.xml)|*.xml|JSON (*.json)|*.json";
            if (fileDialog.ShowDialog() == true)
            {
                tbExport.Text = fileDialog.FileName;
                System.IO.FileInfo finfo = new System.IO.FileInfo(fileDialog.FileName);
                extension = finfo.Extension;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            settings.Folder = tbFolder.Text;
            settings.AutoAdd = chbAtuoAdd.IsChecked == true ? true : false;
            settings.AllowExport = chbAllowExport.IsChecked == true ? true : false;
            settings.ExportFilename = string.IsNullOrEmpty(tbExport.Text) ? null : tbExport.Text;
            settings.ExportFileExtension = extension;
            settings.Save();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == true)
            {
                tbFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void Closing_Window(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void ChbAllowExport_CheckedChanged(object sender, RoutedEventArgs e)
        {
            tbExport.IsEnabled = btnBrowse.IsEnabled = chbAllowExport.IsChecked == true ? true : false;
            if (chbAllowExport.IsChecked == false)
            {
                tbExport.Text = String.Empty;
                btnOk.IsEnabled = true;
            }
            else
            {
                btnOk.IsEnabled = false;
            }
        }

        private void TbExport_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbExport.Text))
                btnOk.IsEnabled = true;
            else
                btnOk.IsEnabled = false;
        }
    }
}
