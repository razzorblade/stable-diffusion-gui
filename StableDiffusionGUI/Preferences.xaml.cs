using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace StableDiffusionGUI
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : MetroWindow
    {
        public Preferences()
        {
            InitializeComponent();
        }

        private void MetroWindow_Initialized(object sender, EventArgs e)
        {
            // update texts
            if (!string.IsNullOrWhiteSpace(PersistentPreferencesData.AnacondaPath))
            {
                anacondaOkText.Text = "OK";
                anacondaOkText.Foreground = new SolidColorBrush(Colors.Green);
            }

            if (!string.IsNullOrWhiteSpace(PersistentPreferencesData.Txt2ImgPath))
            {
                txt2imgOkText.Text = "OK";
                txt2imgOkText.Foreground = new SolidColorBrush(Colors.Green);
            }

            if (!string.IsNullOrWhiteSpace(PersistentPreferencesData.Img2ImgPath))
            {
                img2imgOkText.Text = "OK";
                img2imgOkText.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        private void selectAnacondaBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog()
            {
                SelectedPath = String.IsNullOrWhiteSpace(PersistentPreferencesData.AnacondaPath) ? Directory.GetCurrentDirectory() : PersistentPreferencesData.AnacondaPath,
                Multiselect = false,
                Description = "Select Anaconda installation dir"
            };
            dialog.ShowDialog();

            var result = dialog.SelectedPath;

            var info = new DirectoryInfo(result);
            if(info.GetDirectories().Any(x => x.Name.Contains("bin")))
            {
                PersistentPreferencesData.AnacondaPath = result;
                anacondaOkText.Text = "OK";
                anacondaOkText.Foreground = new SolidColorBrush(Colors.Green);

            }
            else
            {
                PersistentPreferencesData.AnacondaPath = "";
                anacondaOkText.Text = "Error";
                anacondaOkText.Foreground = new SolidColorBrush(Colors.Red);

            }

            PersistentPreferencesData.Save();
        }

        private void selectTxtimgBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog()
            {
                CheckPathExists = true,
                Multiselect = false,
                DefaultExt = "py",
            };
            dialog.ShowDialog();

            var result = dialog.FileName;
            if (!string.IsNullOrEmpty(result))
            {
                PersistentPreferencesData.Txt2ImgPath = result;
                txt2imgOkText.Text = "OK";
                txt2imgOkText.Foreground = new SolidColorBrush(Colors.Green);

            }
            else
            {
                PersistentPreferencesData.Txt2ImgPath = "";
                txt2imgOkText.Text = "Error";
                txt2imgOkText.Foreground = new SolidColorBrush(Colors.Red);

            }

            PersistentPreferencesData.Save();
        }

        private void selectImg2ImgBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog()
            {
                CheckPathExists = true,
                Multiselect = false,
                DefaultExt = "py",
            };
            dialog.ShowDialog();

            var result = dialog.FileName;
            if (!string.IsNullOrEmpty(result))
            {
                PersistentPreferencesData.Img2ImgPath = result;
                img2imgOkText.Text = "OK";
                img2imgOkText.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                PersistentPreferencesData.Img2ImgPath = "";
                img2imgOkText.Text = "Error";
                img2imgOkText.Foreground = new SolidColorBrush(Colors.Red);
            }

            PersistentPreferencesData.Save();
        }
    }
}
