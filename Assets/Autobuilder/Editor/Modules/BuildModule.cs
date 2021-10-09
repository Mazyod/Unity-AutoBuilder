using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Autobuilder.ReorderableList;

namespace Autobuilder {
    public abstract class BuildModule {
        public const string BASE_DIR = "ProjectSettings/BuildModules";
        const string DEFINE_SYMBOLS = "DefineSymbols";
        const string BUILD_NUMBER = "BuildNumber";
        const string SORTING_INDEX = "SortingIndex";

        public abstract BuildTarget Target { get; }
        public abstract BuildTargetGroup TargetGroup { get; }
        public abstract bool IsTarget(BuildTarget aTarget);
        string EnabledKey => $"{Builder.BUILDER}.{ModuleName}.Enabled";
        public bool Enabled {
            get { return PlayerPrefs.GetInt(EnabledKey, 0) > 0; }
            private set { PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0); }
        }
        public int SortingIndex {
            get { return GetInt(SORTING_INDEX, 0); }
            set { SetInt(SORTING_INDEX, value); }
        }
        public int BuildNumber {
            get { return GetInt(BUILD_NUMBER, 0); }
            set { SetInt(BUILD_NUMBER, value); }
        }
        protected string BaseBuildPath => $"{Builder.DataPath}/{Builder.BuildPath}/{ModuleName}";
        string GUIUnfoldKey => $"{Builder.BUILDER}.Unfold.{ModuleName}";
        bool GUIUnfold {
            get { return PlayerPrefs.GetInt(GUIUnfoldKey, 0) > 0; }
            set { PlayerPrefs.SetInt(GUIUnfoldKey, value ? 1 : 0); }
        }

        public void OnGUI(out bool build, out bool development) {
            var isTarget = IsTarget(EditorUserBuildSettings.activeBuildTarget);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(80));
            if (SortingIndex > 0 && GUILayout.Button("^", GUILayout.ExpandHeight(true))) {
                SortingIndex -= 3;
            }
            if (Builder.TargetModuleInstalled(Target)) {
                Enabled = EditorGUILayout.Toggle("Build", Enabled);
            }
            if (GUILayout.Button("v", GUILayout.ExpandHeight(true))) {
                SortingIndex += 3;
            }
            GUILayout.EndVertical();

            // TODO: Change color if enabled/disabled
            GUILayout.BeginVertical(isTarget ? Builder.SelectedAreaStyle : Builder.AreaStyle);

            EditorGUI.BeginChangeCheck();
            var moduleName = EditorGUILayout.DelayedTextField(ModuleName);
            var buildNumber = EditorGUILayout.DelayedIntField("Build number", BuildNumber);
            if (EditorGUI.EndChangeCheck()) {
                bool enabled = Enabled;
                PlayerPrefs.DeleteKey(EnabledKey);
                ModuleName = moduleName;
                Enabled = enabled;
                BuildNumber = buildNumber;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Build Target Group: {TargetGroup}");
            GUILayout.Label($"Build Target: {Target}");
            GUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();

            // DefineSymbols
            ReorderableListGUI.Title("Define Symbols");
            ReorderableListGUI.ListField(m_DefineSymbolsAdaptor);
            
            // Options
            var unfold = EditorGUILayout.Foldout(GUIUnfold, "Options");
            if (EditorGUI.EndChangeCheck()) {
                GUIUnfold = unfold;
                PlayerPrefs.Save();
            }
            if (unfold) {
                OptionsGUI(out build, out development);
            } else {
                build = false;
                development = false;
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        public abstract void OptionsGUI(out bool aBuild, out bool aDevelopment);

        /// <summary>
        /// Builds the game
        /// </summary>
        /// <returns>True if successfull</returns>
        public virtual bool BuildGame(bool development = false) {
            if (!Enabled) return false;

            SetDefineSymbols();

            return true;
        }

        // Data
        string FileName => $"{BASE_DIR}/{ModuleName}.bm";
        public string ModuleName;
        public string ModuleTarget => $"{TargetGroup}.{Target}";
        protected JObject Data;
        protected JArray DefineSymbols;
        JSONNodeAdaptor m_DefineSymbolsAdaptor;

        public BuildModule() {
            Load(new JObject());
        }

        public virtual void Load(JObject root) {
            Data = root;
            var node = Data[DEFINE_SYMBOLS];
            if (node == null || !(node is JArray)) {
                DefineSymbols = new JArray();
                Data[DEFINE_SYMBOLS] = DefineSymbols;
            } else {
                DefineSymbols = node as JArray;
            }
            m_DefineSymbolsAdaptor = new JSONNodeAdaptor(DefineSymbols);
        }

        public virtual void Load() {
            var text = File.ReadAllText(FileName);
            Load(JObject.Parse(text));
        }

        public void Save() {
            Data["ModuleTarget"] = ModuleTarget;
            File.WriteAllText(FileName, Data.ToString(Formatting.Indented));
        }

        protected bool GetBool(string name, bool defaultValue = false) {
            var node = Data[name];
            if (node == null) return defaultValue;
            return (bool) node;
        }

        protected void SetBool(string name, bool value) {
            Data[name] = value;
        }

        protected float GetFloat(string name, float defaultValue = 0) {
            var node = Data[name];
            if (node == null) return defaultValue;
            return (float) node;
        }

        protected void SetFloat(string name, float value) {
            Data[name] = value;
        }

        protected int GetInt(string name, int defaultValue = 0) {
            var node = Data[name];
            if (node == null) return defaultValue;
            return (int) node;
        }

        protected void SetInt(string name, int value) {
            Data[name] = value;
        }

        protected string GetString(string name, string defaultValue) {
            var node = Data[name];
            if (node == null) return defaultValue;
            return (string) node;
        }

        protected void SetString(string name, string value) {
            Data[name] = value;
        }

        public void SetDefineSymbols() {
            string defines = "";
            foreach (var item in DefineSymbols) {
                defines += item + ";";
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(TargetGroup, defines);
        }

        protected string[] GetScenesList() {
            var scenes = new List<string>();

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++) {
                if (EditorBuildSettings.scenes[i].enabled)
                    scenes.Add(EditorBuildSettings.scenes[i].path);
            }
            return scenes.ToArray();
        }
    }
}
