using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Autobuilder {
    public class TVOSModule : XCodeModule {
        const string BUILD = Builder.BUILDER + "BuildTvOS";
        const string BUILD_DIR = "/tvOS";
        const string DATA_PATH = "ProjectSettings/tvOSBuildData.json";

        public override string Name { get { return "tvOS"; } }
        public override BuildTargetGroup TargetGroup { get { return BuildTargetGroup.tvOS; } }
        public override BuildTarget Target { get { return BuildTarget.tvOS; } }

        public override bool Enabled {
            get { return EditorProjectPrefs.GetBool(BUILD, true); }
            set { EditorProjectPrefs.SetBool(BUILD, value); }
        }

        public override int BuildNumber {
            get {
                int tVersion = 0;
                int.TryParse(PlayerSettings.tvOS.buildNumber, out tVersion);
                return tVersion;
            }
            set {
                PlayerSettings.tvOS.buildNumber = value.ToString();
            }
        }
        protected override string DataPath { get { return DATA_PATH; } }

        public override bool BuildGame(bool aDevelopment = false) {
            PlayerSettings.tvOS.sdkVersion = tvOSSdkVersion.Device;
            return base.BuildGame(aDevelopment);
        }

        public override string GetBuildPath(bool aDevelopment) {
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
    }
}
