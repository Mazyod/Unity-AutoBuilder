using System.IO;
using UnityEditor;

namespace Autobuilder {
    public class IOSModule : XCodeModule {
        const string BUILD_IOS = Builder.BUILDER + "BuildIOS";
        const string BUILD_DIR_IOS = "/iOS";
        const string DATA_PATH = "ProjectSettings/iOSBuildData.json";
        const string CAPABILITIES = "Capabilities";


        public override BuildTargetGroup TargetGroup { get { return BuildTargetGroup.iOS; } }
        public override BuildTarget Target { get { return BuildTarget.iOS; } }

        public override bool BuildGame(bool development = false) {
            PlayerSettings.iOS.buildNumber = BuildNumber.ToString();
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            return base.BuildGame(development);
        }

        public override string GetBuildPath(bool development) {
            string path = BaseBuildPath;

            if (development) {
                path += "_dev";
            }
            // Create directory if it doesn't exist
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    }
}
