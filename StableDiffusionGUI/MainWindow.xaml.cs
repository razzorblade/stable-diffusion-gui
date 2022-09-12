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
        private bool _isGenerationRunning = false;
        private TextWriter? _writer = null;

        private int currentIteration, maxIterations;

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

            _writer = new ControlWriter(consoleBox, this.Dispatcher);
            Console.SetOut(_writer);
            Console.SetError(_writer);

            // load preferences
            PersistentPreferencesData.Load();
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

        private bool Img2ImgGenerate()
        {
            var prompt = promptBox.Text;
            var nsamples = nsamplesBox.Text;
            var seed = seedBox.Text;
            var niter = niterBox.Text;
            var ddim = ddimBox.Text;
            var scale = scaleBox.Text;
            var strength = strengthBox.Text;

            currentIteration = 1;
            maxIterations = int.Parse(niter);
            progressIterationText.Text = $"0/{maxIterations}";

            var sb = new StringBuilder();
            sb.Append("--prompt \"").Append(prompt).Append("\" --n_samples ").Append(nsamples).Append(" --n_iter ").Append(niter)
              .Append(" --ddim_steps ").Append(ddim).Append(" --seed ").Append(seed).Append(" --scale ").Append(scale).Append(" --strength ")
              .Append(strength).Append(" --init-img \"").Append(inputImage).Append('\"');

            if (!string.IsNullOrEmpty(PersistentPreferencesData.OutDirPath))
            {
                sb.Append(" --outdir \"").Append(PersistentPreferencesData.OutDirPath).Append('\"');
            }

            if (plmsCheck.IsChecked == true)
            {
                //sb.Append(" --plms");
                // NOT YET SUPPORTED
            }

            if (fixedCodeCheck.IsChecked == true)
            {
                sb.Append(" --fixed_code");
            }

            var args = sb.Replace("\n", "").Replace("\r", "").ToString();

            consoleBox.AppendText($"[Arguments]: {args}\n");

            var iterationCount = false;
            if (RunChecks(true))
            {
                ExternalProcessRunner.Run(PersistentPreferencesData.Img2ImgPath, args, (workDir) =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (showoutputCheck.IsChecked == true)
                        {
                            var outDir = System.IO.Path.Join(workDir, "outputs\\txt2img-samples");
                            if (!string.IsNullOrWhiteSpace(PersistentPreferencesData.OutDirPath))
                                outDir = PersistentPreferencesData.OutDirPath;

                            Process.Start(new ProcessStartInfo() { FileName = outDir, UseShellExecute = true });
                        }

                        consoleBox.ScrollToEnd();

                        _isGenerationRunning = false;
                        generateBtn.Content = "Generate";
                    });
                },

                percentage =>
                {
                    this.Dispatcher?.Invoke(() =>
                    {
                        if (percentage == 0)
                            iterationCount = false;

                        if (generatingProgressBar.Value == 100)
                        {
                            generatingProgressBar.Value = 0;
                            progressIterationText.Text = $"{currentIteration}/{maxIterations}";

                            currentIteration++;
                            iterationCount = true;
                        }

                        if (!iterationCount)
                            generatingProgressBar.Value = percentage;
                    });
                },
                true);

                return true;
            }

            return false;
        }

        private bool Txt2ImgGenerate()
        {
            var prompt = promptBox.Text;
            var nsamples = nsamplesBox.Text;
            var seed = seedBox.Text;
            var niter = niterBox.Text;
            var ddim = ddimBox.Text;
            var scale = scaleBox.Text;
            (string width, string height) size = (widthBox.Text, heightBox.Text);

            currentIteration = 1;
            maxIterations = int.Parse(niter);
            progressIterationText.Text = $"0/{maxIterations}";

            var sb = new StringBuilder();
            sb.Append("--prompt \"").Append(prompt).Append("\" --W ").Append(size.width).Append(" --H ").Append(size.height).Append(" --n_samples ")
              .Append(nsamples).Append(" --n_iter ").Append(niter).Append(" --ddim_steps ").Append(ddim).Append(" --seed ").Append(seed).Append(" --scale ").Append(scale);

            if (!string.IsNullOrEmpty(PersistentPreferencesData.OutDirPath))
            {
                sb.Append(" --outdir \"").Append(PersistentPreferencesData.OutDirPath).Append('\"');
            }

            if (plmsCheck.IsChecked == true)
            {
                sb.Append(" --plms");
            }

            var args = sb.Replace("\n", "").Replace("\r", "").ToString();

            consoleBox.AppendText($"[Arguments]: {args}\n");

            var iterationCount = false;

            if (RunChecks(false))
            {
                ExternalProcessRunner.Run(PersistentPreferencesData.Txt2ImgPath, args, (workDir) =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (showoutputCheck.IsChecked == true)
                        {
                            var outDir = System.IO.Path.Join(workDir, "outputs\\txt2img-samples");
                            if (!string.IsNullOrWhiteSpace(PersistentPreferencesData.OutDirPath))
                                outDir = PersistentPreferencesData.OutDirPath;

                            Process.Start(new ProcessStartInfo() { FileName = outDir, UseShellExecute = true});
                        }

                        consoleBox.ScrollToEnd();

                        _isGenerationRunning = false;
                        generateBtn.Content = "Generate";
                    });
                },
                
                //(percentage) => generatingProgressBar.Value = percentage/100d
                percentage =>
                    {
                        this.Dispatcher?.Invoke(() =>
                        {
                            if (percentage == 0)
                                iterationCount = false;

                            if(generatingProgressBar.Value == 100)
                            {
                                generatingProgressBar.Value = 0;
                                progressIterationText.Text = $"{currentIteration}/{maxIterations}";

                                currentIteration++;
                                iterationCount = true;
                            }

                            if(!iterationCount)
                                generatingProgressBar.Value = percentage;
                        });
                    }
                );

                return true;
            }

            return false;
        }

        
        /// <summary>
        /// Check if all data in preferences are correctly setup
        /// </summary>
        /// <returns></returns>
        private bool RunChecks(bool img2img)
        {
            if(string.IsNullOrWhiteSpace(PersistentPreferencesData.AnacondaPath))
            {
                consoleBox.AppendText("<run check> Error: Anaconda path not set. Please, use File->Preferences->Anaconda Installation to setup correct path.\n");
                return false;
            }

            if (img2img)
            {
                if (string.IsNullOrWhiteSpace(PersistentPreferencesData.Img2ImgPath))
                {
                    consoleBox.AppendText("<run check> Error: Img2Img path not set. Please, use File->Preferences->Img2Img file to setup correct path.\n");
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(PersistentPreferencesData.Txt2ImgPath))
                {
                    consoleBox.AppendText("<run check> Error: Txt2Img path not set. Please, use File->Preferences->Txt2Img file to setup correct path.\n");
                    return false;
                }
            }

            return true;
        }

        #region WPF Events
        private void generateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isGenerationRunning)
            {
                // stop process if running
                var running = ExternalProcessRunner.RunningGeneration;
                if (running != null)
                {
                    running.Kill();
                }

                _isGenerationRunning = false;
                generateBtn.Content = "Generate";
                return;
            }

            bool result;
            // check which mode we are in
            if (imgToImgGroup.Visibility == Visibility.Visible)
            {
                //img2img mode
                result = Img2ImgGenerate();
            }
            else
            {
                result = Txt2ImgGenerate();
            }

            if (result)
            {
                _isGenerationRunning = true;
                consoleBox.Clear();
                generateBtn.Content = "Stop";
            }
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
        private void promptBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton.HasFlag(MouseButtonState.Pressed) && !_isPromptReset)
            {
                _isPromptReset = true;
                promptBox.Text = "";
            }
        }

        private void outdirBtn_Click(object sender, RoutedEventArgs e)
        {
            // set output directory and save it to preferences.dat file

            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog()
            {
                SelectedPath = String.IsNullOrWhiteSpace(PersistentPreferencesData.OutDirPath) ? "" : PersistentPreferencesData.OutDirPath,
                Multiselect = false,
            };
            dialog.ShowDialog();

            var path = dialog.SelectedPath;

            if(string.IsNullOrWhiteSpace(path))
            {
                PersistentPreferencesData.OutDirPath = "";
            }
            else
            {
                PersistentPreferencesData.OutDirPath = path;
            }

            PersistentPreferencesData.Save();
        }

        private void preferencesMenu_Click(object sender, RoutedEventArgs e)
        {
            // open preferences window
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

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(ExternalProcessRunner.RunningGeneration != null)
            {
                ExternalProcessRunner.RunningGeneration.Kill();
            }

            Environment.Exit(0);
        }

        private string inputImage = "";
        private void initImg_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog()
            {
                Filter = "Images|*.png;*.jpg;*jpeg",
            };

            dialog.ShowDialog();

            var path = dialog.FileName;

            if (string.IsNullOrWhiteSpace(path))
            {
                inputImage = "";
            }
            else
            {
                inputImage = path;

                // display preview
                PreviewImage(inputImage);
            }
        }

        private void txt2img_Click(object sender, RoutedEventArgs e)
        {
            txtToImgGroup.Visibility = Visibility.Visible;
            imgToImgGroup.Visibility = Visibility.Hidden;
            promptBox.Width = 546;
        }

        private void img2img_Click(object sender, RoutedEventArgs e)
        {
            txtToImgGroup.Visibility = Visibility.Hidden;
            imgToImgGroup.Visibility = Visibility.Visible;
            promptBox.Width = 446;
        }

        #endregion

        private void PreviewImage(string inputImage)
        {
            inputImage = inputImage.Replace("\\", "/");
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(inputImage);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            previewImageBox.Source = bitmap;
        }
    }
}
