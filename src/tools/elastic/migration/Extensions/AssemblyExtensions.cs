using System.Reflection;

namespace TNO.Elastic.Migration;

/// <summary>
/// AssemblyExtensions static class, provides extension methods for assembly.
/// </summary>
public static class AssemblyExtensions
{
    /// <summary>
    /// Get all the migrations on the assembly.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static Type[] GetMigrationTypes(this Assembly assembly)
    {
        return assembly.GetTypes().Where(t => !t.IsAbstract && t.IsClass && t.IsSubclassOf(typeof(Migration))).ToArray();
    }
}
