using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        internal void AssignFrom(SerializedCommand deserialized)
        {
            promptBox.Text = deserialized.Prompt;
            nsamplesBox.Text = deserialized.Samples;
            seedBox.Text = deserialized.Seed;
            niterBox.Text = deserialized.Iter;
            ddimBox.Text = deserialized.DdimSteps;
            scaleBox.Text = deserialized.Scale;
            widthBox.Text = deserialized.Width;
            heightBox.Text = deserialized.Height;
            plmsCheck.IsChecked = deserialized.Plms;
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

            var sb = new StringBuilder();
            sb.Append("--prompt \"").Append(prompt).Append("\" --W ").Append(size.width).Append(" --H ").Append(size.height).Append(" --n_samples ")
              .Append(nsamples).Append(" --n_iter ").Append(niter).Append(" --ddim_steps ").Append(ddim).Append(" --seed ").Append(seed).Append(" --scale ").Append(scale);

            if (!string.IsNullOrEmpty(PersistantPreferencesData.OutDirPath))
            {
                sb.Append(" --outdir ").Append(PersistantPreferencesData.OutDirPath);
            }

            if(plmsCheck.IsChecked == true)
            {
                sb.Append(" --plms");
            }

            var args = sb.Replace("\n", "").Replace("\r", "").ToString();

            consoleBox.AppendText($"[Arguments]: {args}\n");

            if (RunChecks())
            {
                ExternalProcessRunner.Run(args, (workDir) => 
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (showoutputCheck.IsChecked == true)
                        {
                            var outDir = System.IO.Path.Join(workDir, "outputs\\txt2img-samples");
                            if (!string.IsNullOrWhiteSpace(PersistantPreferencesData.OutDirPath))
                                outDir = PersistantPreferencesData.OutDirPath;

                            Process.Start("explorer.exe", outDir);
                        }

                        consoleBox.ScrollToEnd();
                    });
                });
            }
        }

        private bool RunChecks()
        {
            if(string.IsNullOrWhiteSpace(PersistantPreferencesData.AnacondaPath))
            {
                consoleBox.AppendText("<run check> Error: Anaconda path not set. Please, use File->Preferences->Anaconda Installation to setup correct path.\n");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PersistantPreferencesData.Txt2ImgPath))
            {
                consoleBox.AppendText("<run check> Error: Txt2Img path not set. Please, use File->Preferences->Txt2Img path to setup correct path.\n");
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

        private void saveMenu_Click(object sender, RoutedEventArgs e)
        {
            // save current prompt and args
            var dialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog()
            {
                AddExtension = true,
                Filter = "sdc|*.sdc",
                OverwritePrompt = true,
                DefaultExt = ".sdc"
            };
            dialog.ShowDialog();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                var path = dialog.FileName;
                var serializedCmd = SerializedCommand.Generate(this);
                serializedCmd.Save(path);
            }
        }

        private void loadMenu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog()
            {
                AddExtension = true,
                Filter = "sdc|*.sdc",
                DefaultExt = ".sdc"
            };
            dialog.ShowDialog();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                var path = dialog.FileName;
                var deserializedCmd = SerializedCommand.Load(path);
                AssignFrom(deserializedCmd);
            }
        }
    }
}
