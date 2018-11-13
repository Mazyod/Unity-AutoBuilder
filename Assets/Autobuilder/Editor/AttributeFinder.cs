using System;
using System.Collections.Generic;
using System.Reflection;

namespace Autobuilder
{
    public static class AttributeFinder
    {
        public static IEnumerable<Type> GetTypesWithAttribute<A>(AppDomain aAppDomain) where A : Attribute
        {
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (Type type in GetTypesWithAttribute<A>(assembly))
                    yield return type;
            }
        }
        public static IEnumerable<Type> GetTypesWithAttribute<A>(Assembly assembly) where A : Attribute
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(A), true).Length > 0)
                {
                    yield return type;
                }
            }
        }
    }
}
