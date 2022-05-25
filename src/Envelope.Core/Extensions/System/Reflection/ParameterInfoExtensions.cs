using System.Reflection;

namespace Envelope.Extensions;

public static class ParameterInfoExtensions
{
	public static T? GetFirstAttribute<T>(this ParameterInfo pi, bool inherit = true) where T : Attribute
	{
		if (pi == null) return default;
		var result = pi.GetCustomAttributes(typeof(T), inherit);
		return result != null ? result.FirstOrDefault() as T : null;
	}

	public static T[]? GetAttributeList<T>(this ParameterInfo pi, bool inherit = true) where T : Attribute
	{
		if (pi == null) return default;
		var result = pi.GetCustomAttributes(typeof(T), inherit);
		return result != null ? result as T[] : null;
	}
}
