using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace StableDiffusionGUI
{
    public static class ExternalProcessRunner
    {
        public static void Run(string pythonProcess, string args, Action<string> callback)
        {
            var workingDirectory = Directory.GetParent(new FileInfo(PersistentPreferencesData.Txt2ImgPath).DirectoryName).FullName;
            var process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                }
            };
            process.Exited += (object? s, EventArgs args) => callback(workingDirectory);
            process.Start();

            // Pass multiple commands to cmd.exe
            using (var sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    // Vital to activate Anaconda
                    var anaconda = PersistentPreferencesData.AnacondaPath.Replace("\\","/");
                    sw.WriteLine($"{Path.Join(anaconda, "Scripts/activate.bat")}");

                    // Activate environment
                    sw.WriteLine("activate ldm");
                    // run
                    var pythonFile = pythonProcess.Replace("\\", "/");//PersistentPreferencesData.Txt2ImgPath.Replace("\\","/");
                    sw.WriteLine($"python {pythonFile} {args}");
                }
            }

            // read multiple output lines
             /*while (!process.StandardOutput.EndOfStream)
             {
                 var line = process.StandardOutput.ReadLine();
                 Console.WriteLine(line);
             }
             */
            while (!process.HasExited)
            {
                var line = process.StandardOutput.ReadLine();
                Console.WriteLine(line);

                var err = process.StandardError.ReadLine();
                Console.WriteLine(err);
            }
        }
    }
}
