using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StableDiffusionGUI
{
    [Serializable]
    public class SerializedCommand
    {
        public SerializedCommand(string width, string iter, string samples, string seed, string height, string ddimSteps, string scale, bool plms, string prompt)
        {
            Width = width;
            Iter = iter;
            Samples = samples;
            Seed = seed;
            Height = height;
            DdimSteps = ddimSteps;
            Scale = scale;
            Plms = plms;
            Prompt = prompt;
        }
        public bool Plms { get; set; }
        public string Width { get; set; }
        public string Iter { get; set; }
        public string Samples { get; set; }
        public string Seed { get; set; }
        public string Height { get; set; }
        public string DdimSteps { get; set; }
        public string Scale { get; set; }
        public string Prompt { get; set; }

        public static SerializedCommand Generate(MainWindow window)
        {
            return new SerializedCommand(window.widthBox.Text, window.niterBox.Text, window.nsamplesBox.Text,
                                                window.seedBox.Text, window.heightBox.Text, window.ddimBox.Text,
                                                window.scaleBox.Text, window.plmsCheck.IsChecked == true, window.promptBox.Text);
        }

        public static SerializedCommand Load(string path)
        {
            return JsonSerializer.Deserialize<SerializedCommand>(File.ReadAllText(path));
        }

        internal void Save(string path)
        {
            var data = JsonSerializer.Serialize(this);
            File.WriteAllText(path, data);
        }
    }
}
