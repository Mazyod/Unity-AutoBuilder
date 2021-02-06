﻿using UnityEditor;

namespace Autobuilder {
    public interface IBuildModule {
        string Name { get; }
        BuildTarget Target { get; }
        BuildTargetGroup TargetGroup { get; }
        bool IsTarget(BuildTarget aTarget);
        bool Enabled { get; set; }
        int BuildNumber { get; set; }
        void OnGUI(out bool aBuild, out bool aDevelopment);
        bool BuildGame(bool aDevelopment = false); // Returns true if successfull
    }
}