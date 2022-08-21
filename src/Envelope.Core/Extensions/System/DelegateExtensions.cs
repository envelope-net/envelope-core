using System.Diagnostics.CodeAnalysis;

namespace Envelope.Extensions;

public static class DelegateExtensions
{
	[return: NotNullIfNotNull("func")]
	public static Func<object, object>? ToNonGenericNotNull<T, TProperty>(this Func<T, TProperty>? func)
	{
		if (func == null)
			return default;

		return x => func((T)x)!;
	}

	[return: NotNullIfNotNull("func")]
	public static Func<object, object?>? ToNonGeneric<T, TProperty>(this Func<T, TProperty>? func)
	{
		if (func == null)
			return default;

		return x => func((T)x);
	}

	[return: NotNullIfNotNull("func")]
	public static Func<T, object>? ToNonGenericResult<T, TProperty>(this Func<T, TProperty> func)
	{
		if (func == null)
			return default;

		return x => func(x)!;
	}

	[return: NotNullIfNotNull("func")]
	public static Func<object?, object?>? ToNonGenericNullable<T, TProperty>(this Func<T, TProperty>? func)
	{
		if (func == null)
			return default;

		return x => x == null
			? default
			: func((T)x);
	}

	[return: NotNullIfNotNull("func")]
	public static Func<object?, bool>? ToNonGeneric<T>(this Func<T?, bool>? func)
	{
		if (func == null)
			return default;

		return x => func((T?)x);
	}

	[return: NotNullIfNotNull("func")]
	public static Func<object?, string?>? ToNonGeneric<T>(this Func<T?, string?>? func)
	{
		if (func == null)
			return default;

		return x => func((T?)x);
	}

	[return: NotNullIfNotNull("func")]
	public static Func<TTo>? ConvertGenericType<TFrom, TTo>(this Func<TFrom> func)
		where TFrom : TTo
	{
		if (func == null)
			return default;

		return () => func();
	}
}
