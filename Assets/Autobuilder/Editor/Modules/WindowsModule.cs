using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Autobuilder {
    public class WindowsModule : IBuildModule {
        const string WIN_BUILD_NUMBER = Builder.BUILDER + "WindowsBuildNumber";
        const string BUILD_WIN_32 = Builder.BUILDER + "BuildWin32";
        const string BUILD_WIN_64 = Builder.BUILDER + "BuildWin64";
        const string BUILD_DIR_WINDOWS = "/Windows";

        public string Name { get { return "Windows"; } }
        public BuildTargetGroup TargetGroup { get { return BuildTargetGroup.Standalone; } }
        public BuildTarget Target { get { return BuildTarget.StandaloneWindows64; } }

        public static bool BuildWin32 {
            get { return EditorProjectPrefs.GetBool(BUILD_WIN_32, true); }
            set { EditorProjectPrefs.SetBool(BUILD_WIN_32, value); }
        }
        public static bool BuildWin64 {
            get { return EditorProjectPrefs.GetBool(BUILD_WIN_64, false); }
            set { EditorProjectPrefs.SetBool(BUILD_WIN_64, value); }
        }
        public bool Enabled {
            get { return BuildWin32 || BuildWin64; }
            set {
                BuildWin32 = true;
                BuildWin64 = true;
            }
        }

        public int BuildNumber {
            get { return EditorProjectPrefs.GetInt(WIN_BUILD_NUMBER, 0); }
            set {
                EditorProjectPrefs.SetInt(WIN_BUILD_NUMBER, value);
                EditorProjectPrefs.Save();
            }
        }

        public bool IsTarget(BuildTarget aTarget) {
            return aTarget == BuildTarget.StandaloneWindows
                || aTarget == BuildTarget.StandaloneWindows64;
        }

        public bool BuildGame(bool aDevelopment = false) {
            // Add build number
            if ( Enabled ) {
#if UNITY_2018_1_OR_NEWER
                BuildResult tResult = BuildResult.Succeeded;
                BuildReport tReport;
#else
                string tResult = "";
                string tReport;
#endif
                if ( BuildWin32 ) {
                    tReport = Builder.BuildGame(BuildTarget.StandaloneWindows,
                        GetBuildPath(false, aDevelopment), aDevelopment);
#if UNITY_2018_1_OR_NEWER
                    if ( tReport.summary.result != BuildResult.Succeeded )
                        tResult = BuildResult.Failed;
#else
                    tResult += tReport;
#endif
                }

                if ( BuildWin64 ) {
                    tReport = Builder.BuildGame(BuildTarget.StandaloneWindows64,
                        GetBuildPath(true, aDevelopment), aDevelopment);
#if UNITY_2018_1_OR_NEWER
                    if ( tReport.summary.result != BuildResult.Succeeded )
                        tResult = BuildResult.Failed;
#else
                    tResult += tReport;
#endif
                }

#if UNITY_2018_1_OR_NEWER
                if ( tResult != BuildResult.Succeeded )
#else
                if ( !string.IsNullOrEmpty(tResult) )
#endif
                {
                    return false;
                }
            }
            return false;
        }

        public string GetBuildPath(bool x64bits, bool aDevelopment) {
            string tPath = Builder.DataPath + "/" + Builder.BuildPath
                + BUILD_DIR_WINDOWS;

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

        public void OnGUI(out bool aBuild, out bool aDevelopment) {
            aBuild = false;
            aDevelopment = false;
            BuildWin32 = EditorGUILayout.Toggle("Build 32 bit version", BuildWin32);
            BuildWin64 = EditorGUILayout.Toggle("Build 64 bit version", BuildWin64);
        }
    }
}
