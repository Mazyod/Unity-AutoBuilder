using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Autobuilder
{
    public class ProcessConsoleWindow : EditorWindow
    {
        const int MAX = 10000;

        string m_Content;
        Vector2 m_ScrollPos;
        GUIStyle m_LabelStyle;
        GUIStyle LabelStyle
        {
            get
            {
                if (m_LabelStyle == null)
                {
                    m_LabelStyle = new GUIStyle("Label");
                    m_LabelStyle.wordWrap = true;
                }
                return m_LabelStyle;
            }
        }
        Process m_Process;
        bool m_ScrollToBottom;
        bool m_OnBottom;

        public static ProcessConsoleWindow NewWindow(Process aProcess = null, string aTitle = "")
        {
            ProcessConsoleWindow tWindow = CreateInstance<ProcessConsoleWindow>();
            if (string.IsNullOrEmpty(aTitle))
            {
                if (aProcess != null)
                    aTitle = Path.GetFileNameWithoutExtension(aProcess.StartInfo.FileName);
                else
                    aTitle = "Console";
            }
            tWindow.titleContent = new GUIContent(aTitle);
            tWindow.Clear();
            if (aProcess != null)
                tWindow.AddProcess(aProcess);
            tWindow.m_OnBottom = true;
            tWindow.Show();
            return tWindow;
        }

        void OnDestroy()
        {
            if (m_Process != null)
            {
                m_Process.Close();
            }
        }

        public void AddProcess(Process aProcess, string aIdentifier)
        {
            if (m_Process != null)
                m_Process.OutputDataReceived -= OutputHandler;
            m_Process = aProcess;
            WriteLine("> " + aProcess.StartInfo.FileName + " " + aProcess.StartInfo.Arguments);
            aProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    WriteLine("[" + aIdentifier + "] " + e.Data);
            };
        }

        public void AddProcess(Process aProcess)
        {
            if (m_Process != null)
                m_Process.OutputDataReceived -= OutputHandler;
            m_Process = aProcess;
            WriteLine("> " + aProcess.StartInfo.FileName + " " + aProcess.StartInfo.Arguments);
            aProcess.OutputDataReceived += OutputHandler;
        }

        void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
                WriteLine(e.Data);
        }

        public void Write(string aContent)
        {
            m_Content += aContent;
            if (m_Content.Length > MAX)
                m_Content = m_Content.Substring(m_Content.Length - MAX);
            if (m_OnBottom)
                m_ScrollToBottom = true;
            Repaint();
        }

        public void WriteLine(string aLine)
        {
            Write(aLine + "\n");
        }

        public void Clear()
        {
            m_Content = "";
        }

        void OnGUI()
        {
            Rect tPos = new Rect(0, 0, Screen.width, Screen.height - 10);
            GUIContent tContent = new GUIContent(m_Content);
            
            Rect tView = new Rect(5, 5,
                Screen.width - 5, LabelStyle.CalcHeight(tContent, Screen.width) + 5);
            
            // handle auto scroll
            m_OnBottom = m_ScrollPos.y > tView.height - tPos.height - 20;
            if (m_ScrollToBottom)
            {
                m_ScrollPos.y = tView.height;
                m_ScrollToBottom = false;
            }
            
            m_ScrollPos = GUI.BeginScrollView(tPos, m_ScrollPos, tView);
            EditorGUI.SelectableLabel(tView, m_Content, LabelStyle);
            
            GUI.EndScrollView();
        }
    }
}