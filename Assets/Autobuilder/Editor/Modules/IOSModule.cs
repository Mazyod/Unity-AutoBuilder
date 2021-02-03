using System.IO;
using UnityEditor;

namespace Autobuilder {
    public class IOSModule : XCodeModule {
        const string BUILD_IOS = Builder.BUILDER + "BuildIOS";
        const string BUILD_DIR_IOS = "/iOS";
        const string DATA_PATH = "ProjectSettings/iOSBuildData.json";
        const string CAPABILITIES = "Capabilities";


        public override string Name { get { return "iOS"; } }
        public override BuildTargetGroup TargetGroup { get { return BuildTargetGroup.iOS; } }
        public override BuildTarget Target { get { return BuildTarget.iOS; } }

        public override bool Enabled {
            get { return EditorProjectPrefs.GetBool(BUILD_IOS, true); }
            set { EditorProjectPrefs.SetBool(BUILD_IOS, value); }
        }

        public override int BuildNumber {
            get {
                int tVersion = 0;
                int.TryParse(PlayerSettings.iOS.buildNumber, out tVersion);
                return tVersion;
            }
            set {
                PlayerSettings.iOS.buildNumber = value.ToString();
            }
        }
        protected override string DataPath { get { return DATA_PATH; } }

        public override bool BuildGame(bool aDevelopment = false) {
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            return base.BuildGame(aDevelopment);
        }

        public override string GetBuildPath(bool aDevelopment) {
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
    }
}
