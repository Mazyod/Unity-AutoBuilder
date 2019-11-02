using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Autobuilder {
    public class IOSModule : IBuildModule {
        const string BUILD_IOS = Builder.BUILDER + "BuildIOS";
        const string BUILD_DIR_IOS = "/iOS";

        public string Name { get { return "iOS"; } }
        public BuildTargetGroup TargetGroup { get { return BuildTargetGroup.iOS; } }
        public BuildTarget Target { get { return BuildTarget.iOS; } }

        public bool Enabled {
            get { return EditorProjectPrefs.GetBool(BUILD_IOS, true); }
            set { EditorProjectPrefs.SetBool(BUILD_IOS, value); }
        }

        public int BuildNumber {
            get {
                int tVersion = 0;
                int.TryParse(PlayerSettings.iOS.buildNumber, out tVersion);
                return tVersion;
            }
            set {
                PlayerSettings.iOS.buildNumber = value.ToString();
            }
        }

        public bool IsTarget(BuildTarget aTarget) {
            return aTarget == BuildTarget.iOS;
        }

        public void BuildGame(bool aDevelopment = false) {
            if ( !aDevelopment ) {
                BuildNumber++;
                PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            } else {
                PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
            }
            // Build Game
#if UNITY_2018_1_OR_NEWER
            BuildReport tReport = Builder.BuildGame(BuildTarget.iOS,
                GetBuildPath(aDevelopment), aDevelopment);
            if ( !aDevelopment && tReport.summary.result != BuildResult.Succeeded )
#else
            string tReport = BuildGame(BuildTarget.iOS,
                GetBuildPath(aDevelopment), aDevelopment);
            if (!aDevelopment && !string.IsNullOrEmpty(tReport))
#endif
                BuildNumber--;
        }


        public string GetBuildPath(bool aDevelopment) {
            string tPath = Builder.DataPath + "/" + Builder.BuildPath
                + BUILD_DIR_IOS;

            if ( aDevelopment ) {
                tPath += "_simulator";
            }
            // Create directory if it doesn't exist
            if ( !Directory.Exists(tPath) ) {
                Directory.CreateDirectory(tPath);
            }

            return tPath;
        }

        public void OnGUI(out bool aBuild, out bool aDevelopment) {
            aBuild = false;
            aDevelopment = false;

            Enabled = EditorGUILayout.Toggle("Build iOS version", Enabled);
            EditorGUI.BeginChangeCheck();
            string tIOSIdentifier = EditorGUILayout.TextField("Bundle identifier",
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS));
            if ( EditorGUI.EndChangeCheck() ) {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS,
                    tIOSIdentifier);
            }
        }
    }
}
