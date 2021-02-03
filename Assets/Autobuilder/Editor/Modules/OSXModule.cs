using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Autobuilder {
    public class OSXModule : IBuildModule {
        const string BUILD_OSX_32 = Builder.BUILDER + "BuildOSX32";
        const string BUILD_OSX_64 = Builder.BUILDER + "BuildOSX64";
        const string BUILD_OSX_UNIVERSAL = Builder.BUILDER + "BuildOSXUniversal";
        const string BUILD_DIR_OSX = "/OSX";

        public string Name { get { return "OSX"; } }
        public BuildTargetGroup TargetGroup { get { return BuildTargetGroup.Standalone; } }
        public BuildTarget Target { get { return BuildTarget.StandaloneOSX; } }

        public static bool BuildOSX32 {
            get { return EditorProjectPrefs.GetBool(BUILD_OSX_32, true); }
            set { EditorProjectPrefs.SetBool(BUILD_OSX_32, value); }
        }
        public static bool BuildOSX64 {
            get { return EditorProjectPrefs.GetBool(BUILD_OSX_64, false); }
            set { EditorProjectPrefs.SetBool(BUILD_OSX_64, value); }
        }
        public static bool BuildOSXUniversal {
            get { return EditorProjectPrefs.GetBool(BUILD_OSX_UNIVERSAL, false); }
            set { EditorProjectPrefs.SetBool(BUILD_OSX_UNIVERSAL, value); }
        }
        public bool Enabled {
            get { return BuildOSX32 || BuildOSX64 || BuildOSXUniversal; }
            set {
                BuildOSX32 = true;
                BuildOSX64 = true;
                BuildOSXUniversal = true;
            }
        }

        public int BuildNumber {
            get {
                if ( int.TryParse(PlayerSettings.macOS.buildNumber, out int tVersion) ) {
                    return tVersion;
                }
                return 0;
            }
            set {
                PlayerSettings.macOS.buildNumber = value.ToString();
            }
        }

        public bool IsTarget(BuildTarget aTarget) {
#if UNITY_2017_3_OR_NEWER
            return aTarget == BuildTarget.StandaloneOSX;
#else
            return aTarget == BuildTarget.StandaloneOSXIntel
                || aTarget == BuildTarget.StandaloneOSXIntel64
                || aTarget == BuildTarget.StandaloneOSXUniversal;
#endif
        }

        public bool BuildGame(bool aDevelopment = false) {
            // Add build number
            if ( !Enabled ) return false;

#if UNITY_2017_3_OR_NEWER
#   if UNITY_2018_1_OR_NEWER
            BuildReport tReport = Builder.BuildGame(BuildTarget.StandaloneOSX,
                GetBuildPath(false, false, aDevelopment), aDevelopment);

            if ( tReport.summary.result != BuildResult.Succeeded )
#   else
            string tReport = BuildGame(BuildTarget.StandaloneOSX,
                GetBuildPath(false, false, aDevelopment), aDevelopment);

            if ( !string.IsNullOrEmpty(tReport) )
#   endif
                return false;
#else
            string tError = "";
            
            if (BuildOSX32) {
                tError += BuildGame(BuildTarget.StandaloneOSXIntel,
                    GetBuildPath(false, false, aDevelopment), aDevelopment);
            }
            if (BuildOSX64) {
                tError += BuildGame(BuildTarget.StandaloneOSXIntel64,
                    GetBuildPath(true, false, aDevelopment), aDevelopment);
            }
            if (BuildOSXUniversal) {
                tError += BuildGame(BuildTarget.StandaloneOSXUniversal,
                    GetBuildPath(false, true, aDevelopment), aDevelopment);
            }
            if (!string.IsNullOrEmpty(tError)) {
                return false;
            }
#endif
            return true;
        }

        public string GetBuildPath(bool x64bits, bool aUniversal, bool aDevelopment) {
            string tPath = Builder.DataPath + "/" + Builder.BuildPath
                + BUILD_DIR_OSX;

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
