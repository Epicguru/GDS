using System;
using System.Reflection;

namespace Utility
{
    public static class Extensions
    {
        public static bool IsCompilerGenerated(this Type type)
        {
            return type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() != null;
        }
    }
}
