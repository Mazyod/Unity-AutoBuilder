using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Autobuilder {
    public class TVOSModule : IBuildModule {
        const string BUILD = Builder.BUILDER + "BuildTvOS";
        const string BUILD_DIR = "/tvOS";

        public string Name { get { return "tvOS"; } }
        public BuildTargetGroup TargetGroup { get { return BuildTargetGroup.tvOS; } }
        public BuildTarget Target { get { return BuildTarget.tvOS; } }

        public bool Enabled {
            get { return EditorProjectPrefs.GetBool(BUILD, true); }
            set { EditorProjectPrefs.SetBool(BUILD, value); }
        }

        public int BuildNumber {
            get {
                int tVersion = 0;
                int.TryParse(PlayerSettings.tvOS.buildNumber, out tVersion);
                return tVersion;
            }
            set {
                PlayerSettings.tvOS.buildNumber = value.ToString();
            }
        }

        public bool IsTarget(BuildTarget aTarget) {
            return aTarget == BuildTarget.tvOS;
        }

        public void BuildGame(bool aDevelopment = false) {
            if ( !aDevelopment ) {
                BuildNumber++;
                PlayerSettings.tvOS.sdkVersion = tvOSSdkVersion.Device;
            } else {
                PlayerSettings.tvOS.sdkVersion = tvOSSdkVersion.Simulator;
            }
            // Build Game
#if UNITY_2018_1_OR_NEWER
            BuildReport tReport = Builder.BuildGame(BuildTarget.tvOS,
                GetBuildPath(aDevelopment), aDevelopment);
            if ( !aDevelopment && tReport.summary.result != BuildResult.Succeeded )
#else
            string tReport = BuildGame(BuildTarget.tvOS,
                GetBuildPath(aDevelopment), aDevelopment);
            if (!aDevelopment && !string.IsNullOrEmpty(tReport))
#endif
                BuildNumber--;
        }


        public string GetBuildPath(bool aDevelopment) {
            string tPath = Builder.DataPath + "/" + Builder.BuildPath
                + BUILD_DIR;

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

            Enabled = EditorGUILayout.Toggle("Build tvOS version", Enabled);
            EditorGUI.BeginChangeCheck();
            string tTvOSIdentifier = EditorGUILayout.TextField("Bundle identifier",
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.tvOS));
            if ( EditorGUI.EndChangeCheck() ) {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.tvOS,
                    tTvOSIdentifier);
            }
        }
    }
}
