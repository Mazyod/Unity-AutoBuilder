using Autobuilder.SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Autobuilder {
    public static class EditorProjectPrefs {
        const string OLD_SETTINGS_PATH = "CustomSettings/";
        const string SETTINGS_PATH = "ProjectSettings/";
        const string FILEPATH = "EditorProjectPrefs.json";
        static string OldFilePath {
            get {
                return Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length)
+ "/" + OLD_SETTINGS_PATH + FILEPATH;
            }
        }
        static string FilePath {
            get {
                return Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length)
+ "/" + SETTINGS_PATH + FILEPATH;
            }
        }

        static JSONObject m_Prefs;
        static JSONObject Prefs {
            get {
                if (m_Prefs == null) {
                    MoveOldFile();
                    if (File.Exists(FilePath)) {
                        try {
                            JSONNode tNode = JSON.Parse(File.ReadAllText(FilePath));
                            if (tNode.IsObject)
                                m_Prefs = tNode.AsObject;
                            else
                                m_Prefs = new JSONObject();
                        }
                        catch {
                            m_Prefs = new JSONObject();
                        }
                    } else
                        m_Prefs = new JSONObject();
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
                Prefs[key] = new JSONBool(value);
            else
                Prefs[key].AsBool = value;
            Save();
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static bool GetBool(string key, bool defaultValue) {
            if (Prefs[key] == null)
                return defaultValue;
            else
                return Prefs[key].AsBool;
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
                Prefs[key] = new JSONString(value);
            else
                Prefs[key].Value = value;
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static string GetString(string key, string defaultValue) {
            if (Prefs[key] == null)
                return defaultValue;
            else
                return Prefs[key].Value;
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
                Prefs[key] = new JSONNumber(value);
            else
                Prefs[key].AsInt = value;
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static int GetInt(string key, int defaultValue) {
            if (Prefs[key] == null)
                return defaultValue;
            else
                return Prefs[key].AsInt;
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
                Prefs[key] = new JSONNumber(value);
            else
                Prefs[key].AsFloat = value;
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static float GetFloat(string key, float defaultValue) {
            if (Prefs[key] == null)
                return defaultValue;
            else
                return Prefs[key].AsFloat;
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
                Prefs[key] = new JSONNumber(value);
            else
                Prefs[key].AsDouble = value;
        }

        /// <summary>
        /// Returns the value corresponding to the key in the preferences file if it exists.
        /// </summary>
        public static double GetDouble(string key, double defaultValue) {
            if (Prefs[key] == null)
                return defaultValue;
            else
                return Prefs[key].AsDouble;
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
            JSONArray node = new JSONArray();
            for (int i = 0; i < list.Count; i++)
                node[i] = new JSONNumber(list[i]);
            Prefs[key] = node;
        }

        /// <summary>
        /// Retrieves a list of integers with the specified key and saves it in the provided list.
        /// </summary>
        public static void GetList(string key, ref int[] list) {
            JSONNode node = Prefs[key];
            if (node != null) {
                if (node.IsArray) {
                    JSONArray tArray = node.AsArray;
                    list = new int[tArray.Count];
                    for (int i = 0; i < tArray.Count; i++)
                        list[i] = tArray[i].AsInt;
                } else if (node.IsNumber) {
                    list = new int[1];
                    list[1] = node.AsInt;
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
            JSONNode node = Prefs[key];
            if (node != null) {
                if (node.IsArray) {
                    JSONArray tArray = node.AsArray;
                    for (int i = 0; i < tArray.Count; i++)
                        list.Add(tArray[i].AsInt);
                } else if (node.IsNumber) {
                    list.Add(node.AsInt);
                }
            }
        }

        // Float
        /// <summary>
        /// Saves a list of floats with the specified key.
        /// </summary>
        public static void SetList(string key, IList<float> list) {
            JSONArray node = new JSONArray();
            for (int i = 0; i < list.Count; i++)
                node[i] = new JSONNumber(list[i]);
            Prefs[key] = node;
        }

        /// <summary>
        /// Retrieves a list of floats with the specified key and saves it in the provided list.
        /// </summary>
        public static void GetList(string key, ref float[] list) {
            JSONNode node = Prefs[key];
            if (node != null) {
                if (node.IsArray) {
                    JSONArray tArray = node.AsArray;
                    list = new float[tArray.Count];
                    for (int i = 0; i < tArray.Count; i++)
                        list[i] = tArray[i].AsFloat;
                } else if (node.IsNumber) {
                    list = new float[1];
                    list[1] = node.AsFloat;
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
            JSONNode node = Prefs[key];
            if (node != null) {
                if (node.IsArray) {
                    JSONArray tArray = node.AsArray;
                    for (int i = 0; i < tArray.Count; i++)
                        list.Add(tArray[i].AsFloat);
                } else if (node.IsNumber) {
                    list.Add(node.AsFloat);
                }
            }
        }

        // Double
        /// <summary>
        /// Saves a list of doubles with the specified key.
        /// </summary>
        public static void SetList(string key, IList<double> list) {
            JSONArray node = new JSONArray();
            for (int i = 0; i < list.Count; i++)
                node[i] = new JSONNumber(list[i]);
            Prefs[key] = node;
        }

        /// <summary>
        /// Retrieves a list of doubles with the specified key and saves it in the provided list.
        /// </summary>
        public static void GetList(string key, ref double[] list) {
            JSONNode node = Prefs[key];
            if (node != null) {
                if (node.IsArray) {
                    JSONArray tArray = node.AsArray;
                    list = new double[tArray.Count];
                    for (int i = 0; i < tArray.Count; i++)
                        list[i] = tArray[i].AsFloat;
                } else if (node.IsNumber) {
                    list = new double[1];
                    list[1] = node.AsFloat;
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
            JSONNode node = Prefs[key];
            if (node != null) {
                if (node.IsArray) {
                    JSONArray tArray = node.AsArray;
                    for (int i = 0; i < tArray.Count; i++)
                        list.Add(tArray[i].AsDouble);
                } else if (node.IsNumber) {
                    list.Add(node.AsDouble);
                }
            }
        }

        public static void Save() {
            Directory.CreateDirectory(SETTINGS_PATH);
            File.WriteAllText(FilePath, Prefs.ToString());
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
            m_Prefs = new JSONObject();
            Save();
        }

        /// <summary>
        /// Returns true if the key exists in the preferences file.
        /// </summary>
        public static bool HasKey(string key) {
            return Prefs[key] != null;
        }
    }
}
