using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Autobuilder
{
    /// <summary>
    /// Custom Editor GUI Layout tools
    /// </summary>
    public static class CustomEditorGUILayout
    {
        static readonly Color SEED = new Color(0.361f, 0.875f, 0.608f);
        const float INCREMENT = 0.5f;
        static Dictionary<int, GUIStyle> m_TabStyles;
        static Dictionary<int, GUIStyle> m_BoxStyles;

        /// <summary>
        /// Get unique tab style
        /// </summary>
        /// <param name="i">Tab index</param>
        /// <returns></returns>
        public static GUIStyle GetTabStyle(int i)
        {
            if (m_TabStyles == null)
                m_TabStyles = new Dictionary<int, GUIStyle>();
            if (!m_TabStyles.ContainsKey(i))
            {
                GUIStyle tBoxStyle = new GUIStyle(GUI.skin.box);
                Texture2D tTex = new Texture2D(1, 1);
                tTex.SetPixel(0, 0, GetColor(i));
                tTex.Apply();
                tBoxStyle.normal.background = tTex;
                tBoxStyle.margin.bottom = 0;
                if (i > 0)
                    tBoxStyle.margin.left = 0;
                if (m_TabStyles.ContainsKey(i - 1))
                {
                    m_TabStyles[i - 1].margin.right = 0;
                }
                m_TabStyles.Add(i, tBoxStyle);
            }

            return m_TabStyles[i];
        }

        /// <summary>
        /// Get unique box style
        /// </summary>
        /// <param name="i">Box index</param>
        /// <returns></returns>
        public static GUIStyle GetBoxStyle(int i)
        {
            if (m_BoxStyles == null)
                m_BoxStyles = new Dictionary<int, GUIStyle>();
            if (!m_BoxStyles.ContainsKey(i))
            {
                GUIStyle tBoxStyle = new GUIStyle(GUI.skin.box);
                Texture2D tTex = new Texture2D(1, 1);
                tTex.SetPixel(0, 0, GetColor(i));
                tTex.Apply();
                tBoxStyle.normal.background = tTex;
                tBoxStyle.margin.top = 0;
                m_BoxStyles.Add(i, tBoxStyle);
            }

            return m_BoxStyles[i];
        }

        /// <summary>
        /// Get unique color
        /// </summary>
        /// <param name="i">Style index</param>
        /// <param name="aSeed">Color seed</param>
        /// <returns></returns>
        public static Color GetColor(int i, Color aSeed)
        {
            float value = (aSeed.r + aSeed.g + aSeed.b) / 3;
            float newValue = value + i * INCREMENT;
            float valueRatio = newValue / value;
            Color newColor = new Color();
            newColor.r = aSeed.r * valueRatio;
            newColor.g = aSeed.g * valueRatio;
            newColor.b = aSeed.b * valueRatio;
            newColor.a = 1;
            return newColor;
        }

        /// <summary>
        /// Get unique color
        /// </summary>
        /// <param name="i">Style index</param>
        /// <returns></returns>
        public static Color GetColor(int i)
        {
            return GetColor(i, SEED);
        }

        /// <summary>
        /// Custom field to load a file
        /// </summary>
        /// <param name="aLabel">Field name</param>
        /// <param name="aPath">Current path</param>
        /// <param name="aTitle">Title</param>
        /// <param name="aFolder">Default folder</param>
        /// <param name="aExtension">Extension</param>
        /// <returns></returns>
        public static string FilePathField(string aLabel, string aPath, string aTitle, string aFolder, string aExtension)
        {
            GUILayout.BeginHorizontal();
            string tFilePath = EditorGUILayout.TextField(aLabel, aPath);
            if (GUILayout.Button("...", GUILayout.MaxWidth(25)))
            {
                string tPath = EditorUtility.OpenFilePanel(aTitle, aFolder, aExtension);
                if (tPath != "")
                    tFilePath = tPath;
            }
            GUILayout.EndHorizontal();
            return tFilePath;
        }

        /// <summary>
        /// Custom field to save file
        /// </summary>
        /// <param name="aLabel">Field name</param>
        /// <param name="aPath">Current path</param>
        /// <param name="aTitle">Window title</param>
        /// <param name="aFolder">Default folder</param>
        /// <param name="aDefaultName">Default file name</param>
        /// <param name="aExtension">Extension</param>
        /// <returns></returns>
        public static string SaveFilePathField(string aLabel, string aPath, string aTitle, string aFolder, string aDefaultName, string aExtension)
        {
            GUILayout.BeginHorizontal();
            string tFilePath = EditorGUILayout.TextField(aLabel, aPath);
            if (GUILayout.Button("...", GUILayout.MaxWidth(25)))
            {
                tFilePath = EditorUtility.SaveFilePanel(aTitle, aFolder, aDefaultName, aExtension);
            }
            GUILayout.EndHorizontal();
            return tFilePath;
        }

        /// <summary>
        /// Custom field to select a directory
        /// </summary>
        /// <param name="aLabel">Field name</param>
        /// <param name="aPath">Current path</param>
        /// <param name="aTitle">Window title</param>
        /// <param name="aDefaultName">Default dir name</param>
        /// <returns>Selected path or empty string if canceled</returns>
        public static string DirPathField(GUIContent aLabel, string aPath,
            string aTitle, string aDefaultName)
        {
            GUILayout.BeginHorizontal();
            string tFilePath = EditorGUILayout.TextField(aLabel, aPath);
            if (GUILayout.Button("...", GUILayout.MaxWidth(25)))
            {
                string tPath = EditorUtility.OpenFolderPanel(aTitle, aPath,
                    aDefaultName);
                if (tPath != "")
                    tFilePath = tPath;
            }
            GUILayout.EndHorizontal();
            return tFilePath;
        }

        /// <summary>
        /// Custom field to select a directory
        /// </summary>
        /// <param name="aLabel">Field name</param>
        /// <param name="aPath">Current path</param>
        /// <param name="aTitle">Window title</param>
        /// <param name="aDefaultName">Default dir name</param>
        /// <returns>Selected path or empty string if canceled</returns>
        public static string DirPathField(string aLabel, string aPath,
            string aTitle, string aDefaultName)
        {
            return DirPathField(new GUIContent(aLabel), aPath, aTitle,
                aDefaultName);
        }


        /// <summary>
        /// Drop area
        /// </summary>
        /// <param name="aText">Text inside area</param>
        /// <param name="aHeight">Box height</param>
        /// <param name="aCallback">Callback function called after object is dropped</param>
        /// <returns></returns>
        public static bool DropAreaGUI(string aText, float aHeight, System.Action<Object> aCallback)
        {
            Event evt = Event.current;
            Rect drop_area = Box(aHeight, aText);
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return false;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (Object dragged_object in DragAndDrop.objectReferences)
                        {
                            aCallback(dragged_object);
                        }
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Draw tabs
        /// </summary>
        /// <param name="aNames">List of tab names</param>
        /// <param name="aDrawCallbacks">List of callbacks which draw the tabs</param>
        /// <param name="aSelected">Selected tab</param>
        /// <returns></returns>
        public static int DrawTabs(string[] aNames, System.Action[] aDrawCallbacks, int aSelected)
        {
            if (aNames.Length != aDrawCallbacks.Length)
            {
                Box(30f, "Names list and callbacks list must be the same length");
                return 0;
            }

            int tTab = GUILayout.Toolbar(aSelected, aNames);

            aDrawCallbacks[aSelected]();
            return tTab;
        }

        /// <summary>
        /// Draw a box
        /// </summary>
        /// <param name="aHeight"></param>
        /// <param name="aText"></param>
        /// <returns></returns>
        public static Rect Box(float aHeight, string aText)
        {
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, aHeight, GUILayout.ExpandWidth(true));
            drop_area.width -= 10;
            drop_area.x += 5;
            GUIStyle tStyle = new GUIStyle(GUI.skin.box);
            tStyle.alignment = TextAnchor.MiddleCenter;
            tStyle.richText = true;
            tStyle.fontSize = 10;
            GUI.Box(drop_area, aText, tStyle);
            return drop_area;
        }
    }
}
