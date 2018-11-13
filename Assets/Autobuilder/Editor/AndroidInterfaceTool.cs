using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Autobuilder
{
    class AndroidInterfaceTool
    {
        const string ANDROID_ADB = "AndroidADB";
        const string ADB_INSTALL_OPTIONS = "AndroidADBInstallOptions";
        const string ADB_RUN = "AndroidADBRun";
        const string DEFAULT_OPTIONS = "-r";
        const string FILE = "File/Android Tools/";
        const string INSTALL = "Install on Android devices";
        const string UNINSTALL = "Uninstall from Adnroid devices";
        const string LOGCAT = "Open logcat";

        static Process[] m_Processes;
        static string[] m_Devices;

        public static string AndroidADBPath
        {
            get
            {
                return EditorPrefs.GetString(ANDROID_ADB, AndroidSDKRoot + "/platform-tools/adb"
#if UNITY_EDITOR_WINDOWS
                    + ".exe";
#endif
                );
            }
            set
            {
                EditorPrefs.SetString(ANDROID_ADB, value);
            }
        }

        public static string ADBInstallOptions
        {
            get
            {
                return EditorPrefs.GetString(ADB_INSTALL_OPTIONS, DEFAULT_OPTIONS);
            }
            set
            {
                EditorPrefs.SetString(ADB_INSTALL_OPTIONS, value);
            }
        }

        public static string AndroidSDKRoot
        {
            get
            {
                return EditorPrefs.GetString("AndroidSdkRoot");
            }
        }

        public static bool ADBRun
        {
            get
            {
                return EditorPrefs.GetBool(ADB_RUN, true);
            }
            set
            {
                EditorPrefs.SetBool(ADB_RUN, value);
            }
        }

        [PreferenceItem("Android Tools")]
        public static void PreferenceGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("ADB path");
            EditorGUI.BeginChangeCheck();
            string tPath = EditorGUILayout.TextField(AndroidADBPath);
            if (EditorGUI.EndChangeCheck())
                AndroidADBPath = tPath;

            if (GUILayout.Button("...", GUILayout.MaxWidth(25)))
            {
                string tNewPath = EditorUtility.OpenFilePanel("Select adb.exe", AndroidSDKRoot, "");
                if (tNewPath != null && tNewPath != "")
                    AndroidADBPath = tNewPath;
            }
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            string tOptions = EditorGUILayout.TextField("ADB install options", ADBInstallOptions);
            if (EditorGUI.EndChangeCheck())
                ADBInstallOptions = tOptions;

            EditorGUI.BeginChangeCheck();
            bool tRun = EditorGUILayout.Toggle("Run after installing", ADBRun);
            if (EditorGUI.EndChangeCheck())
                ADBRun = tRun;
        }

        [MenuItem(FILE + INSTALL)]
        public static void InstallLastBuild()
        {
            InstallToDevice(EditorUserBuildSettings.GetBuildLocation(BuildTarget.Android));
        }
        public static void InstallToDevice(string aPath)
        {
            //string tPath = EditorUserBuildSettings.GetBuildLocation(BuildTarget.Android);
            string tADBPath = AndroidADBPath;
            if (File.Exists(tADBPath) && File.Exists(aPath))
            {
                Process tProc = new Process();
                tProc.StartInfo.FileName = tADBPath;
                tProc.StartInfo.Arguments = "devices";
                tProc.StartInfo.UseShellExecute = false;
                tProc.StartInfo.RedirectStandardOutput = true;
                tProc.Start();

                string output = tProc.StandardOutput.ReadToEnd();
                tProc.WaitForExit();

                List<string> tDevices = new List<string>();
                string[] tOut = output.Split('\n', '\r');
                for (int i = 0; i < tOut.Length; i++)
                {
                    string tLine = tOut[i];
                    if (tLine.Contains("\t"))
                        tDevices.Add(tLine.Split('\t')[0]);
                }
                m_Devices = tDevices.ToArray();

                m_Processes = new Process[m_Devices.Length];
                for (int i = 0; i < m_Devices.Length; i++)
                {
                    string tDevice = m_Devices[i];
                    /*
                    Process tProc1 = new Process();
                    tProc1.StartInfo.FileName = tADBPath;
                    tProc1.StartInfo.UseShellExecute = false;
                    tProc1.StartInfo.RedirectStandardOutput = true;
                    tProc1.StartInfo.Arguments = "-s " + tDevice + " uninstall " + PlayerSettings.applicationIdentifier;

                    tProc1.EnableRaisingEvents = false;
                    tProc1.Start();
                    tProc1.WaitForExit();
                    bool tSucess = tProc1.StandardOutput.ReadToEnd().ToLower().Contains("success");
                    UnityEngine.Debug.Log(tDevice + " Uninstall " + (tSucess ? "successful" : "failed"));
                    */

                    Process tProc2 = new Process();
                    m_Processes[i] = tProc2;
                    tProc2.StartInfo.FileName = tADBPath;
                    tProc2.StartInfo.UseShellExecute = false;
                    tProc2.StartInfo.Arguments = "-s " + tDevice + " install " + ADBInstallOptions + " " + aPath;
                    tProc2.StartInfo.RedirectStandardOutput = true;
                    tProc2.StartInfo.CreateNoWindow = true;
                    tProc2.EnableRaisingEvents = true;
                    ProcessConsoleWindow tConsole = ProcessConsoleWindow.NewWindow(tProc2,
                        tDevice);
                    tConsole.WriteLine("> Installing on device " + tDevice);
                    if (ADBRun)
                        tProc2.Exited += new EventHandler(CreateExitDelegate(i, tADBPath, PlayerSettings.applicationIdentifier));
                    tProc2.Start();
                    tProc2.BeginOutputReadLine();
                }
            }
        }

        [MenuItem(FILE + INSTALL, validate = true)]
        static bool InstallToDeviceValidation()
        {
            string tPath = EditorUserBuildSettings.GetBuildLocation(BuildTarget.Android);
            return File.Exists(tPath) && File.Exists(AndroidADBPath);
        }

        [MenuItem(FILE + UNINSTALL)]
        static void Uninstall()
        {
            string tPath = EditorUserBuildSettings.GetBuildLocation(BuildTarget.Android);
            string tADBPath = AndroidADBPath;
            if (File.Exists(tADBPath) && File.Exists(tPath))
            {

                Process tProc = new Process();
                tProc.StartInfo.FileName = tADBPath;
                tProc.StartInfo.Arguments = "devices";
                tProc.StartInfo.UseShellExecute = false;
                tProc.StartInfo.RedirectStandardOutput = true;
                tProc.Start();

                string output = tProc.StandardOutput.ReadToEnd();
                tProc.WaitForExit();

                List<string> tDevices = new List<string>();
                string[] tOut = output.Split('\n', '\r');
                for (int i = 0; i < tOut.Length; i++)
                {
                    string tLine = tOut[i];
                    if (tLine.Contains("\t"))
                        tDevices.Add(tLine.Split('\t')[0]);
                }
                m_Devices = tDevices.ToArray();

                m_Processes = new Process[m_Devices.Length];
                for (int i = 0; i < m_Devices.Length; i++)
                {
                    string tDevice = m_Devices[i];
                    Process tProc1 = new Process();
                    tProc1.StartInfo.FileName = tADBPath;
                    tProc1.StartInfo.UseShellExecute = false;
                    tProc1.StartInfo.RedirectStandardOutput = true;
                    tProc1.StartInfo.Arguments = "-s " + tDevice + " uninstall " + PlayerSettings.applicationIdentifier;
                    tProc1.StartInfo.CreateNoWindow = true;
                    tProc1.EnableRaisingEvents = false;
                    tProc1.Start();
                    tProc1.WaitForExit();
                    bool tSucess = tProc1.StandardOutput.ReadToEnd().ToLower().Contains("success");
                    UnityEngine.Debug.Log(tDevice + " Uninstall " + (tSucess ? "successful" : "failed"));
                }
            }
        }

        [MenuItem(FILE + LOGCAT)]
        static void OpenLog()
        {
            string tADBPath = AndroidADBPath;
            Process tProc = new Process();
            tProc.StartInfo.FileName = tADBPath;
            tProc.StartInfo.UseShellExecute = false;
            tProc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            tProc.StartInfo.CreateNoWindow = true;
            tProc.StartInfo.RedirectStandardOutput = true;
            /*
            // Clean log
            tProc.StartInfo.Arguments = "logcat -c";
            tProc.Start();
            tProc.Close();
            */
            // Start logging
            tProc.StartInfo.Arguments = "logcat -s Unity";

            ProcessConsoleWindow.NewWindow(tProc, "Logcat");
            tProc.Start();
            tProc.BeginOutputReadLine();
        }

        [MenuItem(FILE + UNINSTALL, validate = true)]
        [MenuItem(FILE + LOGCAT, validate = true)]
        static bool ValidateUninstall()
        {
            return File.Exists(AndroidADBPath);
        }

        static Action<object, EventArgs> CreateExitDelegate(int aProc, string adb, string bundleIdentifier)
        {
            return (object sender, EventArgs e) => 
            {
                string tDevice = m_Devices[aProc];
                //UnityEngine.Debug.Log(tDevice + " Install process finished");

                Process tProc1 = new Process();
                tProc1.StartInfo.FileName = adb;
                tProc1.StartInfo.UseShellExecute = false;
                tProc1.StartInfo.Arguments = "-s " + tDevice + " shell monkey -p " + bundleIdentifier + " -c android.intent.category.LAUNCHER 1 ";
                tProc1.Start();
            };
        }
    }
}
