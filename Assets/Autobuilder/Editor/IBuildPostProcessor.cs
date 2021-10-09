#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

using UnityEditor;

namespace Autobuilder
{
    public interface IBuildPostProcessor
    {
    #if UNITY_2018_1_OR_NEWER
        void PostProcess(BuildTarget aTarget, bool aDevelopment, BuildReport report);
    #else
        void PostProcess(BuildTarget aTarget, bool aDevelopment, string report);
    #endif
    }
}
