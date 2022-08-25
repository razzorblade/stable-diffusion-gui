using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace StableDiffusionGUI
{
    public class GenerationProcessData
    {
        public  Process? RunningGeneration { get; set; }
        public Task? BackgroundRunner { get; set; }

        public Thread? ErrorStream { get; set; }
        public Thread? StandardStream { get; set; }

        internal void Kill()
        {
            if(RunningGeneration != null)
            {
                StandardStream?.Interrupt();
                ErrorStream?.Interrupt();

                RunningGeneration.Kill();
                RunningGeneration.WaitForExit();
                RunningGeneration.Dispose();
            }
        }
    }

    /// <summary>
    /// Handles running of python scripts through Anaconda
    /// </summary>
    public static class ExternalProcessRunner
    {
        public static GenerationProcessData? RunningGeneration { get; set; }

        /// <summary>
        /// Run a specific python process to generate images - either img2img or txt2img
        /// </summary>
        /// <param name="pythonProcess">Path to python process</param>
        /// <param name="args">Argument chain (for example --prompt "..." --W 512 --H 512)</param>
        /// <param name="callback">Called on process exit</param>
        public static void Run(string pythonProcess, string args, Action<string> callback, bool img2img = false)
        {
            RunningGeneration = new();

            var task = Task.Run(() => BackgroundRun(pythonProcess, args, callback, img2img));
            RunningGeneration.BackgroundRunner = task;
        }

        /// <summary>
        /// Creates a process and threads listening to process' streams
        /// </summary>
        /// <param name="pythonProcess"></param>
        /// <param name="args"></param>
        /// <param name="callback"></param>
        private static void BackgroundRun(string pythonProcess, string args, Action<string> callback, bool img2img = false)
        {
            // preferences are already checked to not be empty/null ->
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
            var workingDirectory = Directory.GetParent(new FileInfo(PersistentPreferencesData.Txt2ImgPath).DirectoryName).FullName;
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            // build the process that will silently start the console and redirect outputs
            var process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = !img2img,
                    UseShellExecute = false,
                    CreateNoWindow = !img2img,
                    WorkingDirectory = workingDirectory
                }
            };
            process.Exited += (object? s, EventArgs args) =>
            {
                callback(workingDirectory);
                RunningGeneration = null;
            };

            if(RunningGeneration != null)
                RunningGeneration.RunningGeneration = process;

            process.Start();

            // Pass multiple commands to cmd.exe
            using (var sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    // Vital to activate Anaconda
                    var anaconda = PersistentPreferencesData.AnacondaPath.Replace("\\", "/");
                    sw.WriteLine($"{Path.Join(anaconda, "Scripts/activate.bat")}");

                    // Activate environment ldm
                    sw.WriteLine("activate ldm");

                    // run python script (txt2img or img2img)
                    var pythonFile = pythonProcess.Replace("\\", "/");//PersistentPreferencesData.Txt2ImgPath.Replace("\\","/");
                    sw.WriteLine($"python \"{pythonFile}\" {args}");
                }
            }

            // move to new thread ->
            ThreadStart stdThr = new(() => ListenOutput(process.StandardOutput));
            ThreadStart errThr = new(() => ListenOutput(process.StandardError));

            Thread std = new(stdThr);
            Thread err = new(errThr);

            std.Start();

            if(!img2img)
                err.Start();

            if (RunningGeneration != null)
            {
                RunningGeneration.StandardStream = std;
                RunningGeneration.ErrorStream = err;
            }
        }

        /// <summary>
        /// Read the stream until EOF
        /// </summary>
        /// <param name="process"></param>
        private static void ListenOutput(StreamReader process)
        {
            // read multiple output lines
            try
            {
                while (!process.EndOfStream)
                {
                    var line = process.ReadLine();
                    Console.WriteLine(line);
                }
            }
            catch (ThreadInterruptedException ex)
            {
                Console.WriteLine("\nStream interrupted: " + ex.Message);
            }
        }
    }
}
