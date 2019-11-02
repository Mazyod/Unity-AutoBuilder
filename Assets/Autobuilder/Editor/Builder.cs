using System;
using System.Collections.Generic;
using System.IO;
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
        #endregion

        static readonly IBuildModule[] MODULES = {
            new WindowsModule(),
            new LinuxModule(),
            new OSXModule(),
            new AndroidModule(),
            new IOSModule(),
        };

        static Vector2 m_ScrollPos;
        static string m_DataPath;
        public static string DataPath {
            get {
                if ( string.IsNullOrEmpty(m_DataPath) )
                    m_DataPath = Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
                return m_DataPath;
            }
        }
        static GUIStyle m_AreaStyle;
        public static GUIStyle AreaStyle {
            get {
                if ( m_AreaStyle == null || m_AreaStyle.normal.background == null ) {
                    m_AreaStyle = new GUIStyle(EditorStyles.helpBox);
                }
                return m_AreaStyle;
            }
        }
        static GUIStyle m_SelectedAreaStyle;
        static Texture2D m_SelectedTexture;
        public static GUIStyle SelectedAreaStyle {
            get {
                if ( m_SelectedAreaStyle == null ) {
                    m_SelectedAreaStyle = new GUIStyle(AreaStyle);
                    m_SelectedTexture = new Texture2D(1, 1);
                    m_SelectedTexture.SetPixel(0, 0, new Color(0.423f, 0.498f, 0.431f));
                    m_SelectedTexture.Apply();
                    m_SelectedAreaStyle.normal.background = m_SelectedTexture;
                }
                if ( m_SelectedAreaStyle.normal.background == null ) {
                    m_SelectedTexture = new Texture2D(1, 1);
                    m_SelectedTexture.SetPixel(0, 0, new Color(0.423f, 0.498f, 0.431f));
                    m_SelectedTexture.Apply();
                    m_SelectedAreaStyle.normal.background = m_SelectedTexture;
                }
                return m_SelectedAreaStyle;
            }
        }

        [MenuItem("File/Autobuilder...")]
        [MenuItem("Tools/Autobuilder...")]
        public static void ShowWindow() {
            GetWindow<Builder>("Builder");
        }

        public static bool TargetModuleInstalled(BuildTarget target) {
            var moduleManager = Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
            var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            return (bool)isPlatformSupportLoaded.Invoke(null, new object[] {
                (string)getTargetStringFromBuildTarget.Invoke(null, new object[] { target })
            });
        }

        private void OnGUI() {
            GUILayout.Space(10);
            Action<bool> tBuildFunction = null;
            bool tDevelopment = false;

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

            bool tStartWithCurrent = GUILayout.Toggle(StartWithCurrent, "Start with current platform");
            bool tEndWithCurrent = GUILayout.Toggle(EndWithCurrent, "End with current platform");
            if ( tStartWithCurrent && tEndWithCurrent ) {
                if ( !StartWithCurrent )
                    tEndWithCurrent = false;
                else if ( !EndWithCurrent )
                    tStartWithCurrent = false;
            }
            SwitchToCurrent = GUILayout.Toggle(SwitchToCurrent, "Switch to current platform when done");

            m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos);
            GUILayout.Label("Standalone", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            for ( int i = 0; i < MODULES.Length; i++ ) {
                var module = MODULES[i];
                if ( TargetModuleInstalled(module.Target) ) {
                    bool isTarget = module.IsTarget(
                        EditorUserBuildSettings.activeBuildTarget);
                    GUILayout.BeginVertical(isTarget ? SelectedAreaStyle : AreaStyle);

                    GUILayout.BeginHorizontal();
                    if ( !isTarget ) {
                        if ( GUILayout.Button(module.Name,
                                GUILayout.MaxWidth(COLUMN0))
                        ) {
                            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(
                                module.TargetGroup, module.Target);
                        }
                    } else {
                        GUILayout.Label(module.Name, EditorStyles.boldLabel,
                            GUILayout.MaxWidth(COLUMN0));
                    }
                    GUILayout.Label("Build:", GUILayout.MaxWidth(COLUMN1_HALF));
                    module.BuildNumber =
                        EditorGUILayout.IntField(module.BuildNumber,
                        GUILayout.MaxWidth(COLUMN1_HALF));

                    if ( GUILayout.Button("Build") ) {
                        tBuildFunction = module.BuildGame;
                    }
                    if ( GUILayout.Button("Development build") ) {
                        tBuildFunction = module.BuildGame;
                        tDevelopment = true;
                    }
                    GUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;
                    module.OnGUI(out bool build, out bool development);
                    EditorGUI.indentLevel--;

                    GUILayout.EndVertical();
                    if ( build ) {
                        tBuildFunction = module.BuildGame;
                        tDevelopment = development;
                    }
                } else {
                    GUILayout.BeginVertical(AreaStyle);
                    GUILayout.Label("Module " + module.Target + " not installed");
                    GUILayout.EndVertical();
                }
            }

            if ( EditorGUI.EndChangeCheck() ) {
                EditorProjectPrefs.Save();
            }

            GUILayout.EndScrollView();

            if ( EditorGUI.EndChangeCheck() ) {
                StartWithCurrent = tStartWithCurrent;
                EndWithCurrent = tEndWithCurrent;
                BuildPath = PathFunctions.GetRelativePath(tBuildPath, DataPath);
                EditorProjectPrefs.Save();
            }
            // Buttons
            GUILayout.BeginHorizontal();
#if !UNITY_2018_1_OR_NEWER
            if ( GUILayout.Button("Player settings") ) {
                EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
            }
#endif
            if ( GUILayout.Button("Build settings") ) {
                GetWindow<BuildPlayerWindow>();
            }
            if ( GUILayout.Button("Build ALL") ) {
                tBuildFunction = BuildGameAll;
                tDevelopment = false;
            }
            if ( GUILayout.Button("Build ALL development") ) {
                tBuildFunction = BuildGameAll;
                tDevelopment = true;
            }
            GUILayout.EndHorizontal();

            tBuildFunction?.Invoke(tDevelopment);
        }

        public static void BuildGameAll(bool aDevelopment) {
            List<Action<bool>> tBuildOrder = new List<Action<bool>>();
            BuildTarget tCurrent = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup tCurrentGroup =
                EditorUserBuildSettings.selectedBuildTargetGroup;
            for ( int i = 0; i < MODULES.Length; i++ ) {
                var module = MODULES[i];
                if ( module.Enabled ) {
                    if ( StartWithCurrent && module.IsTarget(tCurrent) ) {
                        tBuildOrder.Insert(0, module.BuildGame);
                    } else {
                        tBuildOrder.Add(module.BuildGame);
                    }
                }
            }

            if ( EndWithCurrent ) {
                for ( int i = 0; i < MODULES.Length; i++ ) {
                    var module = MODULES[i];
                    if ( module.Enabled ) {
                        if ( module.IsTarget(tCurrent) ) {
                            tBuildOrder.Remove(module.BuildGame);
                            tBuildOrder.Add(module.BuildGame);
                        }
                    }
                }
            }

            foreach ( Action<bool> tBuild in tBuildOrder )
                tBuild(aDevelopment);

            if ( SwitchToCurrent ) {
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(tCurrentGroup, tCurrent);
            }
        }
#if UNITY_2018_1_OR_NEWER
        public static BuildReport BuildGame(BuildTarget aTarget, string aPath, bool aDevelopment = false)
#else
        public static string BuildGame(BuildTarget aTarget, bool aDevelopment = false)
#endif
        {
            // Build player
            List<string> tScenes = new List<string>();

            for ( int i = 0; i < EditorBuildSettings.scenes.Length; i++ ) {
                if ( EditorBuildSettings.scenes[i].enabled )
                    tScenes.Add(EditorBuildSettings.scenes[i].path);
            }

            BuildPlayerOptions tOptions = new BuildPlayerOptions {
                locationPathName = aPath,
                target = aTarget,
                options = (aDevelopment ? BuildOptions.Development | BuildOptions.AllowDebugging
                    : BuildOptions.None),
                scenes = tScenes.ToArray(),
            };

            // Pre processors
            RunPreProcessor(aTarget, aDevelopment);

#if UNITY_2018_1_OR_NEWER
            BuildReport tReport = BuildPipeline.BuildPlayer(tOptions);
            Debug.Log("Build " + aTarget + ": " + tReport.summary.result);
            if ( tReport.summary.result == BuildResult.Succeeded )
#else
            string tReport = BuildPipeline.BuildPlayer(tOptions);
            Debug.Log("Build " + aTarget + ": " + tReport);
            if (string.IsNullOrEmpty(tReport))
#endif
            {
                EditorUserBuildSettings.SetBuildLocation(aTarget, tOptions.locationPathName);
                EditorUtility.RevealInFinder(tOptions.locationPathName);
            }

            // Post processors
            RunPostProcessor(aTarget, aDevelopment, tReport);

            return tReport;
        }

        // Currently not working
        static void RunPreProcessor(BuildTarget aTarget, bool aDevelopment) {
            foreach ( Type type in AttributeFinder.GetTypesWithAttribute<BuildPreProcessAttribute>(AppDomain.CurrentDomain) ) {
                if ( type.IsSubclassOf(typeof(IBuildPreProcessor)) )
                    ((IBuildPreProcessor)Activator.CreateInstance(type)).PreProcess(aTarget, aDevelopment);
            }
        }

#if UNITY_2018_1_OR_NEWER
        static void RunPostProcessor(BuildTarget aTarget, bool aDevelopment, BuildReport aReport)
#else
        static void RunPostProcessor(BuildTarget aTarget, bool aDevelopment, string aReport)
#endif
        {
            foreach ( Type type in AttributeFinder.GetTypesWithAttribute<BuildPostProcessAttribute>(AppDomain.CurrentDomain) ) {
                if ( type.IsSubclassOf(typeof(IBuildPostProcessor)) )
                    ((IBuildPostProcessor)Activator.CreateInstance(type)).PostProcess(aTarget, aDevelopment, aReport);
            }
        }
    }
}
