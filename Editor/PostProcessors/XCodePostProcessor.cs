#if UNITY_IOS || UNITY_TVOS
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Autobuilder {
    public class XCodePostProcessor {
        [PostProcessBuildAttribute(0)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject) {
            if (buildTarget != BuildTarget.iOS && buildTarget != BuildTarget.tvOS)
                return;

            ProcessPbxProject(buildTarget, pathToBuiltProject);
            // TODO: Turn this into generic for future projects
            ProcessInfoPlist(buildTarget, pathToBuiltProject);
        }

        public static void ProcessPbxProject(BuildTarget buildTarget, string pathToBuiltProject) {
            var targetName = "Unity-iPhone";

            var pbxProjectPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            var pbxProject = new PBXProject();
            pbxProject.ReadFromFile(pbxProjectPath);
#if UNITY_2020_2_OR_NEWER
            var targetGuid = pbxProject.GetUnityMainTargetGuid();
#else
            var targetGuid = pbxProject.TargetGuidByName(targetName);
#endif

            var entitlementsFileName = Builder.FileName + ".entitlements";
            var entitlementsFilePath = pathToBuiltProject + "/" + entitlementsFileName;

            pbxProject.AddFile(entitlementsFilePath, entitlementsFileName);
            pbxProject.SetBuildProperty(targetGuid, "CODE_SIGN_ENTITLEMENTS", entitlementsFileName);
            pbxProject.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "YES");

            pbxProject.WriteToFile(pbxProjectPath);

            var capabilityManager = new ProjectCapabilityManager(pbxProjectPath, entitlementsFilePath, targetName);
            JArray Capabilities;
            JArray Files;
            if (buildTarget == BuildTarget.iOS) {
                var module = new IOSModule();
                Capabilities = module.Capabilities;
                Files = module.Files;
            } else if (buildTarget == BuildTarget.tvOS) {
                var module = new TVOSModule();
                Capabilities = module.Capabilities;
                Files = module.Files;
            } else {
                return;
            }

            for (int i = 0; i < Capabilities.Count; i++) {
                var node = Capabilities[i];
                var type = (XCodeModule.CapabilityType)(int)node[XCodeModule.CAPABILITY_TYPE];

                switch (type) {
                    case XCodeModule.CapabilityType.iCloud:
                        bool enableKeyValueStorage = false;
                        bool enableiCloudDocument = false;
                        bool enableCloudKit = false;

                        var subNode = node[XCodeModule.ENABLE_KEYVALUE_STORAGE];
                        if (subNode != null && subNode.Type == JTokenType.Boolean) {
                            enableKeyValueStorage = (bool)subNode;
                        }

                        subNode = node[XCodeModule.ENABLE_ICLOUD_DOCUMENT];
                        if (subNode != null && subNode.Type == JTokenType.Boolean) {
                            enableiCloudDocument = (bool)subNode;
                        }

                        subNode = node[XCodeModule.ENABLE_CLOUDKIT];
                        if (subNode != null && subNode.Type == JTokenType.Boolean) {
                            enableCloudKit = (bool)subNode;
                        }

                        string[] customContainers;
                        subNode = node[XCodeModule.ICLOUD_CUSTOM_CONTAINERS];
                        if (subNode != null && subNode.Type == JTokenType.Array) {
                            List<string> containersList = new List<string>();
                            foreach (JToken item in subNode.AsJEnumerable<JToken>()) {
                                if (item.Type == JTokenType.String) {
                                    containersList.Add(item.Value<string>());
                                }
                            }
                            customContainers = containersList.ToArray();
                        } else {
                            customContainers = new string[0];
                        }
                        // Add iCloud
                        capabilityManager.AddiCloud(
                            enableKeyValueStorage, enableiCloudDocument, enableCloudKit,
                            false, customContainers);
                        break;
                    case XCodeModule.CapabilityType.AssociatedDomains:
                        string[] associatedDomains;
                        subNode = node[XCodeModule.ASSOCIATED_DOMAINS];
                        if (subNode != null && subNode.Type == JTokenType.Array) {
                            List<string> containersList = new List<string>();
                            foreach (var item in subNode.AsJEnumerable<JToken>()) {
                                if (item.Type == JTokenType.String) {
                                    containersList.Add(item.Value<string>());
                                }
                            }
                            associatedDomains = containersList.ToArray();
                        } else {
                            associatedDomains = new string[0];
                        }
                        capabilityManager.AddAssociatedDomains(associatedDomains);
                        break;
                }
            }
            capabilityManager.WriteToFile();

            for (int i = 0; i < Files.Count; i++) {
                var file = Files[i].Value<string>();
                Debug.Log(file);
                if (Directory.Exists(file)) {
                    //Debug.Log("\tIs a directory");
                    Directory.Move(file, Path.Combine(pathToBuiltProject, Path.GetDirectoryName(file)));
                } else if (File.Exists(file)) {
                    //Debug.Log("\tIs a file");
                    File.Move(file, Path.Combine(pathToBuiltProject, Path.GetFileName(file)));
                }
            }
        }

        public static void ProcessInfoPlist(BuildTarget buildTarget, string pathToBuiltProject) {
            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            if (!File.Exists(plistPath)) return;

            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            JObject plistData;
            if (buildTarget == BuildTarget.iOS) {
                plistData = new IOSModule().Plist;
            } else if (buildTarget == BuildTarget.tvOS) {
                plistData = new TVOSModule().Plist;
            } else {
                return;
            }

            AddObjectToDocument(plist, plistData);

            // Apply editing settings to Info.plist
            plist.WriteToFile(plistPath);
        }

        static void AddObjectToDocument(PlistDocument document, JObject node) {
            AddDictToElement(document.root, node);
        }

        static void AddArrayToElement(PlistElementArray element, JArray node) {
            foreach (var item in node) {
                AddElementToArray(element, item);
            }
        }

        static void AddDictToElement(PlistElementDict element, JObject node) {
            foreach (var item in node) {
                AddElementToDict(element, item.Key, item.Value);
            }
        }

        static void AddElementToDict(PlistElementDict element, string key, JToken node) {
            switch (node.Type) {
                case JTokenType.Array:
                    var array = element.CreateArray(key);
                    AddArrayToElement(array, (JArray)node);
                    break;
                case JTokenType.Boolean:
                    element.SetBoolean(key, (bool)node);
                    break;
                case JTokenType.String:
                    element.SetString(key, node.Value<string>());
                    break;
                case JTokenType.Integer:
                    element.SetReal(key, node.Value<int>());
                    break;
                case JTokenType.Float:
                    element.SetReal(key, node.Value<float>());
                    break;
                case JTokenType.Object:
                    var dict = element.CreateDict(key);
                    AddDictToElement(dict, (JObject)node);
                    break;
                case JTokenType.Null:
                    element.values.Remove(key);
                    break;
            }
        }

        static void AddElementToArray(PlistElementArray element, JToken node) {
            switch (node.Type) {
                case JTokenType.Array:
                    var array = element.AddArray();
                    AddArrayToElement(array, (JArray)node);
                    break;
                case JTokenType.Boolean:
                    element.AddBoolean(node.Value<bool>());
                    break;
                case JTokenType.String:
                    element.AddString(node.Value<string>());
                    break;
                case JTokenType.Integer:
                    element.AddInteger(node.Value<int>());
                    break;
                case JTokenType.Float:
                    element.AddReal(node.Value<float>());
                    break;
                case JTokenType.Object:
                    var dict = element.AddDict();
                    AddDictToElement(dict, (JObject)node);
                    break;
                case JTokenType.Null:
                    break;
            }
        }
    }
}
#endif
