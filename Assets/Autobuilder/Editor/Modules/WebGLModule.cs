using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Autobuilder {
    public class WebGLModule : IBuildModule {
        const string WEBGL_BUILD_NUMBER = Builder.BUILDER + "WebGLBuildNumber";
        const string BUILD_WEBGL = Builder.BUILDER + "BuildWebGL";
        const string BUILD_DIR_WEBGL = "/WebGL";

        public string Name => "WebGL";

        public BuildTarget Target => BuildTarget.WebGL;

        public BuildTargetGroup TargetGroup => BuildTargetGroup.WebGL;

        public bool Enabled {
            get { return EditorProjectPrefs.GetBool(BUILD_WEBGL, true); }
            set { EditorProjectPrefs.SetBool(BUILD_WEBGL, value); }

        }
        public int BuildNumber {
            get { return EditorProjectPrefs.GetInt(WEBGL_BUILD_NUMBER); }
            set { EditorProjectPrefs.SetInt(WEBGL_BUILD_NUMBER, value); }
        }

        public bool BuildGame(bool aDevelopment = false) {
            if (!Enabled) return false;

            // Build Game
#if UNITY_2018_1_OR_NEWER
            BuildResult tResult = BuildResult.Succeeded;
            BuildReport tReport;
#else
            string tResult = "";
            string tReport;
#endif
            tReport = Builder.BuildGame(
                BuildTarget.WebGL,
                GetBuildPath(aDevelopment),
                aDevelopment
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

        public bool IsTarget(BuildTarget aTarget) {
            return aTarget == Target;
        }

        public void OnGUI(out bool aBuild, out bool aDevelopment) {
            aBuild = false;
            aDevelopment = false;

            Enabled = EditorGUILayout.Toggle("Build WebGL", Enabled);
        }

        public string GetBuildPath(bool aDevelopment) {
            string tPath = Builder.DataPath + "/" + Builder.BuildPath
                + BUILD_DIR_WEBGL;

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