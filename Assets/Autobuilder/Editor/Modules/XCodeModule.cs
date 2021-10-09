using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Autobuilder.ReorderableList;

namespace Autobuilder {
    public abstract class XCodeModule : BuildModule {
        const string BUILD_IOS = Builder.BUILDER + "BuildIOS";
        const string BUILD_DIR_IOS = "/iOS";
        const string DATA_PATH = "ProjectSettings/iOSBuildData.json";
        const string CAPABILITIES = "Capabilities";
        const string FILES = "Files";
        public const string CAPABILITY_TYPE = "Type";
        // iCloud capability
        public const string ENABLE_KEYVALUE_STORAGE = "EnableKeyValueStorage";
        public const string ENABLE_ICLOUD_DOCUMENT = "EnableiCloudDocument";
        public const string ENABLE_CLOUDKIT = "EnableiCloudKit";
        public const string ICLOUD_CUSTOM_CONTAINERS = "CustomContainers";
        public const string ASSOCIATED_DOMAINS = "AssociatedDomains";
        // PList
        public const string PLIST = "Plist";

        public enum CapabilityType {
            iCloud,
            AssociatedDomains,
        }

        JObject jsonData;
        protected JObject JsonData {
            get {
                if (jsonData == null) {
                    var node = Data["XCodeData"];
                    if (node == null || !(node is JObject)) {
                        jsonData = new JObject();
                        Data["XCodeData"] = jsonData;
                    } else {
                        jsonData = node as JObject;
                    }
                }
                return jsonData;
            }
        }

        public JArray Capabilities {
            get {
                var node = JsonData[CAPABILITIES];
                if (node == null || !(node is JArray)) {
                    node = new JArray();
                    JsonData[CAPABILITIES] = node;
                }

                return node as JArray;
            }
        }
        CapabilitiesAdaptor m_CapabilitiesAdaptor;

        public JObject Plist {
            get {
                var node = JsonData[PLIST];
                if (node == null || !(node is JObject)) {
                    node = new JObject();
                    JsonData[PLIST] = node;
                }

                return node as JObject;
            }
        }
        JSONNodeAdaptor m_PlistAdaptor;

        public JArray Files {
            get {
                var node = JsonData[FILES];
                if (node == null || !(node is JArray)) {
                    node = new JArray();
                    JsonData[FILES] = node;
                }
                return node as JArray;
            }
        }

        public override abstract BuildTarget Target { get; }

        public override abstract BuildTargetGroup TargetGroup { get; }

        JSONFilesAdaptor filesAdaptor;

        public override void Load(JObject root) {
            jsonData = null;
            base.Load(root);
            m_CapabilitiesAdaptor = new CapabilitiesAdaptor(Capabilities);
            m_PlistAdaptor = new JSONNodeAdaptor(Plist);
            filesAdaptor = new JSONFilesAdaptor(Files);
        }

        public override bool IsTarget(BuildTarget aTarget) {
            return aTarget == Target;
        }

        public override bool BuildGame(bool development = false) {
            if (!base.BuildGame(development)) return false;

            if (development) {
                // PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
            }

            string path = GetBuildPath(development);
            // Build Game
#if UNITY_2018_1_OR_NEWER
            BuildReport tReport = Builder.BuildGame(TargetGroup, Target,
                path, GetScenesList(), development);
            return tReport.summary.result != BuildResult.Succeeded;
#else
            string tReport = BuildGame(Target,
                path, aDevelopment);
            return !string.IsNullOrEmpty(tReport);
#endif
        }

        public virtual string GetBuildPath(bool aDevelopment) {
            return "";
        }

        public override void OptionsGUI(out bool build, out bool development) {
            build = false;
            development = false;

            EditorGUI.BeginChangeCheck();
            string tIOSIdentifier = EditorGUILayout.TextField("Bundle identifier",
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS));
            if (EditorGUI.EndChangeCheck()) {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS,
                    tIOSIdentifier);
            }

            EditorGUI.BeginChangeCheck();
            ReorderableListGUI.Title("Capabilities");
            ReorderableListGUI.ListField(m_CapabilitiesAdaptor);
            GUILayout.Label("If a value is Null, it will be set to be removed from the Plist",
                EditorStyles.miniLabel);
            ReorderableListGUI.Title("Plist");
            ReorderableListGUI.ListField(m_PlistAdaptor);
            ReorderableListGUI.Title("Files/Directories");
            ReorderableListGUI.ListField(filesAdaptor);
            if (EditorGUI.EndChangeCheck()) {
                Save();
            }

#if UNITY_IOS || UNITY_TVOS
            if ( GUILayout.Button("Apply Capabilities") ) {
                XCodePostProcessor.ProcessPbxProject(Target, GetBuildPath(false));
            }
            if ( GUILayout.Button("Modify Plist") ) {
                XCodePostProcessor.ProcessInfoPlist(Target, GetBuildPath(false));
            }
#endif
        }

        class CapabilitiesAdaptor : IReorderableListAdaptor {
            JArray m_Capabilities;

            public CapabilitiesAdaptor(JArray aCapabilities) {
                m_Capabilities = aCapabilities;
            }

            public int Count { get { return m_Capabilities.Count; } }

            public void Add() {
                var newNode = new JObject();
                SetType(CapabilityType.iCloud, newNode);
                m_Capabilities.Add(newNode);
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
                m_Capabilities = new JArray();
            }

            void SetType(CapabilityType type, JToken node) {
                node[CAPABILITY_TYPE] = (int) type;
                switch (type) {
                    case CapabilityType.iCloud:
                        if (node[ENABLE_KEYVALUE_STORAGE] == null) {
                            node[ENABLE_KEYVALUE_STORAGE] = false;
                        }
                        if (node[ENABLE_ICLOUD_DOCUMENT] == null) {
                            node[ENABLE_ICLOUD_DOCUMENT] = false;
                        }
                        if (node[ENABLE_CLOUDKIT] == null) {
                            node[ENABLE_CLOUDKIT] = true;
                        }
                        if (node[ICLOUD_CUSTOM_CONTAINERS] == null) {
                            node[ICLOUD_CUSTOM_CONTAINERS] = new JArray();
                        }
                        break;
                    case CapabilityType.AssociatedDomains:
                        if (node[ASSOCIATED_DOMAINS] == null) {
                            node[ASSOCIATED_DOMAINS] = new JArray();
                        }
                        break;
                }
            }

            public void DrawItem(Rect position, int index) {
                var node = m_Capabilities[index];
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginChangeCheck();
                var type = (CapabilityType) EditorGUI.EnumPopup(position, "Type",
                    (CapabilityType) (int) node[CAPABILITY_TYPE]);
                if (EditorGUI.EndChangeCheck()) {
                    SetType(type, node);
                }

                node[CAPABILITY_TYPE] = (int) type;

                switch (type) {
                    case CapabilityType.iCloud:
                        position.y += position.height;
                        node[ENABLE_KEYVALUE_STORAGE] = EditorGUI.ToggleLeft(position,
                            "Key-value storage", (bool) node[ENABLE_KEYVALUE_STORAGE]);

                        position.y += position.height;
                        node[ENABLE_ICLOUD_DOCUMENT] = EditorGUI.ToggleLeft(position,
                            "iCloud Documents", (bool) node[ENABLE_ICLOUD_DOCUMENT]);

                        position.y += position.height;
                        node[ENABLE_CLOUDKIT] = EditorGUI.ToggleLeft(position, "CloudKit",
                            (bool) node[ENABLE_CLOUDKIT]);

                        position.y += position.height;
                        position.height = EditorGUIUtility.singleLineHeight;
                        ReorderableListGUI.Title(position, "Containers");

                        position.y += position.height - 1;
                        var customContainers = node[ICLOUD_CUSTOM_CONTAINERS] as JArray;
                        position.height = ReorderableListGUI.CalculateListFieldHeight(
                            customContainers.Count, EditorGUIUtility.singleLineHeight);
                        ReorderableListGUI.ListFieldAbsolute<JToken>(position,
                            customContainers, DrawContainer, null,
                            EditorGUIUtility.singleLineHeight);
                        break;
                    case CapabilityType.AssociatedDomains:
                        position.y += position.height + 5;
                        ReorderableListGUI.Title(position, "Domains");

                        position.y += position.height - 1;
                        var associatedDomains = node[ASSOCIATED_DOMAINS] as JArray;
                        position.height = ReorderableListGUI.CalculateListFieldHeight(
                            associatedDomains.Count, EditorGUIUtility.singleLineHeight);
                        ReorderableListGUI.ListFieldAbsolute<JToken>(position,
                            associatedDomains, DrawContainer, null,
                            EditorGUIUtility.singleLineHeight);
                        break;
                }
            }

            JToken DrawContainer(Rect position, JToken node) {
                if (node.Type == JTokenType.String) {
                    node = new JValue("iCloud.$(CFBundleIdentifier)");
                }
                node = EditorGUI.DelayedTextField(position, (string) node);
                return node;
            }

            public void DrawItemBackground(Rect position, int index) {
            }

            public void Duplicate(int index) {
                var node = m_Capabilities[index];
                m_Capabilities.Add(node);
            }

            public void EndGUI() {
            }

            public float GetItemHeight(int index) {
                float height = EditorGUIUtility.singleLineHeight;

                var node = m_Capabilities[index];
                var type = (CapabilityType) (int) node[CAPABILITY_TYPE];

                switch (type) {
                    case CapabilityType.iCloud:
                        height += EditorGUIUtility.singleLineHeight * 4;
                        var customContainers = node[ICLOUD_CUSTOM_CONTAINERS] as JArray;
                        height += ReorderableListGUI.CalculateListFieldHeight(
                            customContainers.Count, EditorGUIUtility.singleLineHeight);
                        break;
                    case CapabilityType.AssociatedDomains:
                        height += EditorGUIUtility.singleLineHeight;
                        var associatedDomains = node[ASSOCIATED_DOMAINS] as JArray;
                        height += ReorderableListGUI.CalculateListFieldHeight(
                            associatedDomains.Count, EditorGUIUtility.singleLineHeight);
                        break;
                }

                return height;
            }

            public void Insert(int index) {
            }

            public void Move(int sourceIndex, int destIndex) {
            }

            public void Remove(int index) {
                m_Capabilities.RemoveAt(index);
            }
        }
    }
}
