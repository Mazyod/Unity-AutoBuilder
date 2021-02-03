using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Autobuilder {
    public class LinuxModule : IBuildModule {
        const string LINUX_BUILD_NUMBER = Builder.BUILDER + "LinuxBuildNumber";
        const string BUILD_LINUX_32 = Builder.BUILDER + "BuildLinux32";
        const string BUILD_LINUX_64 = Builder.BUILDER + "BuildLinux64";
        const string BUILD_LINUX_UNIVERSAL = Builder.BUILDER + "BuildLinuxUniversal";
        const string BUILD_DIR_LINUX = "/Linux";

        public string Name { get { return "Linux"; } }
        public BuildTargetGroup TargetGroup { get { return BuildTargetGroup.Standalone; } }
        public BuildTarget Target { get { return BuildTarget.StandaloneLinux64; } }

        public static bool BuildLinux32 {
            get { return EditorProjectPrefs.GetBool(BUILD_LINUX_32, true); }
            set { EditorProjectPrefs.SetBool(BUILD_LINUX_32, value); }
        }
        public static bool BuildLinux64 {
            get { return EditorProjectPrefs.GetBool(BUILD_LINUX_64, false); }
            set { EditorProjectPrefs.SetBool(BUILD_LINUX_64, value); }
        }
        public static bool BuildLinuxUniversal {
            get { return EditorProjectPrefs.GetBool(BUILD_LINUX_UNIVERSAL, false); }
            set { EditorProjectPrefs.SetBool(BUILD_LINUX_UNIVERSAL, value); }
        }
        public bool Enabled {
            get { return BuildLinux32 || BuildLinux64 || BuildLinuxUniversal; }
            set {
                BuildLinux32 = true;
                BuildLinux64 = true;
                BuildLinuxUniversal = true;
            }
        }

        public int BuildNumber {
            get { return EditorProjectPrefs.GetInt(LINUX_BUILD_NUMBER, 0); }
            set {
                EditorProjectPrefs.SetInt(LINUX_BUILD_NUMBER, value);
                EditorProjectPrefs.Save();
            }
        }

        public bool IsTarget(BuildTarget aTarget) {
#if UNITY_2019_2_OR_NEWER
            return aTarget == BuildTarget.StandaloneLinux64;
#else
            return aTarget == BuildTarget.StandaloneLinux
                || aTarget == BuildTarget.StandaloneLinux64
                || aTarget == BuildTarget.StandaloneLinuxUniversal;
#endif
        }

        public bool BuildGame(bool aDevelopment = false) {
            if ( !Enabled ) return false;

            // Build Game
#if UNITY_2018_1_OR_NEWER
            BuildResult tResult = BuildResult.Succeeded;
            BuildReport tReport;
#else
            string tResult = "";
            string tReport;
#endif

#if !UNITY_2019_2_OR_NEWER
            if ( BuildLinux32 ) {
                tReport = Builder.BuildGame(BuildTarget.StandaloneLinux,
                    GetBuildPath(false, false, aDevelopment), aDevelopment);
#if UNITY_2018_1_OR_NEWER
                if ( tReport.summary.result != BuildResult.Succeeded )
                    tResult = BuildResult.Failed;
#else
                tResult += tReport;
#endif
            }
#endif
            if ( BuildLinux64 ) {
                tReport = Builder.BuildGame(BuildTarget.StandaloneLinux64,
                    GetBuildPath(true, false, aDevelopment), aDevelopment);
#if UNITY_2018_1_OR_NEWER
                if ( tReport.summary.result != BuildResult.Succeeded )
                    tResult = BuildResult.Failed;
#else
                tResult += tReport;
#endif
            }

#if !UNITY_2019_2_OR_NEWER
            if ( BuildLinuxUniversal ) {
                tReport = BuildGame(BuildTarget.StandaloneLinuxUniversal,
                    GetBuildPath(false, true, aDevelopment), aDevelopment);
#if UNITY_2018_1_OR_NEWER
                if ( tReport.summary.result != BuildResult.Succeeded )
                    tResult = BuildResult.Failed;
#else
                tResult += tReport;
#endif
            }
#endif
#if UNITY_2018_1_OR_NEWER
            if ( tResult != BuildResult.Succeeded )
#else
            if ( !string.IsNullOrEmpty(tResult) )
#endif
            {
                return false;
            }
            return true;
        }

        public string GetBuildPath(bool x64bits, bool aUniversal, bool aDevelopment) {
            string tPath = Builder.DataPath + "/" + Builder.BuildPath
                + BUILD_DIR_LINUX;

            if ( x64bits ) {
                tPath += Builder.BUILD_64;
            } else if ( aUniversal ) {
                tPath += Builder.BUILD_UNIVERSAL;
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
            tPath += "/" + Builder.FileName;

            return tPath;
        }

        public void OnGUI(out bool aBuild, out bool aDevelopment) {
            aBuild = false;
            aDevelopment = false;

#if !UNITY_2019_2_OR_NEWER
            BuildLinux32 = EditorGUILayout.Toggle("Build 32 bit version",
                BuildLinux32);
#endif
            BuildLinux64 = EditorGUILayout.Toggle("Build 64 bit version",
                BuildLinux64);
#if !UNITY_2019_2_OR_NEWER
            BuildLinuxUniversal = EditorGUILayout.Toggle("Build universal version",
                BuildLinuxUniversal);
#endif
        }
    }
}
