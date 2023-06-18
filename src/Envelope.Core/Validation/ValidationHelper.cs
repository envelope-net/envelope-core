using Envelope.Extensions;
using System.Collections;

namespace Envelope.Validation;

public class ValidationHelper
{
	public static bool IsDefault<T>(T? value)
		=> Equals(value, typeof(T).GetDefaultNullableValue());

	public static bool IsDefaultOrEmpty<T>(T? value)
	{
		var defaultValue = typeof(T).GetDefaultNullableValue();
		return IsDefaultOrEmpty(value, defaultValue);
	}

	public static bool IsDefaultOrEmpty<T>(T? value, object? defaultValue)
	{
		if (Equals(value, defaultValue))
			return true;

		switch (value)
		{
			case null:
			case string s when string.IsNullOrWhiteSpace(s):
			case ICollection c when c.Count == 0:
			case Array a when a.Length == 0:
			case IEnumerable e when !e.Cast<object>().Any():
				return true;
			default:
				return false;
		}
	}
}
