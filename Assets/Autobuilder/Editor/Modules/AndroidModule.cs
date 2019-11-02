using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Autobuilder {
    public class AndroidModule : IBuildModule {
        const string BUILD_ANDROID = Builder.BUILDER + "BuildAndroid";
        const string RUN_ANDROID = Builder.BUILDER + "RunAndroid";
        const string ANDROID_KEYSTORE_PASS = Builder.BUILDER + "AndroidKeystorePass";
        const string ANDROID_KEYALIAS_PASS = Builder.BUILDER + "AndroidKeyAliasPass";
        const string BUILD_DIR_ANDROID = "/Android";

        public string Name { get { return "Android"; } }
        public BuildTargetGroup TargetGroup { get { return BuildTargetGroup.Android; } }
        public BuildTarget Target { get { return BuildTarget.Android; } }

        public bool Enabled {
            get { return EditorProjectPrefs.GetBool(BUILD_ANDROID, true); }
            set { EditorProjectPrefs.SetBool(BUILD_ANDROID, value); }
        }
        public static bool RunAndroid {
            get { return EditorProjectPrefs.GetBool(RUN_ANDROID, true); }
            set { EditorProjectPrefs.SetBool(RUN_ANDROID, value); }
        }
        public static string AndroidKeyAliasPass {
            get { return EditorProjectPrefs.GetString(ANDROID_KEYALIAS_PASS, PlayerSettings.Android.keyaliasPass); }
            set { EditorProjectPrefs.SetString(ANDROID_KEYALIAS_PASS, value); }
        }
        public static string AndroidKeyStorePass {
            get { return EditorProjectPrefs.GetString(ANDROID_KEYSTORE_PASS, PlayerSettings.Android.keystorePass); }
            set { EditorProjectPrefs.SetString(ANDROID_KEYSTORE_PASS, value); }
        }

        public int BuildNumber {
            get { return PlayerSettings.Android.bundleVersionCode; }
            set { PlayerSettings.Android.bundleVersionCode = value; }
        }

        public bool IsTarget(BuildTarget aTarget) {
            return aTarget == BuildTarget.Android;
        }

        public void BuildGame(bool aDevelopment = false) {
            PlayerSettings.Android.keystorePass = AndroidKeyStorePass;
            PlayerSettings.Android.keyaliasPass = AndroidKeyAliasPass;
            // Add build number
            if ( !aDevelopment ) {
                BuildNumber++;
            }
            // Build Game
#if UNITY_2018_1_OR_NEWER
            BuildReport tReport = Builder.BuildGame(BuildTarget.Android,
                GetBuildPath(aDevelopment), aDevelopment);
            if ( tReport.summary.result == BuildResult.Succeeded )
#else
                string tReport = BuildGame(BuildTarget.Android,
                    GetBuildPath(aDevelopment), aDevelopment);
                if (string.IsNullOrEmpty(tReport))
#endif
            {
                if ( RunAndroid ) {
                    AndroidInterfaceTool.InstallToDevice(
                        GetBuildPath(aDevelopment));
                }
            } else if ( !aDevelopment )
                BuildNumber--;
        }

        public string GetBuildPath(bool aDevelopment) {
            string tPath = Builder.DataPath + "/" + Builder.BuildPath
                + BUILD_DIR_ANDROID;

            if ( aDevelopment ) {
                tPath += "/dev";
            } else {
                tPath += "/b" + BuildNumber;
            }
            // Create directory if it doesn't exist
            if ( !Directory.Exists(tPath) ) {
                Directory.CreateDirectory(tPath);
            }

            // File
            tPath += "/" + Builder.FileName + ".apk";

            return tPath;
        }

        public void OnGUI(out bool aBuild, out bool aDevelopment) {
            aBuild = false;
            aDevelopment = false;

            Enabled = EditorGUILayout.Toggle("Build android version", Enabled);
            GUILayout.BeginHorizontal();
            RunAndroid = EditorGUILayout.Toggle("Install and run on device", RunAndroid);
            if ( GUILayout.Button("Install last version") )
                AndroidInterfaceTool.InstallLastBuild();
            GUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            string tAndroiddentifier = EditorGUILayout.TextField("Bundle identifier",
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));
            if ( EditorGUI.EndChangeCheck() ) {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,
                    tAndroiddentifier);
            }
            AndroidKeyStorePass = EditorGUILayout.TextField("Keystore password", AndroidKeyStorePass);
            AndroidKeyAliasPass = EditorGUILayout.TextField("Key alias password", AndroidKeyAliasPass);
        }
    }
}
