using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;

namespace Autobuilder
{
    public class Builder : EditorWindow
    {
        const string BUILDER = "Autobuilder";
        const string BUILDS_PATH = BUILDER + "BuildsPath";
        const string FILENAME = BUILDER + "BuildFilename";
        const string WIN_BUILD_NUMBER = BUILDER + "WindowsBuildNumber";
        const string LINUX_BUILD_NUMBER = BUILDER + "LinuxBuildNumber";
        const string BUILD_WIN_32 = BUILDER + "BuildWin32";
        const string BUILD_WIN_64 = BUILDER + "BuildWin64";
        const string BUILD_LINUX_32 = BUILDER + "BuildLinux32";
        const string BUILD_LINUX_64 = BUILDER + "BuildLinux64";
        const string BUILD_LINUX_UNIVERSAL = BUILDER + "BuildLinuxUniversal";
        const string BUILD_OSX_32 = BUILDER + "BuildOSX32";
        const string BUILD_OSX_64 = BUILDER + "BuildOSX64";
        const string BUILD_OSX_UNIVERSAL = BUILDER + "BuildOSXUniversal";
        const string BUILD_ANDROID = BUILDER + "BuildAndroid";
        const string BUILD_IOS = BUILDER + "BuildIOS";
        const string RUN_ANDROID = BUILDER + "RunAndroid";
        const string ANDROID_KEYSTORE_PASS = BUILDER + "AndroidKeystorePass";
        const string ANDROID_KEYALIAS_PASS = BUILDER + "AndroidKeyAliasPass";
        const string START_WITH_CURRENT = BUILDER + "StartWithCurrent";
        const string END_WITH_CURRENT = BUILDER + "EndWithCurrent";
        const string SWITCH_TO_CURRENT = BUILDER + "SwitchToCurrent";
        const string DEFAULT_BUILDS_PATH = "Builds";
        const string BUILD_64 = "_x86_64";
        const string BUILD_UNIVERSAL = "Universal";
        const string BUILD_DIR_WINDOWS = "/Windows";
        const string BUILD_DIR_LINUX = "/Linux";
        const string BUILD_DIR_OSX = "/OSX";
        const string BUILD_DIR_ANDROID = "/Android";
        const string BUILD_DIR_IOS = "/iOS";

        #region EditorPrefs
        public static string BuildPath
        {
            get
            {
                return EditorProjectPrefs.GetString(BUILDS_PATH, DEFAULT_BUILDS_PATH);
            }
            set { EditorProjectPrefs.SetString(BUILDS_PATH, value); }
        }

        public static string FileName
        {
            get
            {
                return EditorProjectPrefs.GetString(FILENAME,
                    PlayerSettings.productName);
            }
            set { EditorProjectPrefs.SetString(FILENAME, value); }
        }

        public static int WindowsBuildNumber
        {
            get { return EditorProjectPrefs.GetInt(WIN_BUILD_NUMBER, 0); }
            set { EditorProjectPrefs.SetInt(WIN_BUILD_NUMBER, value); }
        }

        public static int LinuxBuildNumber
        {
            get { return EditorProjectPrefs.GetInt(LINUX_BUILD_NUMBER, 0); }
            set { EditorProjectPrefs.SetInt(LINUX_BUILD_NUMBER, value); }
        }

        public static int OSXBuildNumber
        {
            get
            {
                int tVersion = 0;
                int.TryParse(PlayerSettings.macOS.buildNumber, out tVersion);
                return tVersion;
            }
            set
            {
                PlayerSettings.macOS.buildNumber = value.ToString();
            }
        }

        public static int IOSBuildNumber
        {
            get
            {
                int tVersion = 0;
                int.TryParse(PlayerSettings.iOS.buildNumber, out tVersion);
                return tVersion;
            }
            set
            {
                PlayerSettings.iOS.buildNumber = value.ToString();
            }
        }

        public static string AndroidKeyAliasPass
        {
            get { return EditorProjectPrefs.GetString(ANDROID_KEYALIAS_PASS, PlayerSettings.Android.keyaliasPass); }
            set { EditorProjectPrefs.SetString(ANDROID_KEYALIAS_PASS, value); }
        }
        public static string AndroidKeyStorePass
        {
            get { return EditorProjectPrefs.GetString(ANDROID_KEYSTORE_PASS, PlayerSettings.Android.keystorePass); }
            set { EditorProjectPrefs.SetString(ANDROID_KEYSTORE_PASS, value); }
        }

        // Bools
        public static bool BuildWin32
        {
            get { return EditorProjectPrefs.GetBool(BUILD_WIN_32, true); }
            set { EditorProjectPrefs.SetBool(BUILD_WIN_32, value); }
        }
        public static bool BuildWin64
        {
            get { return EditorProjectPrefs.GetBool(BUILD_WIN_64, false); }
            set { EditorProjectPrefs.SetBool(BUILD_WIN_64, value); }
        }
        public static bool BuildLinux32
        {
            get { return EditorProjectPrefs.GetBool(BUILD_LINUX_32, true); }
            set { EditorProjectPrefs.SetBool(BUILD_LINUX_32, value); }
        }
        public static bool BuildLinux64
        {
            get { return EditorProjectPrefs.GetBool(BUILD_LINUX_64, false); }
            set { EditorProjectPrefs.SetBool(BUILD_LINUX_64, value); }
        }
        public static bool BuildLinuxUniversal
        {
            get { return EditorProjectPrefs.GetBool(BUILD_LINUX_UNIVERSAL, false); }
            set { EditorProjectPrefs.SetBool(BUILD_LINUX_UNIVERSAL, value); }
        }
        public static bool BuildOSX32
        {
            get { return EditorProjectPrefs.GetBool(BUILD_OSX_32, true); }
            set { EditorProjectPrefs.SetBool(BUILD_OSX_32, value); }
        }
        public static bool BuildOSX64
        {
            get { return EditorProjectPrefs.GetBool(BUILD_OSX_64, false); }
            set { EditorProjectPrefs.SetBool(BUILD_OSX_64, value); }
        }
        public static bool BuildOSXUniversal
        {
            get { return EditorProjectPrefs.GetBool(BUILD_OSX_UNIVERSAL, false); }
            set { EditorProjectPrefs.SetBool(BUILD_OSX_UNIVERSAL, value); }
        }
        public static bool BuildAndroid
        {
            get { return EditorProjectPrefs.GetBool(BUILD_ANDROID, true); }
            set { EditorProjectPrefs.SetBool(BUILD_ANDROID, value); }
        }
        public static bool BuildIOS
        {
            get { return EditorProjectPrefs.GetBool(BUILD_IOS, true); }
            set { EditorProjectPrefs.SetBool(BUILD_IOS, value); }
        }
        public static bool RunAndroid
        {
            get { return EditorProjectPrefs.GetBool(RUN_ANDROID, true); }
            set { EditorProjectPrefs.SetBool(RUN_ANDROID, value); }
        }
        public static bool StartWithCurrent
        {
            get { return EditorProjectPrefs.GetBool(START_WITH_CURRENT, true); }
            set { EditorProjectPrefs.SetBool(START_WITH_CURRENT, value); }
        }
        public static bool EndWithCurrent
        {
            get { return EditorProjectPrefs.GetBool(END_WITH_CURRENT, false); }
            set { EditorProjectPrefs.SetBool(END_WITH_CURRENT, value); }
        }
        public static bool SwitchToCurrent
        {
            get { return EditorProjectPrefs.GetBool(SWITCH_TO_CURRENT, true); }
            set { EditorProjectPrefs.SetBool(SWITCH_TO_CURRENT, value); }
        }
        #endregion

        static Vector2 m_ScrollPos;
        static string m_DataPath;
        static string DataPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_DataPath))
                    m_DataPath = Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
                return m_DataPath;
            }
        }
        GUIStyle m_AreaStyle;
        GUIStyle AreaStyle
        {
            get
            {
                if (m_AreaStyle == null || m_AreaStyle.normal.background == null)
                {
                    m_AreaStyle = new GUIStyle(EditorStyles.helpBox);
                }
                return m_AreaStyle;
            }
        }
        GUIStyle m_SelectedAreaStyle;
        Texture2D m_SelectedTexture;
        GUIStyle SelectedAreaStyle
        {
            get
            {
                if (m_SelectedAreaStyle == null)
                {
                    m_SelectedAreaStyle = new GUIStyle(AreaStyle);
                    m_SelectedTexture = new Texture2D(1, 1);
                    m_SelectedTexture.SetPixel(0, 0, new Color(0.423f, 0.498f, 0.431f));
                    m_SelectedTexture.Apply();
                    m_SelectedAreaStyle.normal.background = m_SelectedTexture;
                }
                if (m_SelectedAreaStyle.normal.background == null)
                {
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
        public static void ShowWindow()
        {
            GetWindow<Builder>("Builder");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            System.Action<bool> tBuildFunction = null;
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
            if (tStartWithCurrent && tEndWithCurrent)
            {
                if (!StartWithCurrent)
                    tEndWithCurrent = false;
                else if (!EndWithCurrent)
                    tStartWithCurrent = false;
            }
            SwitchToCurrent = GUILayout.Toggle(SwitchToCurrent, "Switch to current platform when done");
            
            m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos);
            GUILayout.Label("Standalone", EditorStyles.boldLabel);

            int tColumn0 = 70;
            int tColumn1 = 70;
            int tColumn1Half = tColumn1 / 2;

            // WINDOWS
            GUILayout.BeginVertical(
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ? SelectedAreaStyle : AreaStyle);
            GUILayout.BeginHorizontal();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows)
            {
                if (GUILayout.Button("Windows", GUILayout.MaxWidth(tColumn0)))
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone,
                        BuildTarget.StandaloneWindows);
            }
            else
                GUILayout.Label("Windows", GUILayout.MaxWidth(tColumn0));
            GUILayout.Label("Build:", GUILayout.MaxWidth(tColumn1Half));
            int tWindowsBuild = EditorGUILayout.IntField(WindowsBuildNumber, GUILayout.MaxWidth(tColumn1Half));
            if (GUILayout.Button("Build"))
            {
                tBuildFunction = BuildGameWindows;
            }
            if (GUILayout.Button("Development build"))
                BuildGameWindows(true);
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            BuildWin32 = EditorGUILayout.Toggle("Build 32 bit version", BuildWin32);
            BuildWin64 = EditorGUILayout.Toggle("Build 64 bit version", BuildWin64);
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            // LINUX
            GUILayout.BeginVertical(
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux ? SelectedAreaStyle : AreaStyle);
            GUILayout.BeginHorizontal();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneLinux)
            {
                if (GUILayout.Button("Linux", GUILayout.MaxWidth(tColumn0)))
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone,
                        BuildTarget.StandaloneLinux);
            }
            else
                GUILayout.Label("Linux", GUILayout.MaxWidth(tColumn0));
            GUILayout.Label("Build:", GUILayout.MaxWidth(tColumn1Half));
            int tLinuxBuild = EditorGUILayout.IntField(LinuxBuildNumber, GUILayout.MaxWidth(tColumn1Half));
            if (GUILayout.Button("Build"))
            {
                tBuildFunction = BuildGameLinux;
                tDevelopment = false;
            }
            if (GUILayout.Button("Development build"))
            {
                tBuildFunction = BuildGameLinux;
                tDevelopment = true;
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            BuildLinux32 = EditorGUILayout.Toggle("Build 32 bit version", BuildLinux32);
            BuildLinux64 = EditorGUILayout.Toggle("Build 64 bit version", BuildLinux64);
            BuildLinuxUniversal = EditorGUILayout.Toggle("Build universal version", BuildLinuxUniversal);
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            // OSX
            GUILayout.BeginVertical(
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX ? SelectedAreaStyle : AreaStyle);
            GUILayout.BeginHorizontal();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneOSX)
            {
                if (GUILayout.Button("OSX", GUILayout.MaxWidth(tColumn0)))
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone,
                        BuildTarget.StandaloneOSX);
            }
            else
                GUILayout.Label("OSX", GUILayout.MaxWidth(tColumn0));
            GUILayout.Label("Build:", GUILayout.MaxWidth(tColumn1Half));
            int tOSXBuild = EditorGUILayout.IntField(OSXBuildNumber, GUILayout.MaxWidth(tColumn1Half));
            if (GUILayout.Button("Build"))
            {
                tBuildFunction = BuildGameOSX;
                tDevelopment = false;
            }
            if (GUILayout.Button("Development build"))
            {
                tBuildFunction = BuildGameOSX;
                tDevelopment = true;
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            BuildOSX32 = EditorGUILayout.Toggle("Build 32 bit version", BuildOSX32);
            BuildOSX64 = EditorGUILayout.Toggle("Build 64 bit version", BuildOSX64);
            BuildOSXUniversal = EditorGUILayout.Toggle("Build universal version", BuildOSXUniversal);
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            GUILayout.Label("Mobile", EditorStyles.boldLabel);

            // ANDROID
            GUILayout.BeginVertical(
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? SelectedAreaStyle : AreaStyle);
            GUILayout.BeginHorizontal();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                if (GUILayout.Button("Android", GUILayout.MaxWidth(tColumn0)))
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android,
                        BuildTarget.Android);
            }
            else
                GUILayout.Label("Android", EditorStyles.boldLabel, GUILayout.MaxWidth(tColumn0));
            GUILayout.Label("Build:", GUILayout.MaxWidth(tColumn1Half));
            PlayerSettings.Android.bundleVersionCode = EditorGUILayout.IntField(
                    PlayerSettings.Android.bundleVersionCode, GUILayout.MaxWidth(tColumn1Half));
            if (GUILayout.Button("Build"))
            {
                BuildAndroid = true;
                tBuildFunction = BuildGameAndroid;
                tDevelopment = false;
            }
            if (GUILayout.Button("Development build"))
            {
                BuildAndroid = true;
                tBuildFunction = BuildGameAndroid;
                tDevelopment = true;
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            BuildAndroid = EditorGUILayout.Toggle("Build android version", BuildAndroid);
            GUILayout.BeginHorizontal();
            RunAndroid = EditorGUILayout.Toggle("Install and run on device", RunAndroid);
            if (GUILayout.Button("Install last version"))
                AndroidInterfaceTool.InstallLastBuild();
            GUILayout.EndHorizontal();
            string tAndroiddentifier = EditorGUILayout.TextField("Bundle identifier",
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));
            AndroidKeyStorePass = EditorGUILayout.TextField("Keystore password", AndroidKeyStorePass);
            AndroidKeyAliasPass = EditorGUILayout.TextField("Key alias password", AndroidKeyAliasPass);
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            // IOS
            GUILayout.BeginVertical(
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS ? SelectedAreaStyle : AreaStyle);
            GUILayout.BeginHorizontal();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
            {
                if (GUILayout.Button("iOS", GUILayout.MaxWidth(tColumn0)))
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.iOS,
                        BuildTarget.iOS);
            }
            else
                GUILayout.Label("iOS", EditorStyles.boldLabel, GUILayout.MaxWidth(tColumn0));
            GUILayout.Label("Build:", GUILayout.MaxWidth(tColumn1Half));
            IOSBuildNumber = EditorGUILayout.IntField(
                    IOSBuildNumber, GUILayout.MaxWidth(tColumn1Half));
            if (GUILayout.Button("Build"))
            {
                BuildIOS = true;
                tBuildFunction = BuildGameIOS;
                tDevelopment = false;
                PlayerSettings.iOS.sdkVersion =iOSSdkVersion.DeviceSDK;
            }
            if (GUILayout.Button("Development build"))
            {
                BuildIOS = true;
                tBuildFunction = BuildGameIOS;
                tDevelopment = true;
                PlayerSettings.iOS.sdkVersion =iOSSdkVersion.DeviceSDK;
            }
            if (GUILayout.Button("Simulator build"))
            {
                BuildIOS = true;
                tBuildFunction = BuildGameIOS;
                tDevelopment = true;
                PlayerSettings.iOS.sdkVersion =iOSSdkVersion.SimulatorSDK;
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            BuildIOS = EditorGUILayout.Toggle("Build iOS version", BuildIOS);
            string tIOSIdentifier = EditorGUILayout.TextField("Bundle identifier",
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS));
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                StartWithCurrent = tStartWithCurrent;
                EndWithCurrent = tEndWithCurrent;
                BuildPath = PathFunctions.GetRelativePath(tBuildPath, DataPath);
                WindowsBuildNumber = tWindowsBuild;
                LinuxBuildNumber = tLinuxBuild;
                OSXBuildNumber = tOSXBuild;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, tAndroiddentifier);
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, tIOSIdentifier);
                EditorProjectPrefs.Save();
            }
            GUILayout.EndScrollView();
            // Buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Player settings"))
            {
                EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
            }
            if (GUILayout.Button("Build settings"))
            {
                GetWindow<BuildPlayerWindow>();
            }
            if (GUILayout.Button("Build ALL"))
            {
                tBuildFunction = BuildGameAll;
                tDevelopment = false;
            }
            if (GUILayout.Button("Build ALL development"))
            {
                tBuildFunction = BuildGameAll;
                tDevelopment = true;
            }
            GUILayout.EndHorizontal();

            if (tBuildFunction != null)
            {
                tBuildFunction(tDevelopment);
            }
        }

        public static void BuildGameAll(bool aDevelopment)
        {
            List<System.Action<bool>> tBuildOrder = new List<System.Action<bool>>
            {
                BuildGameWindows,
                BuildGameLinux,
                BuildGameOSX,
                BuildGameAndroid,
            };
            BuildTarget tCurrent = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup tCurrentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (StartWithCurrent)
            {
                if (tCurrent == BuildTarget.Android && BuildAndroid)
                {
                    tBuildOrder.Remove(BuildGameAndroid);
                    tBuildOrder.Insert(0, BuildGameAndroid);
                }
                else if (TargetIsLinux(tCurrent) && (BuildLinux32 || BuildLinux64 || BuildLinuxUniversal))
                {
                    tBuildOrder.Remove(BuildGameLinux);
                    tBuildOrder.Insert(0, BuildGameLinux);
                }
                else if (TargetIsOSX(tCurrent) && (BuildOSX32 || BuildOSX64 || BuildOSXUniversal))
                {
                    tBuildOrder.Remove(BuildGameOSX);
                    tBuildOrder.Insert(0, BuildGameOSX);
                }
                else if (TargetIsWindows(tCurrent) && (BuildWin32 || BuildWin64))
                {
                    tBuildOrder.Remove(BuildGameWindows);
                    tBuildOrder.Insert(0, BuildGameWindows);
                }
            }
            if (EndWithCurrent)
            {
                if (tCurrent == BuildTarget.Android && BuildAndroid)
                {
                    tBuildOrder.Remove(BuildGameAndroid);
                    tBuildOrder.Add(BuildGameAndroid);
                }
                else if (TargetIsLinux(tCurrent) && (BuildLinux32 || BuildLinux64 || BuildLinuxUniversal))
                {
                    tBuildOrder.Remove(BuildGameLinux);
                    tBuildOrder.Add(BuildGameLinux);
                }
                else if (TargetIsOSX(tCurrent) && (BuildOSX32 || BuildOSX64 || BuildOSXUniversal))
                {
                    tBuildOrder.Remove(BuildGameOSX);
                    tBuildOrder.Add(BuildGameOSX);
                }
                else if (TargetIsWindows(tCurrent) && (BuildWin32 || BuildWin64))
                {
                    tBuildOrder.Remove(BuildGameWindows);
                    tBuildOrder.Add(BuildGameWindows);
                }
            }

            foreach (System.Action<bool> tBuild in tBuildOrder)
                tBuild(aDevelopment);

            if (SwitchToCurrent)
            {
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(tCurrentGroup, tCurrent);
            }
        }

        public static void BuildGameWindows(bool aDevelopment = false)
        {
            // Add build number
            if (BuildWin32 || BuildWin64)
            {
                if (!aDevelopment)
                    AddBuildNumber(BuildTarget.StandaloneWindows);
                EditorProjectPrefs.Save();
#if UNITY_2018_1_OR_NEWER
                BuildResult tResult = BuildResult.Succeeded;
                BuildReport tReport;
#else
                string tResult = "";
                string tReport;
#endif
                if (BuildWin32)
                {
                    tReport = BuildGame(BuildTarget.StandaloneWindows, aDevelopment);
#if UNITY_2018_1_OR_NEWER
                    if (tReport.summary.result != BuildResult.Succeeded)
                        tResult = BuildResult.Failed;
#else
                    tResult += tReport;
#endif
                }
                if (BuildWin64)
                {
                    tReport = BuildGame(BuildTarget.StandaloneWindows64, aDevelopment);
#if UNITY_2018_1_OR_NEWER
                    if (tReport.summary.result != BuildResult.Succeeded)
                        tResult = BuildResult.Failed;
#else
                    tResult += tReport;
#endif
                }
#if UNITY_2018_1_OR_NEWER
                if (tResult != BuildResult.Succeeded && !aDevelopment)
#else
                if (!string.IsNullOrEmpty(tResult) && !aDevelopment)
#endif
                    SubtractBuildNumber(BuildTarget.StandaloneWindows);
            }
        }

        public static void BuildGameOSX(bool aDevelopment = false)
        {
            // Add build number
            if (BuildOSX32 || BuildOSX64 || BuildOSXUniversal)
            {
#if UNITY_2017_3_OR_NEWER
                if (!aDevelopment)
                    AddBuildNumber(BuildTarget.StandaloneOSX);
                EditorProjectPrefs.Save();
#if UNITY_2018_1_OR_NEWER
                BuildReport tReport = BuildGame(BuildTarget.StandaloneOSX, aDevelopment);

                if (tReport.summary.result != BuildResult.Succeeded && !aDevelopment)
#else
                string tReport = BuildGame(BuildTarget.StandaloneOSX, aDevelopment);

                if (!string.IsNullOrEmpty(tReport) && !aDevelopment)
#endif
                    SubtractBuildNumber(BuildTarget.StandaloneOSX);
#else
                string tError = "";
                if (!aDevelopment)
                    AddBuildNumber(BuildTarget.StandaloneOSXIntel);
                EditorProjectPrefs.Save();
                if (BuildOSX32)
                    tError += BuildGame(BuildTarget.StandaloneOSXIntel, aDevelopment);
                if (BuildOSX64)
                    tError += BuildGame(BuildTarget.StandaloneOSXIntel64, aDevelopment);
                if (BuildOSXUniversal)
                    tError += BuildGame(BuildTarget.StandaloneOSXUniversal, aDevelopment);
                if (!string.IsNullOrEmpty(tError))
                    SubtractBuildNumber(BuildTarget.StandaloneOSXIntel);
#endif
            }
        }

        public static void BuildGameLinux(bool aDevelopment = false)
        {
            if (BuildLinux32 || BuildLinux64 || BuildLinuxUniversal)
            {
                // Add build number
                if (!aDevelopment)
                    AddBuildNumber(BuildTarget.StandaloneLinux);
                EditorProjectPrefs.Save();
                // Build Game
#if UNITY_2018_1_OR_NEWER
                BuildResult tResult = BuildResult.Succeeded;
                BuildReport tReport;
#else
                string tResult = "";
                string tReport;
#endif
                if (BuildLinux32)
                {
					tReport = BuildGame(BuildTarget.StandaloneLinux, aDevelopment);
#if UNITY_2018_1_OR_NEWER
                    if (tReport.summary.result != BuildResult.Succeeded)
                        tResult = BuildResult.Failed;
#else
                    tResult += tReport;
#endif

                }
                if (BuildLinux64)
                {
                    tReport = BuildGame(BuildTarget.StandaloneLinux64, aDevelopment);
#if UNITY_2018_1_OR_NEWER
                    if (tReport.summary.result != BuildResult.Succeeded)
                        tResult = BuildResult.Failed;
#else
                    tResult += tReport;
#endif
                }
                if (BuildLinuxUniversal)
                {
                    tReport = BuildGame(BuildTarget.StandaloneLinuxUniversal, aDevelopment);
#if UNITY_2018_1_OR_NEWER
                    if (tReport.summary.result != BuildResult.Succeeded)
                        tResult = BuildResult.Failed;
#else
                    tResult += tReport;
#endif
                }
#if UNITY_2018_1_OR_NEWER
                if (tResult != BuildResult.Succeeded && !aDevelopment)
#else
                if (!string.IsNullOrEmpty(tResult) && !aDevelopment)
#endif
                    SubtractBuildNumber(BuildTarget.StandaloneLinux);
            }
        }

        public static void BuildGameAndroid(bool aDevelopment = false)
        {
            if (BuildAndroid)
            {
                PlayerSettings.Android.keystorePass = AndroidKeyStorePass;
                PlayerSettings.Android.keyaliasPass = AndroidKeyAliasPass;
                // Add build number
                if (!aDevelopment)
                    AddBuildNumber(BuildTarget.Android);
                EditorProjectPrefs.Save();
                // Build Game
#if UNITY_2018_1_OR_NEWER
                BuildReport tReport = BuildGame(BuildTarget.Android, aDevelopment);
                if (tReport.summary.result == BuildResult.Succeeded)
#else
                string tReport = BuildGame(BuildTarget.Android, aDevelopment);
                if (string.IsNullOrEmpty(tReport))
#endif
                {
                    if (RunAndroid)
                        AndroidInterfaceTool.InstallToDevice(GetBuildPath(BuildTarget.Android, aDevelopment));
                }
                else if (!aDevelopment)
                    SubtractBuildNumber(BuildTarget.Android);
            }
        }

        public static void BuildGameIOS(bool aDevelopment = false)
        {
            if (BuildIOS)
            {
                if (!aDevelopment)
                    AddBuildNumber(BuildTarget.iOS);
                EditorProjectPrefs.Save();
                // Build Game
#if UNITY_2018_1_OR_NEWER
                BuildReport tReport = BuildGame(BuildTarget.iOS, aDevelopment);
                if (!aDevelopment && tReport.summary.result != BuildResult.Succeeded)
#else
                string tReport = BuildGame(BuildTarget.iOS, aDevelopment);
                if (!aDevelopment && !string.IsNullOrEmpty(tReport))
#endif
                    SubtractBuildNumber(BuildTarget.iOS);
            }
        }

#if UNITY_2018_1_OR_NEWER
        public static BuildReport BuildGame(BuildTarget aTarget, bool aDevelopment = false)
#else
        public static string BuildGame(BuildTarget aTarget, bool aDevelopment = false)
#endif
        {
            // Build player
            List<string> tScenes = new List<string>();

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                    tScenes.Add(EditorBuildSettings.scenes[i].path);
            }

            BuildPlayerOptions tOptions = new BuildPlayerOptions
            {
                locationPathName = GetBuildPath(aTarget, aDevelopment),
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
            if (tReport.summary.result == BuildResult.Succeeded)
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

        static void AddBuildNumber(BuildTarget aTarget, int aValue)
        {
            if (aTarget == BuildTarget.Android)
                PlayerSettings.Android.bundleVersionCode += aValue;
            else if (aTarget == BuildTarget.iOS)
                IOSBuildNumber += aValue;
            else if (TargetIsOSX(aTarget))
                OSXBuildNumber += aValue;
            else if (TargetIsWindows(aTarget))
                WindowsBuildNumber += aValue;
            else if (TargetIsLinux(aTarget))
                LinuxBuildNumber += aValue;
        }

        // Currently not working
        static void RunPreProcessor(BuildTarget aTarget, bool aDevelopment)
        {
            foreach (Type type in AttributeFinder.GetTypesWithAttribute<BuildPreProcessAttribute>(AppDomain.CurrentDomain))
            {
                if (type.IsSubclassOf(typeof(IBuildPreProcessor)))
                    ((IBuildPreProcessor)Activator.CreateInstance(type)).PreProcess(aTarget, aDevelopment);
            }
        }

#if UNITY_2018_1_OR_NEWER
        static void RunPostProcessor(BuildTarget aTarget, bool aDevelopment, BuildReport aReport)
#else
        static void RunPostProcessor(BuildTarget aTarget, bool aDevelopment, string aReport)
#endif
        {
            foreach (Type type in AttributeFinder.GetTypesWithAttribute<BuildPostProcessAttribute>(AppDomain.CurrentDomain))
            {
                if (type.IsSubclassOf(typeof(IBuildPostProcessor)))
                    ((IBuildPostProcessor)Activator.CreateInstance(type)).PostProcess(aTarget, aDevelopment, aReport);
            }
        }

        static void AddBuildNumber(BuildTarget aTarget)
        {
            AddBuildNumber(aTarget, 1);
        }

        static void SubtractBuildNumber(BuildTarget aTarget)
        {
            AddBuildNumber(aTarget, -1);
        }

        public static string GetBuildPath(BuildTarget aTarget, bool aDevelopment)
        {
            string tPath = DataPath + "/" + BuildPath;
            // Directory
            if (aTarget == BuildTarget.StandaloneWindows)
                tPath += BUILD_DIR_WINDOWS;
            else if (aTarget == BuildTarget.StandaloneWindows64)
                tPath += BUILD_DIR_WINDOWS + BUILD_64;
            else if (aTarget == BuildTarget.StandaloneLinux)
                tPath += BUILD_DIR_LINUX;
            else if (aTarget == BuildTarget.StandaloneLinux64)
                tPath += BUILD_DIR_LINUX + BUILD_64;
            else if (aTarget == BuildTarget.StandaloneLinuxUniversal)
                tPath += BUILD_DIR_LINUX + BUILD_UNIVERSAL;
#if UNITY_2017_3_OR_NEWER
            else if (aTarget == BuildTarget.StandaloneOSX)
                tPath += BUILD_DIR_OSX;
#else
            else if (aTarget == BuildTarget.StandaloneOSXIntel)
                tPath += BUILD_DIR_OSX;
            else if (aTarget == BuildTarget.StandaloneOSXIntel64)
                tPath += BUILD_DIR_OSX + BUILD_64;
            else if (aTarget == BuildTarget.StandaloneOSXUniversal)
                tPath += BUILD_DIR_OSX + BUILD_UNIVERSAL;
#endif
            else if (aTarget == BuildTarget.Android)
                tPath += BUILD_DIR_ANDROID;
            else if (aTarget == BuildTarget.iOS)
                tPath += BUILD_DIR_IOS;
            
            if (aTarget == BuildTarget.iOS)
            {
                if (aDevelopment)
                    tPath += "_simulator";
            }
            else
            {
                if (aDevelopment)
                    tPath += "/dev";
                else
                {
                    // Build version
                    tPath += "/b";
                    if (aTarget == BuildTarget.Android)
                        tPath += PlayerSettings.Android.bundleVersionCode;
                    else if (TargetIsOSX(aTarget))
                        tPath += PlayerSettings.macOS.buildNumber;
                    else if (TargetIsLinux(aTarget))
                        tPath += LinuxBuildNumber.ToString();
                    else if (TargetIsWindows(aTarget))
                        tPath += WindowsBuildNumber;
                }
            }

            if (!Directory.Exists(tPath))
                Directory.CreateDirectory(tPath);

            // File
            tPath += "/";
            if (aTarget == BuildTarget.Android)
                tPath += FileName + ".apk";
            else if (TargetIsOSX(aTarget))
                tPath += FileName;
            else if (TargetIsLinux(aTarget))
                tPath += FileName;
            else if (TargetIsWindows(aTarget))
                tPath += FileName + ".exe";

            return tPath;
        }

        static bool TargetIsWindows(BuildTarget aTarget)
        {
            return aTarget == BuildTarget.StandaloneWindows
                    || aTarget == BuildTarget.StandaloneWindows64;
        }

        static bool TargetIsLinux(BuildTarget aTarget)
        {
            return aTarget == BuildTarget.StandaloneLinux
                    || aTarget == BuildTarget.StandaloneLinux64
                    || aTarget == BuildTarget.StandaloneLinuxUniversal;
        }

        static bool TargetIsOSX(BuildTarget aTarget)
        {
#if UNITY_2017_3_OR_NEWER
            return aTarget == BuildTarget.StandaloneOSX;
#else
            return aTarget == BuildTarget.StandaloneOSXIntel
                    || aTarget == BuildTarget.StandaloneOSXIntel64
                    || aTarget == BuildTarget.StandaloneOSXUniversal;
#endif
        }
    }
}
