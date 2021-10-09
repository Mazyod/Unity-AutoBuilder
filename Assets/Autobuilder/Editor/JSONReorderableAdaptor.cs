using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Autobuilder.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace Autobuilder {
    public class JSONFilesAdaptor : IReorderableListAdaptor {
        JArray array;

        public JSONFilesAdaptor(JArray array) {
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
            array[index] = FileSelect(position, (string) array[index], "Select file", Application.dataPath, "");
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
        JToken m_Node;
        List<string> m_Keys;
        Dictionary<string, JSONNodeAdaptor> m_NodeAdaptors;
        System.Random random;

        public JSONNodeAdaptor(JToken aNode) {
            m_Node = aNode;
            m_Keys = new List<string>();
            m_NodeAdaptors = new Dictionary<string, JSONNodeAdaptor>();
            random = new System.Random();

            UpdateKeys();
        }

        void UpdateKeys() {
            if (Count == m_Keys.Count) return;
            m_Keys.Clear();
            if (m_Node is JObject) {
                foreach (var key in (m_Node as JObject).Properties()) {
                    m_Keys.Add(key.Name);
                }
            }
        }

        public int Count {
            get {
                if (m_Node is JObject) return (m_Node as JObject).Count;
                if (m_Node is JArray) return (m_Node as JArray).Count;
                return 0;
            }
        }

        public void Add() {
            var node = new JObject();
            if (m_Node is JObject) (m_Node as JObject).Add(random.Next().ToString(), node);
            if (m_Node is JArray) (m_Node as JArray).Add(node);
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
            JToken node;
            float total = 1;
            const float typeWidth = 70f;
            float width = position.width - typeWidth;
            string key = index.ToString();

            if (m_Node is JObject) {
                var objNode = m_Node as JObject;
                key = m_Keys[index];
                node = m_Node[key];

                total -= .5f;
                position.width = width * (1 - total);
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginChangeCheck();
                var newkey = EditorGUI.DelayedTextField(position, key);
                if (EditorGUI.EndChangeCheck()) {
                    objNode.Remove(key);
                    m_Keys.RemoveAt(index);
                    objNode.Add(newkey, node);
                    m_Keys.Add(newkey);
                }
                position.x += position.width;
            } else {
                node = (m_Node as JArray)[index];
            }

            position.width = typeWidth;
            EditorGUI.BeginChangeCheck();
            JTokenType type = node.Type;
            var newType = (JTokenType) EditorGUI.EnumPopup(position, type);
            if (EditorGUI.EndChangeCheck() && newType != type) {
                JToken newNode = null;
                switch (newType) {
                    case JTokenType.String:
                        try {
                            newNode = new JValue((string) node);
                        } catch (ArgumentException) {
                            newNode = new JValue("");
                        }
                        break;
                    case JTokenType.Boolean:
                        bool newVal = false;
                        if (((string) node).ToUpper() == "NO") {
                            newVal = false;
                        } else if (((string) node).ToUpper() == "YES") {
                            newVal = true;
                        } else {

                            bool.TryParse((string) node, out newVal);
                        }
                        newNode = new JValue(newVal);
                        break;
                    case JTokenType.Float:
                        float newFloat;
                        try {
                            float.TryParse((string) node, out newFloat);
                        } catch (ArgumentException) {
                            newFloat = 0;
                        }
                        newNode = new JValue(newFloat);
                        break;
                    case JTokenType.Integer:
                        int newInt;
                        try {
                            int.TryParse((string) node, out newInt);
                        } catch (ArgumentException) {
                            newInt = 0;
                        }
                        newNode = new JValue(newInt);
                        break;
                    case JTokenType.Array:
                        newNode = new JArray();
                        if (node is JObject) {
                            foreach (JToken item in (node as JObject).Children()) {
                                (newNode as JArray).Add(item);
                            }
                        }
                        break;
                    case JTokenType.Object:
                        newNode = new JObject();
                        if (node is JArray) {
                            foreach (JToken item in (node as JArray)) {
                                (newNode as JObject).Add(item);
                            }
                        }
                        break;
                    default:
                        newNode = node;
                        break;
                }
                if (m_Node is JArray) {
                    (m_Node as JArray)[index] = newNode;
                } else {
                    m_Node[key] = newNode;
                }
                node = newNode;
            }


            position.x += position.width;
            position.width = width * total;
            switch (node.Type) {
                case JTokenType.Boolean:
                    SetNode(index, new JValue(EditorGUI.Toggle(position, (bool) node)));
                    break;
                case JTokenType.Integer:
                    SetNode(index, new JValue(EditorGUI.IntField(position, (int) node)));
                    break;
                case JTokenType.Float:
                    SetNode(index, new JValue(EditorGUI.FloatField(position, (float) node)));
                    break;
                case JTokenType.String:
                    SetNode(index, new JValue(EditorGUI.TextField(position, (string) node)));
                    break;
                default:
                    JSONNodeAdaptor adaptor = GetAdaptor(key, node);
                    position.height = ReorderableListGUI.CalculateListFieldHeight(adaptor);
                    ReorderableListGUI.ListFieldAbsolute(position, adaptor);
                    break;
            }
        }

        void SetNode(int index, JValue value) {
            if (m_Node is JObject) {
                var key = m_Keys[index];
                m_Node[key] = value;
            } else if (m_Node is JArray) {
                m_Node[index] = value;
            }
        }

        private JSONNodeAdaptor GetAdaptor(string key, JToken node) {
            JSONNodeAdaptor adaptor;
            if (m_NodeAdaptors.ContainsKey(key)) {
                adaptor = m_NodeAdaptors[key];
            } else {
                adaptor = new JSONNodeAdaptor(node);
            }
            return adaptor;
        }

        public void DrawItemBackground(Rect position, int index) {
        }

        public void Duplicate(int index) {
            if (m_Node is JObject) {
                (m_Node as JObject).Add(random.Next().ToString(), m_Node[index]);
            } else if (m_Node is JArray) {
                (m_Node as JArray).Add(m_Node[index]);
            }
        }

        public void EndGUI() {
        }

        public float GetItemHeight(int index) {
            UpdateKeys();
            JToken node;
            string key;
            if (m_Node is JObject) {
                key = m_Keys[index];
                node = m_Node[key];
            } else {
                key = index.ToString();
                node = (m_Node as JArray)[index];
            }
            if (node is JObject || node is JArray) {
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
            if (m_Node is JObject) {
                key = m_Keys[index];
                (m_Node as JObject).Remove(key);
            } else {
                key = index.ToString();
                (m_Node as JArray).RemoveAt(index);
            }
            if (m_NodeAdaptors.ContainsKey(key)) {
                m_NodeAdaptors.Remove(key);
            }
            m_Keys.RemoveAt(index);
        }
    }
}
