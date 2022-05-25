namespace Envelope.Extensions;

public static class DelegateExtensions
{
	public static Func<object, object>? ToNonGenericNotNull<T, TProperty>(this Func<T, TProperty>? func)
	{
		if (func == null)
			return default;

		return x => func((T)x)!;
	}

	public static Func<object, object?>? ToNonGeneric<T, TProperty>(this Func<T, TProperty>? func)
	{
		if (func == null)
			return default;

		return x => func((T)x);
	}

	public static Func<T, object>? ToNonGenericResult<T, TProperty>(this Func<T, TProperty> func)
	{
		if (func == null)
			return default;

		return x => func(x)!;
	}

	public static Func<object?, object?>? ToNonGenericNullable<T, TProperty>(this Func<T, TProperty>? func)
	{
		if (func == null)
			return default;

		return x => x == null
			? default
			: func((T)x);
	}

	public static Func<object?, bool>? ToNonGeneric<T>(this Func<T?, bool>? func)
	{
		if (func == null)
			return default;

		return x => func((T?)x);
	}

	public static Func<object?, string?>? ToNonGeneric<T>(this Func<T?, string?>? func)
	{
		if (func == null)
			return default;

		return x => func((T?)x);
	}
}
