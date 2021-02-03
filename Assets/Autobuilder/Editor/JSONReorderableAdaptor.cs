using System;
using System.Collections.Generic;
using Autobuilder.SimpleJSON;
using RhoTools.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace Autobuilder {
    public class JSONFilesAdaptor : IReorderableListAdaptor {
        JSONArray array;

        public JSONFilesAdaptor(JSONArray array) {
            this.array = array;
        }

        public int Count => array.Count;

        public void Add() {
            array.Add("");
        }

        public void BeginGUI() {
        }

        public bool CanDrag(int index) {
            return true;
        }

        public bool CanRemove(int index) {
            return true;
        }

        public void Clear() {
            array.Clear();
        }

        static string FileSelect(Rect position, string value,
            string title, string folder, string extension, Action<string> onChange = null
        ) {
            var area = position;

            position.width -= 50;
            value = EditorGUI.TextField(position, value);
            position.x += position.width;
            position.width = 50;
            if (GUI.Button(position, "...")) {
                value = EditorUtility.OpenFilePanel(title, folder, extension);
                onChange?.Invoke(value);
            }
            
            if (area.Contains(Event.current.mousePosition)) {
                if (Event.current.type == EventType.DragUpdated) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                } else if (Event.current.type == EventType.DragPerform) {
                    for (int i = 0; i < DragAndDrop.paths.Length; i++) {
                        value = DragAndDrop.paths[i];
                    }
                    Event.current.Use();
                    onChange?.Invoke(value);
                }
            }

            return value;
        }

        public void DrawItem(Rect position, int index) {
            array[index].Value = FileSelect(position, array[index], "Select file", Application.dataPath,
                "");
        }

        public void DrawItemBackground(Rect position, int index) {
        }

        public void Duplicate(int index) {
            array.Add(array[index]);
        }

        public void EndGUI() {
        }

        public float GetItemHeight(int index) {
            return EditorGUIUtility.singleLineHeight;
        }

        public void Insert(int index) {
            array.Insert(index, "");
        }

        public void Move(int sourceIndex, int destIndex) {
            var item1 = array[sourceIndex];
            array.RemoveAt(sourceIndex);
            array.Insert(destIndex, item1);
        }

        public void Remove(int index) {
            array.RemoveAt(index);
        }
    }

    public class JSONNodeAdaptor : IReorderableListAdaptor {
        JSONNode m_Node;
        List<string> m_Keys;
        Dictionary<string, JSONNodeAdaptor> m_NodeAdaptors;

        public JSONNodeAdaptor(JSONNode aNode) {
            m_Node = aNode;
            m_Keys = new List<string>();
            m_NodeAdaptors = new Dictionary<string, JSONNodeAdaptor>();
            UpdateKeys();
        }

        void UpdateKeys() {
            if ( Count == m_Keys.Count ) return;
            m_Keys.Clear();
            if ( m_Node.IsObject ) {
                foreach ( var key in m_Node.AsObject.Keys ) {
                    m_Keys.Add(key);
                }
            }
        }

        public int Count { get { return m_Node.Count; } }

        public void Add() {
            var node = new JSONObject();
            m_Node.Add(node);
        }

        public void BeginGUI() {
            UpdateKeys();
        }

        public bool CanDrag(int index) {
            return true;
        }

        public bool CanRemove(int index) {
            return true;
        }

        public void Clear() {

        }

        public void DrawItem(Rect position, int index) {
            JSONNode node;
            float total = 1;
            const float typeWidth = 70f;
            float width = position.width - typeWidth;
            string key = index.ToString();

            if ( m_Node.IsObject ) {
                key = m_Keys[index];
                node = m_Node[key];

                total -= .5f;
                position.width = width * (1 - total);
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginChangeCheck();
                var newkey = EditorGUI.DelayedTextField(position, key);
                if ( EditorGUI.EndChangeCheck() ) {
                    m_Node.Remove(key);
                    m_Keys.RemoveAt(index);
                    m_Node.Add(newkey, node);
                    m_Keys.Add(newkey);
                }
                position.x += position.width;
            } else {
                node = m_Node.AsArray[index];
            }

            position.width = typeWidth;
            EditorGUI.BeginChangeCheck();
            JSONNodeType type = node.Tag;
            var newType = (JSONNodeType) EditorGUI.EnumPopup(position, type);
            if ( EditorGUI.EndChangeCheck() && newType != type ) {
                JSONNode newNode = new JSONNull();
                switch ( newType ) {
                    case JSONNodeType.String:
                        newNode = new JSONString(node.Value);
                        break;
                    case JSONNodeType.Boolean:
                        bool newVal = false;
                        if ( node.Value.ToUpper() == "NO" ) {
                            newVal = false;
                        } else if ( node.Value.ToUpper() == "YES" ) {
                            newVal = true;
                        } else {
                            bool.TryParse(node.Value, out newVal);
                        }
                        newNode = new JSONBool(newVal);
                        break;
                    case JSONNodeType.Number:
                        double newDouble = 0;
                        double.TryParse(node.Value, out newDouble);
                        newNode = new JSONNumber(newDouble);
                        break;
                    case JSONNodeType.Array:
                        newNode = new JSONArray();
                        if ( node.IsObject ) {
                            foreach ( JSONNode item in node.AsObject.Children ) {
                                newNode.AsArray.Add(item);
                            }
                        }
                        break;
                    case JSONNodeType.Object:
                        newNode = new JSONObject();
                        if ( node.IsArray ) {
                            foreach ( JSONNode item in node.AsArray ) {
                                newNode.AsObject.Add(item);
                            }
                        }
                        break;
                }
                if ( m_Node.IsArray ) {
                    m_Node.AsArray[index] = newNode;
                } else {
                    m_Node[key] = newNode;
                }
                node = newNode;
            }


            position.x += position.width;
            position.width = width * total;
            if ( node.IsBoolean ) {
                node.AsBool = EditorGUI.Toggle(position, node.AsBool);
            } else if ( node.IsString ) {
                node.Value = EditorGUI.DelayedTextField(position, node.Value);
            } else if ( node.IsNumber ) {
                node.AsDouble = EditorGUI.DoubleField(position, node.AsDouble);
            } else if ( node.IsArray || node.IsObject ) {
                JSONNodeAdaptor adaptor = GetAdaptor(key, node);
                position.height = ReorderableListGUI.CalculateListFieldHeight(adaptor);
                ReorderableListGUI.ListFieldAbsolute(position, adaptor);
            }
        }

        private JSONNodeAdaptor GetAdaptor(string key, JSONNode node) {
            JSONNodeAdaptor adaptor;
            if ( m_NodeAdaptors.ContainsKey(key) ) {
                adaptor = m_NodeAdaptors[key];
            } else {
                adaptor = new JSONNodeAdaptor(node);
            }
            return adaptor;
        }

        public void DrawItemBackground(Rect position, int index) {
        }

        public void Duplicate(int index) {
            m_Node.Add(JSON.Parse(m_Node[index].ToString()));
        }

        public void EndGUI() {
        }

        public float GetItemHeight(int index) {
            UpdateKeys();
            JSONNode node;
            string key;
            if ( m_Node.IsObject ) {
                key = m_Keys[index];
                node = m_Node[key];
            } else {
                key = index.ToString();
                node = m_Node.AsArray[index];
            }
            if ( node.IsObject || node.IsArray ) {
                return ReorderableListGUI.CalculateListFieldHeight(GetAdaptor(key, node));
            }
            return EditorGUIUtility.singleLineHeight;
        }

        public void Insert(int index) {
        }

        public void Move(int sourceIndex, int destIndex) {
        }

        public void Remove(int index) {
            string key;
            if ( m_Node.IsObject ) {
                key = m_Keys[index];
            } else {
                key = index.ToString();
            }
            if ( m_NodeAdaptors.ContainsKey(key) ) {
                m_NodeAdaptors.Remove(key);
            }
            m_Node.Remove(index);
            m_Keys.RemoveAt(index);
        }
    }
}
