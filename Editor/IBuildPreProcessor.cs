#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEditor;

namespace Autobuilder
{
    public interface IBuildPreProcessor
    {
        void PreProcess(BuildTarget aTarget, bool aDevelopment);
    }
}
