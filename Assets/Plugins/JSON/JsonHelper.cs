using Newtonsoft.Json;
using System.IO;

namespace STVR.JSON
{
    public static class JsonHelper
    {
        public interface IJsonData { }
        static JsonHelper() { }

        public static void WriteToJson<T>(T data, string fileName, params string[] dir) where T : IJsonData
        {
            string validateDir = "";
            string validateFile = "";

            for (int i = 0; i < dir.Length; i++)
            {
                validateDir = Path.Combine(validateDir, dir[i]);
            }

            validateFile = Path.Combine(validateDir, $"{fileName}.json");

            if (!Directory.Exists(validateDir))
                Directory.CreateDirectory(validateDir);

            if (Directory.Exists(validateDir))
            {
                string jsonConvert = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(jsonConvert);

                using (StreamWriter file = File.CreateText(validateFile))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    json.WriteTo(writer);
                }
            }

        }

        public static T ReadJson<T>(string fileDirectory) where T : IJsonData
        {
            if (File.Exists(fileDirectory))
            {
                Newtonsoft.Json.Linq.JObject objects = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(fileDirectory));
                string noFormat = objects.ToString(Formatting.None);
                T necessityFiles = JsonConvert.DeserializeObject<T>(noFormat);

                return necessityFiles;
            }

            return default(T);
        }
    }
}
