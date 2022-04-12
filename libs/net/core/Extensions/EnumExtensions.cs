using System.ComponentModel.DataAnnotations;

namespace TNO.Core.Extensions;

/// <summary>
/// EnumExtensions static class, provides extension methods for enum values.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Get the Keycloak name value of the specified permission.
    /// </summary>
    /// <param name="permission"></param>
    /// <returns></returns>
    public static string? GetName<T>(this T evalue)
    {
        if (evalue == null) return null;

        var enumType = typeof(T);
        var memberInfos = enumType.GetMember(evalue.ToString() ?? "");
        var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType) ?? throw new InvalidOperationException("Invalid enum type");
        var attribute = (DisplayAttribute?)enumValueMemberInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
        return attribute?.Name;
    }
}
