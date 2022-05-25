using System.ComponentModel;

namespace Envelope.Converters;

public static class ConverterHelper
{
	/// <remarks>USAGE: ConverterHelper.ConvertFrom("5C6C9643-C207-4AF0-A882-D2C0C92CEEB2", typeof(Guid))</remarks>
	public static object? ConvertFrom(object fromValue, Type toType)
	{
		if (toType == null)
			throw new ArgumentNullException(nameof(toType));

		var result = TypeDescriptor.GetConverter(toType).ConvertFrom(fromValue);
		return result;
	}

	/// <remarks>USAGE: ConverterHelper.ConvertFrom("5C6C9643-C207-4AF0-A882-D2C0C92CEEB2", typeof(Guid), out object? guidValue)</remarks>
	public static bool TryConvertFrom(object fromValue, Type toType, out object? result)
	{
		if (toType == null)
			throw new ArgumentNullException(nameof(toType));

		try
		{
			var convertedValue = TypeDescriptor.GetConverter(toType).ConvertFrom(fromValue);
			result = convertedValue!;
			return true;
		}
		catch (Exception)
		{
			result = default;
			return false;
		}
	}

	/// <remarks>USAGE: ConverterHelper.ConvertFrom&lt;Guid&gt;("5C6C9643-C207-4AF0-A882-D2C0C92CEEB2")</remarks>
	public static TResult ConvertFrom<TResult>(object fromValue)
	{
		var result = TypeDescriptor.GetConverter(typeof(TResult)).ConvertFrom(fromValue);
		return (TResult)result!;
	}

	/// <remarks>USAGE: ConverterHelper.ConvertFrom("5C6C9643-C207-4AF0-A882-D2C0C92CEEB2", out Guid guidValue)</remarks>
	public static bool TryConvertFrom<TResult>(object fromValue, out TResult? result)
	{
		try
		{
			var convertedValue = TypeDescriptor.GetConverter(typeof(TResult)).ConvertFrom(fromValue);
			result = (TResult)convertedValue!;
			return true;
		}
		catch (Exception)
		{
			result = default;
			return false;
		}
	}
}
