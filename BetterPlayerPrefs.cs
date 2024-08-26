using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SpellSinger
{
    public static class BetterPlayerPrefs
    {
        private static readonly string IdbfsPath = GetIdbfsPath();

        public static void SetInt(string key, int value)
        {
            if (IsWebGl())
                WriteToIdbfs(key, value.ToString());
            else
                PlayerPrefs.SetInt(key, value);
        }

        public static int GetInt(string key, int defaultValue)
        {
            if (IsWebGl())
            {
                return ReadFromIdbfs(key, defaultValue, int.TryParse);
            }

            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static int GetInt(string key) => GetInt(key, 0);


        public static void SetFloat(string key, float value)
        {
            if (IsWebGl())
                WriteToIdbfs(key, value.ToString(CultureInfo.InvariantCulture));
            else
                PlayerPrefs.SetFloat(key, value);
        }

        public static float GetFloat(string key, float defaultValue)
        {
            if (IsWebGl())
            {
                return ReadFromIdbfs(key, defaultValue, (string value, out float val) =>
                    float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out val));
            }

            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static float GetFloat(string key) => GetFloat(key, 0.0f);


        public static void SetString(string key, string value)
        {
            if (IsWebGl())
                WriteToIdbfs(key, value);
            else
                PlayerPrefs.SetString(key, value);
        }


        public static string GetString(string key, string defaultValue)
        {
            if (IsWebGl())
            {
                return ReadFromIdbfs(key, defaultValue, (string value, out string val) =>
                {
                    val = value;
                    return true;
                });
            }

            return PlayerPrefs.GetString(key, defaultValue);
        }

        public static string GetString(string key) => GetString(key, "");


        public static bool HasKey(string key)
        {
            if (IsWebGl())
            {
                return File.Exists(GetFilePath(key));
            }

            return PlayerPrefs.HasKey(key);
        }


        public static void DeleteKey(string key)
        {
            if (IsWebGl())
                File.Delete(GetFilePath(key));
            else
                PlayerPrefs.DeleteKey(key);
        }

        public static void DeleteAll()
        {
            if (IsWebGl())
            {
                var directoryInfo = new DirectoryInfo(IdbfsPath);
                var fileInfos = directoryInfo.GetFiles();
                foreach (var fileInfo in fileInfos)
                    File.Delete(GetFilePath(fileInfo.Name));
            }
            else
                PlayerPrefs.DeleteAll();
        }

        private static bool IsWebGl()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        private static string GetIdbfsPath()
        {
            return $"/idbfs/{CleanupName(Application.companyName)}/{CleanupName(Application.productName)}/";
        }

        private static string CleanupName(string name)
        {
            return Regex.Replace(name, "[^0-9a-zA-Z_.-]+", "");
        }

        private static string GetFilePath(string filename)
        {
            return $"{IdbfsPath}/{filename}";
        }

        private delegate bool TryParse<T>(string value, out T val);

        private static T ReadFromIdbfs<T>(string key, T defaultValue, TryParse<T> parser)
        {
            var file = GetFilePath(key);
            if (!File.Exists(file))
                return defaultValue;

            if (parser.Invoke(File.ReadAllText(file), out var val))
            {
                return val;
            }

            return defaultValue;
        }

        private static void WriteToIdbfs(string key, string stringValue)
        {
            Directory.CreateDirectory(IdbfsPath);
            File.WriteAllText(GetFilePath(key), stringValue);
#if UNITY_WEBGL
            SyncIdbfs();
#endif
        }

#if UNITY_WEBGL
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool SyncIdbfs();
#endif
    }
}