using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Autobuilder.SimpleJSON;
using RhoTools.ReorderableList;

namespace Autobuilder {
    public class XCodeModule : IBuildModule {
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

        public virtual string Name { get { return "iOS"; } }
        public virtual BuildTargetGroup TargetGroup { get { return BuildTargetGroup.iOS; } }
        public virtual BuildTarget Target { get { return BuildTarget.iOS; } }

        public virtual bool Enabled { get; set; }

        public virtual int BuildNumber { get; set; }

        protected virtual string DataPath { get; }

        JSONNode m_JsonData;
        protected JSONNode JsonData {
            get {
                if ( m_JsonData == null ) {
                    string path = DataPath;
                    if ( File.Exists(path) ) {
                        m_JsonData = JSON.Parse(File.ReadAllText(path));
                    } else {
                        m_JsonData = new JSONObject();
                    }
                }
                return m_JsonData;
            }
        }

        public JSONArray Capabilities {
            get {
                var node = JsonData[CAPABILITIES];
                if ( node == null || !node.IsArray ) {
                    node = new JSONArray();
                    JsonData[CAPABILITIES] = node;
                }

                return node.AsArray;
            }
        }
        CapabilitiesAdaptor m_CapabilitiesAdaptor;

        public JSONObject Plist {
            get {
                var node = JsonData[PLIST];
                if ( node == null || !node.IsObject ) {
                    node = new JSONObject();
                    JsonData[PLIST] = node;
                }

                return node.AsObject;
            }
        }
        JSONNodeAdaptor m_PlistAdaptor;

        public JSONArray Files {
            get {
                var node = JsonData[FILES];
                if (node == null || !node.IsArray) {
                    node = new JSONArray();
                    JsonData[FILES] = node;
                }
                return node.AsArray;
            }
        }
        JSONFilesAdaptor filesAdaptor;

        public XCodeModule() {
            m_CapabilitiesAdaptor = new CapabilitiesAdaptor(Capabilities);
            m_PlistAdaptor = new JSONNodeAdaptor(Plist);
            filesAdaptor = new JSONFilesAdaptor(Files);
        }

        void Save() {
            File.WriteAllText(DataPath, JsonData.ToString());
        }

        public bool IsTarget(BuildTarget aTarget) {
            return aTarget == Target;
        }

        public virtual bool BuildGame(bool aDevelopment = false) {
            if ( !aDevelopment ) {
                // } else {
                //     PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
            }

            string path = GetBuildPath(aDevelopment);
            // Build Game
#if UNITY_2018_1_OR_NEWER
            BuildReport tReport = Builder.BuildGame(Target,
                path, aDevelopment);
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

        public void OnGUI(out bool aBuild, out bool aDevelopment) {
            aBuild = false;
            aDevelopment = false;

            Enabled = EditorGUILayout.Toggle("Build " + Target + " version", Enabled);
            EditorGUI.BeginChangeCheck();
            string tIOSIdentifier = EditorGUILayout.TextField("Bundle identifier",
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS));
            if ( EditorGUI.EndChangeCheck() ) {
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
            if ( EditorGUI.EndChangeCheck() ) {
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
            JSONArray m_Capabilities;

            public CapabilitiesAdaptor(JSONArray aCapabilities) {
                m_Capabilities = aCapabilities;
            }

            public int Count { get { return m_Capabilities.Count; } }

            public void Add() {
                var newNode = new JSONObject();
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
                m_Capabilities = new JSONArray();
            }

            void SetType(CapabilityType type, JSONNode node) {
                node[CAPABILITY_TYPE] = (int) type;
                switch ( type ) {
                    case CapabilityType.iCloud:
                        if ( node[ENABLE_KEYVALUE_STORAGE] == null ) {
                            node[ENABLE_KEYVALUE_STORAGE] = false;
                        }
                        if ( node[ENABLE_ICLOUD_DOCUMENT] == null ) {
                            node[ENABLE_ICLOUD_DOCUMENT] = false;
                        }
                        if ( node[ENABLE_CLOUDKIT] == null ) {
                            node[ENABLE_CLOUDKIT] = true;
                        }
                        if ( node[ICLOUD_CUSTOM_CONTAINERS] == null ) {
                            node[ICLOUD_CUSTOM_CONTAINERS] = new JSONArray();
                        }
                        break;
                    case CapabilityType.AssociatedDomains:
                        if ( node[ASSOCIATED_DOMAINS] == null ) {
                            node[ASSOCIATED_DOMAINS] = new JSONArray();
                        }
                        break;
                }
            }

            public void DrawItem(Rect position, int index) {
                var node = m_Capabilities[index];
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginChangeCheck();
                var type = (CapabilityType) EditorGUI.EnumPopup(position, "Type",
                    (CapabilityType) node[CAPABILITY_TYPE].AsInt);
                if ( EditorGUI.EndChangeCheck() ) {
                    SetType(type, node);
                }

                node[CAPABILITY_TYPE] = (int) type;

                switch ( type ) {
                    case CapabilityType.iCloud:
                        position.y += position.height;
                        node[ENABLE_KEYVALUE_STORAGE] = EditorGUI.ToggleLeft(position,
                            "Key-value storage", node[ENABLE_KEYVALUE_STORAGE].AsBool);

                        position.y += position.height;
                        node[ENABLE_ICLOUD_DOCUMENT] = EditorGUI.ToggleLeft(position,
                            "iCloud Documents", node[ENABLE_ICLOUD_DOCUMENT].AsBool);

                        position.y += position.height;
                        node[ENABLE_CLOUDKIT] = EditorGUI.ToggleLeft(position, "CloudKit",
                            node[ENABLE_CLOUDKIT].AsBool);

                        position.y += position.height;
                        position.height = EditorGUIUtility.singleLineHeight;
                        ReorderableListGUI.Title(position, "Containers");

                        position.y += position.height - 1;
                        var customContainers = node[ICLOUD_CUSTOM_CONTAINERS].AsArray;
                        position.height = ReorderableListGUI.CalculateListFieldHeight(
                            customContainers.Count, EditorGUIUtility.singleLineHeight);
                        ReorderableListGUI.ListFieldAbsolute<JSONNode>(position,
                            customContainers, DrawContainer, null,
                            EditorGUIUtility.singleLineHeight);
                        break;
                    case CapabilityType.AssociatedDomains:
                        position.y += position.height + 5;
                        ReorderableListGUI.Title(position, "Domains");

                        position.y += position.height - 1;
                        var associatedDomains = node[ASSOCIATED_DOMAINS].AsArray;
                        position.height = ReorderableListGUI.CalculateListFieldHeight(
                            associatedDomains.Count, EditorGUIUtility.singleLineHeight);
                        ReorderableListGUI.ListFieldAbsolute<JSONNode>(position,
                            associatedDomains, DrawContainer, null,
                            EditorGUIUtility.singleLineHeight);
                        break;
                }
            }

            JSONNode DrawContainer(Rect position, JSONNode node) {
                if ( !node.IsString ) {
                    node = new JSONString("iCloud.$(CFBundleIdentifier)");
                }
                node.Value = EditorGUI.DelayedTextField(position, node.Value);
                return node;
            }

            public void DrawItemBackground(Rect position, int index) {
            }

            public void Duplicate(int index) {
                var node = m_Capabilities[index];
                m_Capabilities.Add(JSON.Parse(node.ToString()));
            }

            public void EndGUI() {
            }

            public float GetItemHeight(int index) {
                float height = EditorGUIUtility.singleLineHeight;

                var node = m_Capabilities[index];
                var type = (CapabilityType) node[CAPABILITY_TYPE].AsInt;

                switch ( type ) {
                    case CapabilityType.iCloud:
                        height += EditorGUIUtility.singleLineHeight * 4;
                        var customContainers = node[ICLOUD_CUSTOM_CONTAINERS].AsArray;
                        height += ReorderableListGUI.CalculateListFieldHeight(
                            customContainers.Count, EditorGUIUtility.singleLineHeight);
                        break;
                    case CapabilityType.AssociatedDomains:
                        height += EditorGUIUtility.singleLineHeight;
                        var associatedDomains = node[ASSOCIATED_DOMAINS].AsArray;
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
                m_Capabilities.Remove(index);
            }
        }
    }
}
