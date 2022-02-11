using System.IO;
using UnityEditor;

namespace Autobuilder {
    public class TVOSModule : XCodeModule {
        const string BUILD = Builder.BUILDER + "BuildTvOS";
        const string BUILD_DIR = "/tvOS";
        const string DATA_PATH = "ProjectSettings/tvOSBuildData.json";

        public override BuildTargetGroup TargetGroup { get { return BuildTargetGroup.tvOS; } }
        public override BuildTarget Target { get { return BuildTarget.tvOS; } }

        public override bool BuildGame(bool aDevelopment = false) {
            PlayerSettings.tvOS.sdkVersion = tvOSSdkVersion.Device;
            PlayerSettings.tvOS.buildNumber = BuildNumber.ToString();
            return base.BuildGame(aDevelopment);
        }

        public override string GetBuildPath(bool aDevelopment) {
            string tPath = BaseBuildPath;

            if ( aDevelopment ) {
                tPath += "_dev";
            }
            // Create directory if it doesn't exist
            if ( !Directory.Exists(tPath) ) {
                Directory.CreateDirectory(tPath);
            }

            return tPath;
        }
    }
}
