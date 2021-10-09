using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Autobuilder {
    public class WebGLModule : BuildModule {
        const string WEBGL_BUILD_NUMBER = Builder.BUILDER + "WebGLBuildNumber";
        const string BUILD_WEBGL = Builder.BUILDER + "BuildWebGL";
        const string BUILD_DIR_WEBGL = "/WebGL";


        public override BuildTarget Target => BuildTarget.WebGL;

        public override BuildTargetGroup TargetGroup => BuildTargetGroup.WebGL;

        public override bool BuildGame(bool development = false) {
            if (!base.BuildGame(development)) return false;

            // Build Game
#if UNITY_2018_1_OR_NEWER
            BuildResult tResult = BuildResult.Succeeded;
            BuildReport tReport;
#else
            string tResult = "";
            string tReport;
#endif
            tReport = Builder.BuildGame(
                BuildTargetGroup.WebGL,
                BuildTarget.WebGL,
                GetBuildPath(development),
                GetScenesList(),
                development
            );
#if UNITY_2018_1_OR_NEWER
            if (tReport.summary.result != BuildResult.Succeeded) {
                tResult = BuildResult.Failed;
            }
#else
            tResult += tReport;
#endif
#if UNITY_2018_1_OR_NEWER
            if (tResult != BuildResult.Succeeded)
#else
            if (!string.IsNullOrEmpty(tResult))
#endif
            {
                return false;
            }
            return true;
        }

        public override bool IsTarget(BuildTarget aTarget) {
            return aTarget == Target;
        }

        public override void OptionsGUI(out bool build, out bool development) {
            build = false;
            development = false;
        }

        public string GetBuildPath(bool aDevelopment) {
            string tPath = BaseBuildPath;

            if (aDevelopment) {
                tPath += "/dev";
            } else {
                tPath += "/b" + BuildNumber;
            }
            // Create directory if it doesn't exist
            if (!Directory.Exists(tPath)) {
                Directory.CreateDirectory(tPath);
            }

            // File
            tPath += "/" + Builder.FileName;

            return tPath;
        }
    }
}