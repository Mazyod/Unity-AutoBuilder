using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;

namespace Autobuilder {
    public class Builder : EditorWindow {
        #region Constants
        public const string BUILDER = "Autobuilder";
        const string BUILDS_PATH = BUILDER + "BuildsPath";
        const string FILENAME = BUILDER + "BuildFilename";
        const string START_WITH_CURRENT = BUILDER + "StartWithCurrent";
        const string END_WITH_CURRENT = BUILDER + "EndWithCurrent";
        const string SWITCH_TO_CURRENT = BUILDER + "SwitchToCurrent";
        const string AUTO_INCREASE_BUILD = BUILDER + "AutoIncreaseBuild";
        const string DEFAULT_BUILDS_PATH = "Builds";
        public const string BUILD_64 = "_x86_64";
        public const string BUILD_UNIVERSAL = "Universal";
        public const int COLUMN0 = 100;
        public const int COLUMN1 = 70;
        public const int COLUMN1_HALF = COLUMN1 / 2;
        #endregion

        #region EditorPrefs
        public static string BuildPath {
            get {
                return EditorProjectPrefs.GetString(BUILDS_PATH, DEFAULT_BUILDS_PATH);
            }
            set { EditorProjectPrefs.SetString(BUILDS_PATH, value); }
        }
        public static string FileName {
            get {
                return EditorProjectPrefs.GetString(FILENAME,
                    PlayerSettings.productName);
            }
            set { EditorProjectPrefs.SetString(FILENAME, value); }
        }
        public static bool StartWithCurrent {
            get { return EditorProjectPrefs.GetBool(START_WITH_CURRENT, true); }
            set { EditorProjectPrefs.SetBool(START_WITH_CURRENT, value); }
        }
        public static bool EndWithCurrent {
            get { return EditorProjectPrefs.GetBool(END_WITH_CURRENT, false); }
            set { EditorProjectPrefs.SetBool(END_WITH_CURRENT, value); }
        }
        public static bool SwitchToCurrent {
            get { return EditorProjectPrefs.GetBool(SWITCH_TO_CURRENT, true); }
            set { EditorProjectPrefs.SetBool(SWITCH_TO_CURRENT, value); }
        }
        public static bool AutoIncreaseBuild {
            get { return EditorProjectPrefs.GetBool(AUTO_INCREASE_BUILD, true); }
            set { EditorProjectPrefs.SetBool(AUTO_INCREASE_BUILD, value); }
        }
        #endregion

        static readonly BuildModule[] MODULES = {
            new WindowsModule(),
            new LinuxModule(),
            new OSXModule(),
            new AndroidModule(),
            new IOSModule(),
            new TVOSModule(),
            new WebGLModule(),
        };

        static Vector2 m_ScrollPos;
        static string m_DataPath;
        public static string DataPath {
            get {
                if (string.IsNullOrEmpty(m_DataPath))
                    m_DataPath = Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
                return m_DataPath;
            }
        }
        static GUIStyle m_AreaStyle;
        public static GUIStyle AreaStyle {
            get {
                if (m_AreaStyle == null || m_AreaStyle.normal.background == null) {
                    m_AreaStyle = new GUIStyle(EditorStyles.helpBox);
                }
                return m_AreaStyle;
            }
        }
        static GUIStyle m_SelectedAreaStyle;
        static Texture2D m_SelectedTexture;
        public static GUIStyle SelectedAreaStyle {
            get {
                if (m_SelectedAreaStyle == null) {
                    m_SelectedAreaStyle = new GUIStyle(AreaStyle);
                    m_SelectedTexture = new Texture2D(1, 1);
                    m_SelectedTexture.SetPixel(0, 0, new Color(0.423f, 0.498f, 0.431f));
                    m_SelectedTexture.Apply();
                    m_SelectedAreaStyle.normal.background = m_SelectedTexture;
                }
                if (m_SelectedAreaStyle.normal.background == null) {
                    m_SelectedTexture = new Texture2D(1, 1);
                    m_SelectedTexture.SetPixel(0, 0, new Color(0.423f, 0.498f, 0.431f));
                    m_SelectedTexture.Apply();
                    m_SelectedAreaStyle.normal.background = m_SelectedTexture;
                }
                return m_SelectedAreaStyle;
            }
        }
        static Texture textureSettings;
        static Texture TextureSettings {
            get {
                if (textureSettings == null) {
                    textureSettings = Resources.Load<Texture>("settings");
                }
                return textureSettings;
            }
        }
        static GUIContent settingsContent;
        static GUIContent SettingsContent {
            get {
                if (settingsContent == null) {
                    settingsContent = new GUIContent(TextureSettings);
                }
                return settingsContent;
            }
        }

        int selectedModule;
        string[] moduleOptions;
        List<BuildModule> modules;

        [MenuItem("File/Autobuilder...")]
        [MenuItem("Tools/Autobuilder...")]
        public static void ShowWindow() {
            GetWindow<Builder>("Builder");
        }

        void LoadModules() {
            modules.Clear();
            if (!Directory.Exists(BuildModule.BASE_DIR)) {
                Directory.CreateDirectory(BuildModule.BASE_DIR);
            }
            var files = Directory.GetFiles(BuildModule.BASE_DIR, "*.bm");
            foreach (var filename in files) {
                var rootNode = JObject.Parse(File.ReadAllText(filename));
                var moduleTarget = rootNode["ModuleTarget"];
                if (moduleTarget != null) {
                    var module = GetModule((string) moduleTarget);
                    if (module != null) {
                        module.ModuleName = Path.GetFileNameWithoutExtension(filename);
                        module.Load();
                        modules.Add(module);
                    }
                }
            }

            SortModules();
        }

        int ModuleSorting(BuildModule module1, BuildModule module2) {
            return module1.SortingIndex.CompareTo(module2.SortingIndex);
        }

        void SortModules() {
            modules.Sort(ModuleSorting);

            for (int i = 0; i < modules.Count; i++) {
                modules[i].SortingIndex = i * 2;
                modules[i].Save();
            }
        }

        BuildModule GetModule(string moduleTarget) {
            for (int i = 0; i < MODULES.Length; i++) {
                if (MODULES[i].ModuleTarget == moduleTarget) return GetModule(MODULES[i]);
            }
            return null;
        }

        BuildModule GetModule(BuildModule baseModule) {
            if (baseModule is WindowsModule) return new WindowsModule();
            if (baseModule is LinuxModule) return new LinuxModule();
            if (baseModule is OSXModule) return new OSXModule();
            if (baseModule is AndroidModule) return new AndroidModule();
            if (baseModule is IOSModule) return new IOSModule();
            if (baseModule is TVOSModule) return new TVOSModule();
            if (baseModule is WebGLModule) return new WebGLModule();
            return null;
        }

        string GetAvailableModuleFileName(string name) {
            var num = -1;
            do {
                num++;
            } while (File.Exists(Path.Combine(BuildModule.BASE_DIR, $"{name} {num}.bm")));

            return $"{name} {num}";
        }

        public static bool TargetModuleInstalled(BuildTarget target) {
            var moduleManager = Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
            var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            return (bool) isPlatformSupportLoaded.Invoke(null, new object[] {
                (string)getTargetStringFromBuildTarget.Invoke(null, new object[] { target })
            });
        }

        void OnEnable() {
            modules = new List<BuildModule>();

            moduleOptions = new string[MODULES.Length];
            for (int i = 0; i < moduleOptions.Length; i++) {
                moduleOptions[i] = MODULES[i].ModuleTarget;
            }

            LoadModules();
        }

        private void OnDisable() {
            PlayerPrefs.Save();
        }

        private void OnGUI() {
            GUILayout.Space(10);
            bool makeBuild = false;
            var buildModules = new List<BuildModule>();
            bool development = false;

            EditorGUI.BeginChangeCheck();

            GUIContent tContent = new GUIContent("Builds root dir",
                "All builds will be saved in subfolders of this directory");
            string tBuildPath = CustomEditorGUILayout.DirPathField(tContent, DataPath + "/" + BuildPath,
                "Select a root directory for all the builds", DataPath + "/" + DEFAULT_BUILDS_PATH);

            tContent.text = "Build filename";
            tContent.tooltip = "All builds will be saved with this filename";
            FileName = EditorGUILayout.TextField(tContent, FileName);

            PlayerSettings.bundleVersion = EditorGUILayout.TextField(
                "Version", PlayerSettings.bundleVersion);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(SettingsContent)) {
                GetWindow<BuildPlayerWindow>();
            }
            GUILayout.BeginVertical();
#if !UNITY_2018_1_OR_NEWER
            if ( GUILayout.Button("Player settings") ) {
                EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
            }
#endif
            if (GUILayout.Button("Build ALL", GUILayout.ExpandHeight(true))) {
                makeBuild = true;
                development = false;
                buildModules.AddRange(modules);
            }
            if (GUILayout.Button("Build ALL development", GUILayout.ExpandHeight(true))) {
                makeBuild = true;
                development = true;
                buildModules.AddRange(modules);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            AutoIncreaseBuild = GUILayout.Toggle(AutoIncreaseBuild, "Automatically increase build number");
            bool tStartWithCurrent = GUILayout.Toggle(StartWithCurrent, "Start with current platform");
            bool tEndWithCurrent = GUILayout.Toggle(EndWithCurrent, "End with current platform");
            if (tStartWithCurrent && tEndWithCurrent) {
                if (!StartWithCurrent)
                    tEndWithCurrent = false;
                else if (!EndWithCurrent)
                    tStartWithCurrent = false;
            }
            SwitchToCurrent = GUILayout.Toggle(SwitchToCurrent, "Switch to current platform when done");

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            selectedModule = EditorGUILayout.Popup(selectedModule, moduleOptions);
            if (GUILayout.Button("New module")) {
                var newModuleInfo = GetModule(MODULES[selectedModule]);
                newModuleInfo.ModuleName = GetAvailableModuleFileName(newModuleInfo.ModuleTarget);
                newModuleInfo.Save();
                LoadModules();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("RELOAD MODULES")) {
                LoadModules();
            }

            m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos);
            for (int i = 0; i < modules.Count; i++) {
                GUILayout.Space(10);
                var module = modules[i];
                bool isTarget = module.IsTarget(
                    EditorUserBuildSettings.activeBuildTarget);

                EditorGUI.BeginChangeCheck();
                var moduleName = module.ModuleName;
                module.OnGUI(out makeBuild, out development);
                if (makeBuild) {
                    buildModules.Add(module);
                }

                if (EditorGUI.EndChangeCheck()) {
                    if (moduleName != module.ModuleName) {
                        File.Delete($"{BuildModule.BASE_DIR}/{moduleName}.bm");
                    }
                    module.Save();
                }

                if (TargetModuleInstalled(module.Target)) {
                    GUILayout.BeginHorizontal();
                    if (!isTarget) {
                        if (GUILayout.Button("Switch to target")) {
                            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(
                                module.TargetGroup, module.Target);
                        }
                    }
                    if (GUILayout.Button("Build")) {
                        makeBuild = true;
                        buildModules.Add(module);
                    }
                    if (GUILayout.Button("Development build")) {
                        makeBuild = true;
                        buildModules.Add(module);
                        development = true;
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck()) {
                StartWithCurrent = tStartWithCurrent;
                EndWithCurrent = tEndWithCurrent;
                BuildPath = PathFunctions.GetRelativePath(tBuildPath, DataPath);
                SortModules();
                EditorProjectPrefs.Save();
            }

            if (makeBuild) {
                BuildGame(modules, development);
            }
        }

        public static bool BuildGame(List<BuildModule> modules, bool development) {
            List<BuildModule> buildOrder = new List<BuildModule>();
            BuildTarget currentTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup tCurrentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            for (int i = 0; i < modules.Count; i++) {
                var module = modules[i];
                if (module.Enabled && TargetModuleInstalled(module.Target)) {
                    if (!development) {
                        module.BuildNumber++;
                    }
                    if (StartWithCurrent && module.IsTarget(currentTarget)) {
                        buildOrder.Insert(0, module);
                    } else {
                        buildOrder.Add(module);
                    }
                }
            }

            if (EndWithCurrent) {
                for (int i = 0; i < buildOrder.Count; i++) {
                    var module = buildOrder[i];
                    if (module.IsTarget(currentTarget)) {
                        buildOrder.RemoveAt(i);
                        buildOrder.Add(module);
                        break;
                    }
                }
            }

            bool result = true;
            foreach (var module in buildOrder) {
                Debug.Log($"Build {module.ModuleName} ({module.GetType()})");
                if (!module.BuildGame(development)) {
                    module.BuildNumber--;
                    result = false;
                }
                module.Save();
            }

            if (SwitchToCurrent) {
                EditorUserBuildSettings.SwitchActiveBuildTarget(tCurrentGroup, currentTarget);
            }

            return result;
        }

#if UNITY_2018_1_OR_NEWER
        public static BuildReport BuildGame(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, string buildPath, string[] scenes, bool development = false)
#else
        public static string BuildGame(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, string[] scenes, bool development = false)
#endif
        {
            Debug.Log($"Switch to {buildTargetGroup}:{buildTarget}");
            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
            // Build player
            BuildPlayerOptions tOptions = new BuildPlayerOptions {
                locationPathName = buildPath,
                target = buildTarget,
                options = (development ? BuildOptions.Development | BuildOptions.AllowDebugging
                    : BuildOptions.None),
                scenes = scenes,
            };

            // Pre processors
            RunPreProcessor(buildTarget, development);

#if UNITY_2018_1_OR_NEWER
            BuildReport tReport = BuildPipeline.BuildPlayer(tOptions);
            Debug.Log("Build " + buildTarget + ": " + tReport.summary.result);
            if (tReport.summary.result == BuildResult.Succeeded)
#else
            string tReport = BuildPipeline.BuildPlayer(tOptions);
            Debug.Log("Build " + aTarget + ": " + tReport);
            if (string.IsNullOrEmpty(tReport))
#endif
            {
                EditorUserBuildSettings.SetBuildLocation(buildTarget, tOptions.locationPathName);
                EditorUtility.RevealInFinder(tOptions.locationPathName);
            }

            // Post processors
            RunPostProcessor(buildTarget, development, tReport);

            return tReport;
        }

        // Currently not working
        static void RunPreProcessor(BuildTarget aTarget, bool aDevelopment) {
            foreach (Type type in AttributeFinder.GetTypesWithAttribute<BuildPreProcessAttribute>(AppDomain.CurrentDomain)) {
                if (type.IsSubclassOf(typeof(IBuildPreProcessor)))
                    ((IBuildPreProcessor) Activator.CreateInstance(type)).PreProcess(aTarget, aDevelopment);
            }
        }

#if UNITY_2018_1_OR_NEWER
        static void RunPostProcessor(BuildTarget aTarget, bool aDevelopment, BuildReport aReport)
#else
        static void RunPostProcessor(BuildTarget aTarget, bool aDevelopment, string aReport)
#endif
        {
            foreach (Type type in AttributeFinder.GetTypesWithAttribute<BuildPostProcessAttribute>(AppDomain.CurrentDomain)) {
                if (type.IsSubclassOf(typeof(IBuildPostProcessor)))
                    ((IBuildPostProcessor) Activator.CreateInstance(type)).PostProcess(aTarget, aDevelopment, aReport);
            }
        }
    }
}
