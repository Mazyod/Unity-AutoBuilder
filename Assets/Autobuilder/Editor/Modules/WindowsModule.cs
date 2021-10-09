using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Autobuilder {
    public class WindowsModule : BuildModule {
        const string WIN_BUILD_NUMBER = Builder.BUILDER + "WindowsBuildNumber";
        const string BUILD_WIN_32 = Builder.BUILDER + "BuildWin32";
        const string BUILD_WIN_64 = Builder.BUILDER + "BuildWin64";

        public override BuildTargetGroup TargetGroup { get { return BuildTargetGroup.Standalone; } }
        public override BuildTarget Target { get { return BuildTarget.StandaloneWindows64; } }

        public bool BuildWin32 {
            get { return GetBool(BUILD_WIN_32, false); }
            set { SetBool(BUILD_WIN_32, value); }
        }
        public bool BuildWin64 {
            get { return GetBool(BUILD_WIN_64, true); }
            set { SetBool(BUILD_WIN_64, value); }
        }

        public override bool IsTarget(BuildTarget aTarget) {
            return aTarget == BuildTarget.StandaloneWindows
                || aTarget == BuildTarget.StandaloneWindows64;
        }

        public override bool BuildGame(bool development = false) {
            if (!base.BuildGame(development)) return false;

#if UNITY_2018_1_OR_NEWER
                BuildResult tResult = BuildResult.Succeeded;
                BuildReport tReport;
#else
                string tResult = "";
                string tReport;
#endif
                if ( BuildWin32 ) {
                    tReport = Builder.BuildGame(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows,
                        GetBuildPath(false, development), GetScenesList(), development);
#if UNITY_2018_1_OR_NEWER
                    if ( tReport.summary.result != BuildResult.Succeeded )
                        tResult = BuildResult.Failed;
#else
                    tResult += tReport;
#endif
                }

                if ( BuildWin64 ) {
                    tReport = Builder.BuildGame(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64,
                        GetBuildPath(true, development), GetScenesList(), development);
#if UNITY_2018_1_OR_NEWER
                    if ( tReport.summary.result != BuildResult.Succeeded )
                        tResult = BuildResult.Failed;
#else
                    tResult += tReport;
#endif
                }

#if UNITY_2018_1_OR_NEWER
                return tResult == BuildResult.Succeeded;
#else
                return string.IsNullOrEmpty(tResult);
#endif
        }

        public string GetBuildPath(bool x64bits, bool aDevelopment) {
            string tPath = BaseBuildPath;

            if ( x64bits ) {
                tPath += Builder.BUILD_64;
            }

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
            tPath += "/" + Builder.FileName + ".exe";

            return tPath;
        }

        public override void OptionsGUI(out bool build, out bool development) {
            build = false;
            development = false;
            BuildWin32 = EditorGUILayout.Toggle("Build 32 bit version", BuildWin32);
            BuildWin64 = EditorGUILayout.Toggle("Build 64 bit version", BuildWin64);
        }
    }
}
