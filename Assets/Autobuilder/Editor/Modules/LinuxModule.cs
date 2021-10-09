using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Autobuilder {
    public class LinuxModule : BuildModule {
        const string BUILD_LINUX_32 = "BuildLinux32";
        const string BUILD_LINUX_64 = "BuildLinux64";
        const string BUILD_LINUX_UNIVERSAL = "BuildLinuxUniversal";

        public override BuildTargetGroup TargetGroup { get { return BuildTargetGroup.Standalone; } }
        public override BuildTarget Target { get { return BuildTarget.StandaloneLinux64; } }

        public bool BuildLinux32 {
            get { return GetBool(BUILD_LINUX_32, true); }
            set { SetBool(BUILD_LINUX_32, value); }
        }
        public bool BuildLinux64 {
            get { return GetBool(BUILD_LINUX_64, false); }
            set { SetBool(BUILD_LINUX_64, value); }
        }
        public bool BuildLinuxUniversal {
            get { return GetBool(BUILD_LINUX_UNIVERSAL, false); }
            set { SetBool(BUILD_LINUX_UNIVERSAL, value); }
        }

        public override bool IsTarget(BuildTarget aTarget) {
#if UNITY_2019_2_OR_NEWER
            return aTarget == BuildTarget.StandaloneLinux64;
#else
            return aTarget == BuildTarget.StandaloneLinux
                || aTarget == BuildTarget.StandaloneLinux64
                || aTarget == BuildTarget.StandaloneLinuxUniversal;
#endif
        }

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

#if !UNITY_2019_2_OR_NEWER
            if ( BuildLinux32 ) {
                tReport = Builder.BuildGame(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux,
                    GetBuildPath(false, false, aDevelopment), aDevelopment);
#if UNITY_2018_1_OR_NEWER
                if ( tReport.summary.result != BuildResult.Succeeded )
                    tResult = BuildResult.Failed;
#else
                tResult += tReport;
#endif
            }
#endif
            if (BuildLinux64) {
                tReport = Builder.BuildGame(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64,
                    GetBuildPath(true, false, development), GetScenesList(), development);
#if UNITY_2018_1_OR_NEWER
                if (tReport.summary.result != BuildResult.Succeeded)
                    tResult = BuildResult.Failed;
#else
                tResult += tReport;
#endif
            }

#if !UNITY_2019_2_OR_NEWER
            if ( BuildLinuxUniversal ) {
                tReport = BuildGame(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinuxUniversal,
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
            if (tResult != BuildResult.Succeeded)
#else
            if ( !string.IsNullOrEmpty(tResult) )
#endif
            {
                return false;
            }
            return true;
        }

        public string GetBuildPath(bool x64bits, bool aUniversal, bool aDevelopment) {
            string tPath = BaseBuildPath;

            if (x64bits) {
                tPath += Builder.BUILD_64;
            } else if (aUniversal) {
                tPath += Builder.BUILD_UNIVERSAL;
            }

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

        public override void OptionsGUI(out bool build, out bool development) {
            build = false;
            development = false;

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
