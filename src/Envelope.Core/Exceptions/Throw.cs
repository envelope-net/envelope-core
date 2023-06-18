using Envelope.Validation;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Envelope.Exceptions;

public class Throw
{
	private const string IS_DEFAULT = "is default";
	private const string IS_WHITE_SPACE = "is white space";
	private const string IS_EMPTY = "is empty";

	[System.Diagnostics.StackTraceHidden]
	public static void ArgumentNull<T>([NotNull] T? argument, [CallerArgumentExpression("argument")] string? paramName = null)
	{
		if (argument is null)
			ThrowArgumentNullException(paramName!);
	}

	[System.Diagnostics.StackTraceHidden]
	public static void ArgumentNullOrEmpty(string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
	{
		if (argument is null)
			ThrowArgumentNullException(paramName!);

		if (string.IsNullOrEmpty(argument))
			ThrowArgumentException(IS_EMPTY, paramName!);
	}

	[System.Diagnostics.StackTraceHidden]
	public static void ArgumentNullOrWhiteSpace(string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
	{
		if (argument is null)
			ThrowArgumentNullException(paramName!);

		if (string.IsNullOrWhiteSpace(argument))
			ThrowArgumentException(IS_WHITE_SPACE, paramName!);
	}

	[System.Diagnostics.StackTraceHidden]
	public static void ArgumentNullOrEmpty(ICollection? argument, [CallerArgumentExpression("argument")] string? paramName = null)
	{
		if (argument is null)
			ThrowArgumentNullException(paramName!);

		if (argument.Count == 0)
			ThrowArgumentException(IS_EMPTY, paramName!);
	}

	[System.Diagnostics.StackTraceHidden]
	public static void ArgumentNullOrEmpty(Array? argument, [CallerArgumentExpression("argument")] string? paramName = null)
	{
		if (argument is null)
			ThrowArgumentNullException(paramName!);

		if (argument.Length == 0)
			ThrowArgumentException(IS_EMPTY, paramName!);
	}

	[System.Diagnostics.StackTraceHidden]
	public static void ArgumentNullOrEmpty(IEnumerable? argument, [CallerArgumentExpression("argument")] string? paramName = null)
	{
		if (argument is null)
			ThrowArgumentNullException(paramName!);

		if (!argument.Cast<object>().Any())
			ThrowArgumentException(IS_EMPTY, paramName!);
	}

	[System.Diagnostics.StackTraceHidden]
	public static void ArgumentDefault<T>([NotNull] T argument, [CallerArgumentExpression("argument")] string? paramName = null)
		where T : struct, IComparable<T>, IComparable
	{
		if (ValidationHelper.IsDefault(argument))
			ThrowArgumentException(IS_DEFAULT, paramName!);
	}

	[System.Diagnostics.StackTraceHidden]
	public static void ArgumentNullableDefault<T>([NotNull] T? argument, [CallerArgumentExpression("argument")] string? paramName = null)
		where T : struct, IComparable<T>, IComparable
	{
		if (ValidationHelper.IsDefault(argument))
			ThrowArgumentException(IS_DEFAULT, paramName!);
	}

	[System.Diagnostics.StackTraceHidden]
	public static void ArgumentNullOrDefault<T>([NotNull] T? argument, [CallerArgumentExpression("argument")] string? paramName = null)
		where T : struct, IComparable<T>, IComparable
	{
		if (argument is null)
		{
			ThrowArgumentNullException(paramName!);
		}
		else if (ValidationHelper.IsDefault(argument))
		{
			ThrowArgumentException(IS_DEFAULT, paramName!);
		}
	}

	[System.Diagnostics.StackTraceHidden]
	public static void ArgumentIf<T>(bool value, [NotNull] T? argument, [CallerArgumentExpression("value")] string? message = null, [CallerArgumentExpression("argument")] string? paramName = null)
	{
		if (value)
			ThrowArgumentException(message!, paramName!);
	}















	[System.Diagnostics.StackTraceHidden]
	public static void IfNull<T>([NotNull] T? value, [CallerArgumentExpression("value")] string? valueName = null)
	{
		if (value is null)
			ThrowInvalidOperationException($"{valueName} == null");
	}

	[System.Diagnostics.StackTraceHidden]
	public static void IfNullOrEmpty(string? value, [CallerArgumentExpression("value")] string? valueName = null)
	{
		if (value is null)
			ThrowInvalidOperationException($"{valueName} == null");

		if (string.IsNullOrEmpty(value))
			ThrowInvalidOperationException($"{valueName} {IS_EMPTY}");
	}

	[System.Diagnostics.StackTraceHidden]
	public static void IfNullOrWhiteSpace(string? value, [CallerArgumentExpression("value")] string? valueName = null)
	{
		if (value is null)
			ThrowInvalidOperationException($"{valueName} == null");

		if (string.IsNullOrWhiteSpace(value))
			ThrowInvalidOperationException($"{valueName} {IS_WHITE_SPACE}");
	}

	[System.Diagnostics.StackTraceHidden]
	public static void IfNullOrEmpty(ICollection? value, [CallerArgumentExpression("value")] string? valueName = null)
	{
		if (value is null)
			ThrowInvalidOperationException($"{valueName} == null");

		if (value.Count == 0)
			ThrowInvalidOperationException($"{valueName} {IS_EMPTY}");
	}

	[System.Diagnostics.StackTraceHidden]
	public static void IfNullOrEmpty(Array? value, [CallerArgumentExpression("value")] string? valueName = null)
	{
		if (value is null)
			ThrowInvalidOperationException($"{valueName} == null");

		if (value.Length == 0)
			ThrowInvalidOperationException($"{valueName} {IS_EMPTY}");
	}

	[System.Diagnostics.StackTraceHidden]
	public static void IfNullOrEmpty(IEnumerable? value, [CallerArgumentExpression("value")] string? valueName = null)
	{
		if (value is null)
			ThrowInvalidOperationException($"{valueName} == null");

		if (!value.Cast<object>().Any())
			ThrowInvalidOperationException($"{valueName} {IS_EMPTY}");
	}

	[System.Diagnostics.StackTraceHidden]
	public static void IfDefault<T>([NotNull] T value, [CallerArgumentExpression("value")] string? valueName = null)
		where T : struct, IComparable<T>, IComparable
	{
		if (ValidationHelper.IsDefault(value))
			ThrowInvalidOperationException($"{valueName} {IS_DEFAULT}");
	}

	[System.Diagnostics.StackTraceHidden]
	public static void IfNullableDefault<T>([NotNull] T? value, [CallerArgumentExpression("value")] string? valueName = null)
		where T : struct, IComparable<T>, IComparable
	{
		if (ValidationHelper.IsDefault(value))
			ThrowInvalidOperationException($"{valueName} {IS_DEFAULT}");
	}

	[System.Diagnostics.StackTraceHidden]
	public static void IfNullOrDefault<T>([NotNull] T? value, [CallerArgumentExpression("value")] string? valueName = null)
		where T : struct, IComparable<T>, IComparable
	{
		if (value is null)
		{
			ThrowInvalidOperationException($"{valueName} == null");
		}
		else if (ValidationHelper.IsDefault(value))
		{
			ThrowInvalidOperationException($"{valueName} {IS_DEFAULT}");
		}
	}

	[System.Diagnostics.StackTraceHidden]
	public static void If(bool value, [CallerArgumentExpression("value")] string? message = null)
	{
		if (value)
			ThrowInvalidOperationException(message!);
	}

	[System.Diagnostics.StackTraceHidden]
	[DoesNotReturn]
	private static void ThrowArgumentNullException(string paramName)
		=> throw new ArgumentNullException(paramName);

	[System.Diagnostics.StackTraceHidden]
	[DoesNotReturn]
	private static void ThrowArgumentException(string message, string paramName)
		=> throw new ArgumentException(message, paramName);

	[System.Diagnostics.StackTraceHidden]
	[DoesNotReturn]
	private static void ThrowInvalidOperationException(string message)
		=> throw new InvalidOperationException(message);
}
