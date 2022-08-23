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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StableDiffusionGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private bool _isPromptReset = false;
        private TextWriter? _writer = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Initialized(object sender, EventArgs e)
        {
            promptBox.Text = "Enter your prompt...";
            _isPromptReset = false;

            // randomize seed on start
            var rnd = new Random();
            seedBox.Text = rnd.Next(0, int.MaxValue).ToString();

            _writer = new ControlWriter(consoleBox);
            Console.SetOut(_writer);

            // load preferences
            PersistantPreferencesData.Load();
        }

        private void promptBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton.HasFlag(MouseButtonState.Pressed) && !_isPromptReset)
            {
                _isPromptReset = true;
                promptBox.Text = "";
            }
        }

        private void generateBtn_Click(object sender, RoutedEventArgs e)
        {
            var prompt = promptBox.Text;
            var nsamples = nsamplesBox.Text;
            var seed = seedBox.Text;
            var niter = niterBox.Text;
            var ddim = ddimBox.Text;
            var scale = scaleBox.Text;
            (string width, string height) size = (widthBox.Text, heightBox.Text);

            //--H 512 --W 512 --n_iter 6 --plms --n_samples 1 --seed 87201
            string send = "";
            
            if(string.IsNullOrEmpty(PersistantPreferencesData.OutDirPath))
                send = $"--prompt \"{prompt}\" --W {size.width} --H {size.height} --n_samples " +
                    $"{nsamples} --n_iter {niter} --ddim_steps {ddim} --seed {seed} --scale {scale} --plms";
            else
                send = $"--prompt \"{prompt}\" --W {size.width} --H {size.height} --outdir {PersistantPreferencesData.OutDirPath} --n_samples " +
                    $"{nsamples} --n_iter {niter} --ddim_steps {ddim} --seed {seed} --scale {scale} --plms";

            send = send.Replace("\n", "").Replace("\r", "");

            if (RunChecks())
            {
                ExternalProcessRunner.Run(send, () => 
                {
                });
            }
        }

        private bool RunChecks()
        {
            if(string.IsNullOrWhiteSpace(PersistantPreferencesData.AnacondaPath))
            {
                consoleBox.AppendText("<run check> Error: Anaconda path not set. Please, use File->Preferences->Anaconda Installation to setup correct path.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PersistantPreferencesData.Txt2ImgPath))
            {
                consoleBox.AppendText("<run check> Error: Txt2Img path not set. Please, use File->Preferences->Txt2Img path to setup correct path.");
                return false;
            }

            return true;
        }

        private void randomizeSeedBtn_Click(object sender, RoutedEventArgs e)
        {
            var rnd = new Random();
            seedBox.Text = rnd.Next(0, int.MaxValue).ToString();
        }

        private void promptBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(promptBox.Text))
            {
                promptBox.Text = "Enter your prompt...";
                _isPromptReset = false;
            }
        }

        private void outdirBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog()
            {
                SelectedPath = String.IsNullOrWhiteSpace(PersistantPreferencesData.OutDirPath) ? "" : PersistantPreferencesData.OutDirPath,
                Multiselect = false,
            };
            dialog.ShowDialog();

            var path = dialog.SelectedPath;

            if(string.IsNullOrWhiteSpace(path))
            {
                PersistantPreferencesData.OutDirPath = "";
            }
            else
            {
                PersistantPreferencesData.OutDirPath = path;
            }

            PersistantPreferencesData.Save();
        }

        private void preferencesMenu_Click(object sender, RoutedEventArgs e)
        {
            var page = new Preferences();
            page.Show();
        }
    }
}
