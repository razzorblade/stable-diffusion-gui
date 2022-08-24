using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StableDiffusionGUI
{
    public static class PersistentPreferencesData
    {
        public static string AnacondaPath = "";

        public static string Txt2ImgPath = "";
        public static string Img2ImgPath = "";

        public static string OutDirPath = "";

        public static PreferencesData GetInstance()
        {
            return new PreferencesData() { AnacondaPath = AnacondaPath, Txt2ImgPath = Txt2ImgPath, OutDirPath = OutDirPath, Img2ImgPath = Img2ImgPath };
        }

        public static void Save()
        {
            string jsonString = JsonSerializer.Serialize(GetInstance());
            using StreamWriter file = new("preferences.dat", append: false);
            file.Write(jsonString);
        }

        public static void Load()
        {
            if (!File.Exists("preferences.dat"))
                return;

            var deserialized = JsonSerializer.Deserialize<PreferencesData>(File.ReadAllText("preferences.dat"));

            if (deserialized == null)
                return;

            AnacondaPath = deserialized.AnacondaPath;
            Txt2ImgPath = deserialized.Txt2ImgPath;
            Img2ImgPath = deserialized.Img2ImgPath;
            OutDirPath = deserialized.OutDirPath;
        }
    }

    [Serializable]
    public class PreferencesData
    {
        public string AnacondaPath { get; set; }
        public string Txt2ImgPath { get; set; }
        public string Img2ImgPath { get; set; }
        public string OutDirPath { get; set; }

        public PreferencesData()
        {
            AnacondaPath = "";
            Txt2ImgPath = "";
            OutDirPath = "";
            Img2ImgPath = "";
        }
    }
}
