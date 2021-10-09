using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Autobuilder {
    public static class EditorProjectPrefs {
        const string OLD_SETTINGS_PATH = "CustomSettings";
        const string SETTINGS_PATH = "ProjectSettings";
        const string FILEPATH = "EditorProjectPrefs.json";
        static string Root => Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
        static string OldFilePath => Path.Combine(Root, OLD_SETTINGS_PATH, FILEPATH);
        static string FilePath => Path.Combine(Root, SETTINGS_PATH, FILEPATH);

        static JObject m_Prefs;
        static JObject Prefs {
            get {
                if (m_Prefs == null) {
                    MoveOldFile();
                    if (File.Exists(FilePath)) {
                        m_Prefs = JObject.Parse(File.ReadAllText(FilePath));
                    } else {
                        m_Prefs = new JObject();
                    }
                }
                return m_Prefs;
            }
        }

        static void MoveOldFile() {
            if (File.Exists(OldFilePath) && !File.Exists(FilePath)) {
                File.Move(OldFilePath, FilePath);
            }
        }

        // Bool
        /// <summary>
        /// Sets the value of the preferences identified by the key.
        /// </summary>
        public static void SetBool(string key, bool value) {
            if (Prefs[key] == null)
                Prefs[key] = new JValue(value);
            else
                Prefs[key] = value;
            Save();
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static bool GetBool(string key, bool defaultValue) {
            if (Prefs[key] == null)
                return defaultValue;
            else
                return (bool) Prefs[key];
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static bool GetBool(string key) {
            return GetBool(key, false);
        }

        // String
        /// <summary>
        /// Sets the value of the preferences identified by the key.
        /// </summary>
        public static void SetString(string key, string value) {
            if (Prefs[key] == null)
                Prefs[key] = new JValue(value);
            else
                Prefs[key] = value;
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static string GetString(string key, string defaultValue) {
            if (Prefs[key] == null)
                return defaultValue;
            else
                return (string) Prefs[key];
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static string GetString(string key) {
            return GetString(key, "");
        }

        // Int
        /// <summary>
        /// Sets the value of the preferences identified by the key.
        /// </summary>
        public static void SetInt(string key, int value) {
            if (Prefs[key] == null)
                Prefs[key] = new JValue(value);
            else
                Prefs[key] = value;
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static int GetInt(string key, int defaultValue) {
            if (Prefs[key] == null)
                return defaultValue;
            else
                return (int) Prefs[key];
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static int GetInt(string key) {
            return GetInt(key, 0);
        }

        // Float
        /// <summary>
        /// Sets the value of the preferences identified by the key.
        /// </summary>
        public static void SetFloat(string key, float value) {
            if (Prefs[key] == null)
                Prefs[key] = new JValue(value);
            else
                Prefs[key] = value;
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static float GetFloat(string key, float defaultValue) {
            if (Prefs[key] == null)
                return defaultValue;
            else
                return (float) Prefs[key];
        }


        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static float GetFloat(string key) {
            return GetFloat(key, 0f);
        }

        // Double
        /// <summary>
        /// Sets the value of the preferences identified by the key.
        /// </summary>
        public static void SetDouble(string key, double value) {
            if (Prefs[key] == null)
                Prefs[key] = new JValue(value);
            else
                Prefs[key] = value;
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static double GetDouble(string key, double defaultValue) {
            if (Prefs[key] == null)
                return defaultValue;
            else
                return (double) Prefs[key];
        }


        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static double GetDouble(string key) {
            return GetDouble(key, 0.0);
        }

        // Lists
        // Int
        /// <summary>
        /// Saves a list of integers with the specified key.
        /// </summary>
        public static void SetList(string key, IList<int> list) {
            JArray node = new JArray();
            for (int i = 0; i < list.Count; i++)
                node[i] = new JValue(list[i]);
            Prefs[key] = node;
        }

        /// <summary>
        /// Retrieves a list of integers with the specified key and saves it in the provided list.
        /// </summary>
        public static void GetList(string key, ref int[] list) {
            var node = Prefs[key];
            if (node != null) {
                if (node is JArray) {
                    var tArray = node as JArray;
                    list = new int[tArray.Count];
                    for (int i = 0; i < tArray.Count; i++)
                        list[i] = (int) tArray[i];
                } else if (node.Type == JTokenType.Integer) {
                    list = new int[1];
                    list[1] = (int) node;
                }
            } else
                list = new int[0];
        }

        /// <summary>
        /// Retrieves a list of integers with the specified key and saves it in the provided list.
        /// </summary>
        public static void GetList(string key, ref List<int> list) {
            if (list == null)
                list = new List<int>();
            list.Clear();
            var node = Prefs[key];
            if (node != null) {
                if (node is JArray) {
                    var tArray = node as JArray;
                    for (int i = 0; i < tArray.Count; i++)
                        list.Add((int) tArray[i]);
                } else if (node.Type == JTokenType.Integer) {
                    list.Add((int) node);
                }
            }
        }

        // Float
        /// <summary>
        /// Saves a list of floats with the specified key.
        /// </summary>
        public static void SetList(string key, IList<float> list) {
            var node = new JArray();
            for (int i = 0; i < list.Count; i++)
                node[i] = new JValue(list[i]);
            Prefs[key] = node;
        }

        /// <summary>
        /// Retrieves a list of floats with the specified key and saves it in the provided list.
        /// </summary>
        public static void GetList(string key, ref float[] list) {
            var node = Prefs[key];
            if (node != null) {
                if (node is JArray) {
                    var tArray = node as JArray;
                    list = new float[tArray.Count];
                    for (int i = 0; i < tArray.Count; i++)
                        list[i] = (float) tArray[i];
                } else if (node.Type == JTokenType.Integer) {
                    list = new float[1];
                    list[1] = (int) node;
                }
            } else
                list = new float[0];
        }

        /// <summary>
        /// Retrieves a list of floats with the specified key and saves it in the provided list.
        /// </summary>
        public static void GetList(string key, ref List<float> list) {
            if (list == null)
                list = new List<float>();
            list.Clear();
            var node = Prefs[key];
            if (node != null) {
                if (node is JArray) {
                    var tArray = node as JArray;
                    for (int i = 0; i < tArray.Count; i++)
                        list.Add((float) tArray[i]);
                } else if (node.Type == JTokenType.Float) {
                    list.Add((float) node);
                }
            }
        }

        // Double
        /// <summary>
        /// Saves a list of doubles with the specified key.
        /// </summary>
        public static void SetList(string key, IList<double> list) {
            var node = new  JArray();
            for (int i = 0; i < list.Count; i++)
                node[i] = new JValue(list[i]);
            Prefs[key] = node;
        }

        /// <summary>
        /// Retrieves a list of doubles with the specified key and saves it in the provided list.
        /// </summary>
        public static void GetList(string key, ref double[] list) {
            var node = Prefs[key];
            if (node != null) {
                if (node is JArray) {
                     JArray tArray = node as JArray;
                    list = new double[tArray.Count];
                    for (int i = 0; i < tArray.Count; i++)
                        list[i] = (float) tArray[i];
                } else if (node.Type == JTokenType.Float) {
                    list = new double[1];
                    list[1] = (float) node;
                }
            } else
                list = new double[0];
        }

        /// <summary>
        /// Retrieves a list of doubles with the specified key and saves it in the provided list.
        /// </summary>
        public static void GetList(string key, ref List<double> list) {
            if (list == null)
                list = new List<double>();
            list.Clear();
            var node = Prefs[key];
            if (node != null) {
                if (node is JArray) {
                     JArray tArray = node as JArray;
                    for (int i = 0; i < tArray.Count; i++)
                        list.Add((double) tArray[i]);
                } else if (node.Type == JTokenType.Float) {
                    list.Add((double) node);
                }
            }
        }

        public static void Save() {
            Directory.CreateDirectory(SETTINGS_PATH);
            File.WriteAllText(FilePath, Prefs.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        /// <summary>
        /// Removes key and its corresponding value from the preferences.
        /// </summary>
        public static void DeleteKey(string key) {
            Prefs.Remove(key);
            Save();
        }

        /// <summary>
        /// Removes all keys and values from the preferences. Use with caution.
        /// </summary>
        public static void DeleteAll() {
            m_Prefs = new JObject();
            Save();
        }

        /// <summary>
        /// Returns true if the key exists in the preferences file.
        /// </summary>
        public static bool HasKey(string key) {
            return Prefs.ContainsKey(key);
        }
    }
}
