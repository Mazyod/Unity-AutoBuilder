#if UNITY_IOS || UNITY_TVOS
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;
using Autobuilder.SimpleJSON;
using System.Collections.Generic;

namespace Autobuilder {
    public class XCodePostProcessor {
        [PostProcessBuildAttribute(0)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject) {
            if ( buildTarget != BuildTarget.iOS && buildTarget != BuildTarget.tvOS )
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
            JSONArray Capabilities;
            JSONArray Files;
            if ( buildTarget == BuildTarget.iOS ) {
                var module = new IOSModule();
                Capabilities = module.Capabilities;
                Files = module.Files;
            } else if ( buildTarget == BuildTarget.tvOS ) {
                var module = new TVOSModule();
                Capabilities = module.Capabilities;
                Files = module.Files;
            } else {
                return;
            }

            for ( int i = 0; i < Capabilities.Count; i++ ) {
                var node = Capabilities[i];
                var type = (XCodeModule.CapabilityType) node[XCodeModule.CAPABILITY_TYPE].AsInt;
                
                switch ( type ) {
                    case XCodeModule.CapabilityType.iCloud:
                        bool enableKeyValueStorage = false;
                        bool enableiCloudDocument = false;
                        bool enableCloudKit = false;

                        var subNode = node[XCodeModule.ENABLE_KEYVALUE_STORAGE];
                        if ( subNode != null && subNode.IsBoolean ) {
                            enableKeyValueStorage = subNode.AsBool;
                        }

                        subNode = node[XCodeModule.ENABLE_ICLOUD_DOCUMENT];
                        if ( subNode != null && subNode.IsBoolean ) {
                            enableiCloudDocument = subNode.AsBool;
                        }

                        subNode = node[XCodeModule.ENABLE_CLOUDKIT];
                        if ( subNode != null && subNode.IsBoolean ) {
                            enableCloudKit = subNode.AsBool;
                        }

                        string[] customContainers;
                        subNode = node[XCodeModule.ICLOUD_CUSTOM_CONTAINERS];
                        if ( subNode != null && subNode.IsArray ) {
                            List<string> containersList = new List<string>();
                            foreach ( JSONNode item in subNode.AsArray ) {
                                if ( item.IsString ) {
                                    containersList.Add(item.Value);
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
                        if ( subNode != null && subNode.IsArray ) {
                            List<string> containersList = new List<string>();
                            foreach ( JSONNode item in subNode.AsArray ) {
                                if ( item.IsString ) {
                                    containersList.Add(item.Value);
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
                var file = Files[i].Value;
                Debug.Log(file);
                if (Directory.Exists(file)) {
                    Debug.Log("\tIs a directory");
                    Directory.Move(file, Path.Combine(pathToBuiltProject, Path.GetDirectoryName(file)));
                } else if (File.Exists(file)) {
                    Debug.Log("\tIs a file");
                    File.Move(file, Path.Combine(pathToBuiltProject, Path.GetFileName(file)));
                }
            }
        }

        public static void ProcessInfoPlist(BuildTarget buildTarget, string pathToBuiltProject) {
            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            if ( !File.Exists(plistPath) ) return;
            
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            JSONObject plistData;
            if ( buildTarget == BuildTarget.iOS ) {
                plistData = new IOSModule().Plist;
            } else if ( buildTarget == BuildTarget.tvOS ) {
                plistData = new TVOSModule().Plist;
            } else {
                return;
            }

            AddObjectToDocument(plist, plistData);

            // Apply editing settings to Info.plist
            plist.WriteToFile(plistPath);
        }

        static void AddObjectToDocument(PlistDocument document, JSONObject node) {
            AddDictToElement(document.root, node);
        }

        static void AddArrayToElement(PlistElementArray element, JSONArray node) {
            foreach ( JSONNode item in node ) {
                AddElementToArray(element, item);
            }
        }

        static void AddDictToElement(PlistElementDict element, JSONObject node) {
            foreach ( var itemKey in node.Keys ) {
                AddElementToDict(element, itemKey, node[itemKey]);
            }
        }

        static void AddElementToDict(PlistElementDict element, string key, JSONNode node) {
            if ( node.IsArray ) {
                var array = element.CreateArray(key);
                AddArrayToElement(array, node.AsArray);
            } else if ( node.IsBoolean ) {
                element.SetBoolean(key, node.AsBool);
            } else if ( node.IsString ) {
                element.SetString(key, node.Value);
            } else if ( node.IsNumber ) {
                if ( node.Value.Contains(".") ) {
                    element.SetReal(key, node.AsFloat);
                } else {
                    element.SetInteger(key, node.AsInt);
                }
            } else if ( node.IsObject ) {
                var dict = element.CreateDict(key);
                AddDictToElement(dict, node.AsObject);
            } else if ( node.IsNull ) {
                element.values.Remove(key);
            }
        }

        static void AddElementToArray(PlistElementArray element, JSONNode node) {
            if ( node.IsBoolean ) {
                element.AddBoolean(node.AsBool);
            } else if ( node.IsString ) {
                element.AddString(node.Value);
            } else if ( node.IsNumber ) {
                if ( node.Value.Contains(".") ) {
                    element.AddReal(node.AsFloat);
                } else {
                    element.AddInteger(node.AsInt);
                }
            } else if ( node.IsArray ) {
                var array = element.AddArray();
                AddArrayToElement(array, node.AsArray);
            } else if ( node.IsObject ) {
                var dict = element.AddDict();
                AddDictToElement(dict, node.AsObject);
            }
        }
    }
}
#endif
