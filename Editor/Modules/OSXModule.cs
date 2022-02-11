using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Autobuilder {
    public class OSXModule : BuildModule {
        const string BUILD_OSX_32 = Builder.BUILDER + "BuildOSX32";
        const string BUILD_OSX_64 = Builder.BUILDER + "BuildOSX64";
        const string BUILD_OSX_UNIVERSAL = Builder.BUILDER + "BuildOSXUniversal";
        const string BUILD_DIR_OSX = "/OSX";

        public override BuildTargetGroup TargetGroup { get { return BuildTargetGroup.Standalone; } }
        public override BuildTarget Target { get { return BuildTarget.StandaloneOSX; } }

        public bool BuildOSX32 {
            get { return GetBool(BUILD_OSX_32, true); }
            set { SetBool(BUILD_OSX_32, value); }
        }
        public bool BuildOSX64 {
            get { return GetBool(BUILD_OSX_64, true); }
            set { SetBool(BUILD_OSX_64, value); }
        }
        public bool BuildOSXUniversal {
            get { return GetBool(BUILD_OSX_UNIVERSAL, true); }
            set { SetBool(BUILD_OSX_UNIVERSAL, value); }
        }

        public override bool IsTarget(BuildTarget aTarget) {
#if UNITY_2017_3_OR_NEWER
            return aTarget == BuildTarget.StandaloneOSX;
#else
            return aTarget == BuildTarget.StandaloneOSXIntel
                || aTarget == BuildTarget.StandaloneOSXIntel64
                || aTarget == BuildTarget.StandaloneOSXUniversal;
#endif
        }

        public override bool BuildGame(bool development = false) {
            if (!base.BuildGame(development)) return false;

            PlayerSettings.macOS.buildNumber = BuildNumber.ToString();
#if UNITY_2017_3_OR_NEWER
#   if UNITY_2018_1_OR_NEWER
            BuildReport tReport = Builder.BuildGame(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX,
                GetBuildPath(false, false, development), GetScenesList(), development);

            if ( tReport.summary.result != BuildResult.Succeeded )
#   else
            string tReport = Builder.BuildGame(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX,
                GetBuildPath(false, false, aDevelopment), aDevelopment);

            if ( !string.IsNullOrEmpty(tReport) )
#   endif
                return false;
#else
            string tError = "";
            
            if (BuildOSX32) {
                tError += Builder.BuildGame(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSXIntel,
                    GetBuildPath(false, false, aDevelopment), aDevelopment);
            }
            if (BuildOSX64) {
                tError += Builder.BuildGame(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSXIntel64,
                    GetBuildPath(true, false, aDevelopment), aDevelopment);
            }
            if (BuildOSXUniversal) {
                tError += Builder.BuildGame(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSXUniversal,
                    GetBuildPath(false, true, aDevelopment), aDevelopment);
            }
            if (!string.IsNullOrEmpty(tError)) {
                return false;
            }
#endif
            return true;
        }

        public string GetBuildPath(bool x64bits, bool aUniversal, bool aDevelopment) {
            string tPath = BaseBuildPath;

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

        public override void OptionsGUI(out bool build, out bool development) {
            build = false;
            development = false;

#if !UNITY_2017_3_OR_NEWER
            BuildOSX32 = EditorGUILayout.Toggle("Build 32 bit version", BuildOSX32);
#endif
            BuildOSX64 = EditorGUILayout.Toggle("Build 64 bit version", BuildOSX64);
#if !UNITY_2017_3_OR_NEWER
            BuildOSXUniversal = EditorGUILayout.Toggle("Build universal version", BuildOSXUniversal);
#endif
        }
    }
}
