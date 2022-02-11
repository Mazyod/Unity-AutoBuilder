using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Autobuilder {
    public class AndroidModule : BuildModule {
        const string BUILD_ANDROID = "BuildAndroid";
        const string RUN_ANDROID = "RunAndroid";
        const string ANDROID_KEYSTORE_PASS = "AndroidKeystorePass";
        const string ANDROID_KEYALIAS_PASS = "AndroidKeyAliasPass";

        public override BuildTargetGroup TargetGroup { get { return BuildTargetGroup.Android; } }
        public override BuildTarget Target { get { return BuildTarget.Android; } }

        public bool RunAndroid {
            get { return GetBool(RUN_ANDROID, true); }
            set { SetBool(RUN_ANDROID, value); }
        }
        public string AndroidKeyAliasPass {
            get { return GetString(ANDROID_KEYALIAS_PASS, PlayerSettings.Android.keyaliasPass); }
            set { SetString(ANDROID_KEYALIAS_PASS, value); }
        }
        public string AndroidKeyStorePass {
            get { return GetString(ANDROID_KEYSTORE_PASS, PlayerSettings.Android.keystorePass); }
            set { SetString(ANDROID_KEYSTORE_PASS, value); }
        }

        public override bool IsTarget(BuildTarget aTarget) {
            return aTarget == BuildTarget.Android;
        }

        public override bool BuildGame(bool development = false) {
            if (!base.BuildGame(development)) return false;

            PlayerSettings.Android.keystorePass = AndroidKeyStorePass;
            PlayerSettings.Android.keyaliasPass = AndroidKeyAliasPass;
            PlayerSettings.Android.bundleVersionCode = BuildNumber;

            // Build Game
#if UNITY_2018_1_OR_NEWER
            BuildReport tReport = Builder.BuildGame(BuildTargetGroup.Android, BuildTarget.Android,
                GetBuildPath(development), GetScenesList(), development);
            if ( tReport.summary.result == BuildResult.Succeeded )
#else
            string tReport = Builder.BuildGame(BuildTargetGroup.Android, BuildTarget.Android,
                GetBuildPath(aDevelopment), aDevelopment);
            if (string.IsNullOrEmpty(tReport))
#endif
            {
                if ( RunAndroid ) {
                    AndroidInterfaceTool.InstallToDevice(
                        GetBuildPath(development));
                }
                return true;
            } else {
                return false;
            }
        }

        public string GetBuildPath(bool aDevelopment) {
            string tPath = BaseBuildPath;

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

        public override void OptionsGUI(out bool build, out bool development) {
            build = false;
            development = false;

            GUILayout.BeginHorizontal();
            RunAndroid = EditorGUILayout.Toggle("Install and run on device", RunAndroid);
            if ( GUILayout.Button("Install last version") )
                AndroidInterfaceTool.InstallLastBuild();
            GUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            string tAndroiddentifier = EditorGUILayout.TextField("Bundle identifier",
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));
            var keyStorePass = EditorGUILayout.TextField("Keystore password", AndroidKeyStorePass);
            var keyAliasPass = EditorGUILayout.TextField("Key alias password", AndroidKeyAliasPass);
            
            if ( EditorGUI.EndChangeCheck() ) {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,
                    tAndroiddentifier);
                AndroidKeyStorePass = keyStorePass;
                AndroidKeyAliasPass = keyAliasPass;
            }
        }
    }
}
