using Envelope.Exceptions;
using Envelope.Text;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Envelope.Validation;

public class ValidationRuleBuilder
{
	private const string IS_DEFAULT = "is default";
	private const string IS_WHITE_SPACE = "is white space";
	private const string IS_EMPTY = "is empty";

	public ValidationBuilder ValidationBuilder { get; }

	public string? Prefix { get; }
	public Dictionary<string, object> GlobalValidationContext { get; }

	public IReadOnlyList<IValidationMessage> Messages => ValidationBuilder._messages;

	internal ValidationRuleBuilder(string? prefix, Dictionary<string, object> globalValidationContext, ValidationBuilder validationBuilder)
	{
		Throw.ArgumentNull(validationBuilder);
		ValidationBuilder = validationBuilder;
		Prefix = prefix;
		GlobalValidationContext = globalValidationContext ?? new Dictionary<string, object>();
	}

	public List<IValidationMessage> Build()
		=> ValidationBuilder._messages;

	public ValidationRuleBuilder IfNull<T>(
		T? parameter,
		[CallerArgumentExpression("parameter")] string? parameterName = null,
		string? detailMessage = null,
		Action<ValidationRuleBuilder>? onSuccess = null)
	{
		if (parameter is null)
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} == null{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		else
			onSuccess?.Invoke(this);

		return this;
	}

	public ValidationRuleBuilder Validate(
		IValidable? validable,
		bool allowNullValue = false,
		Dictionary<string, object>? customValidationContext = null,
		[CallerArgumentExpression("validable")] string? validableParameterName = null,
		string? detailMessage = null)
	{
		if (validable is null)
		{
			if (!allowNullValue)
				ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", validableParameterName)} == null{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		}
		else
		{
			validable.Validate(StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", validableParameterName), ValidationBuilder, GlobalValidationContext, customValidationContext);
		}

		return this;
	}

	public ValidationRuleBuilder Validate(
		IEnumerable<IValidable>? validableCollection,
		bool allowNullCollection = false,
		bool allowEmptyCollection = false,
		Dictionary<string, object>? customValidationContext = null,
		[CallerArgumentExpression("validableCollection")] string? validableCollectionParameterName = null,
		string? detailMessage = null)
	{
		if (validableCollection is null)
		{
			if (!allowNullCollection)
				ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", validableCollectionParameterName)} == null{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		}
		else if (!validableCollection.Any())
		{
			if (!allowEmptyCollection)
				ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", validableCollectionParameterName)} {IS_EMPTY}{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		}
		else
		{
			var i = 0;
			foreach (var validable in validableCollection)
				validable.Validate(StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", $"{validableCollectionParameterName}[{i++}]"), ValidationBuilder, GlobalValidationContext, customValidationContext);
		}

		return this;
	}

	public ValidationRuleBuilder ValidateInitialization(
		IInitializationValidable? initializable,
		Dictionary<string, object>? customValidationContext = null,
		[CallerArgumentExpression("initializable")] string?
		initializableParameterName = null,
		string? detailMessage = null)
	{
		if (initializable is null)
		{
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", initializableParameterName)} == null{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		}
		else
		{
			initializable.ValidateInitialization(StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", initializableParameterName), ValidationBuilder, GlobalValidationContext, customValidationContext);
		}

		return this;
	}

	public ValidationRuleBuilder ValidateInitialization(
		IEnumerable<IInitializationValidable>? initializableCollection,
		Dictionary<string, object>? customValidationContext = null,
		[CallerArgumentExpression("initializableCollection")] string?
		initializableCollectionParameterName = null,
		string? detailMessage = null)
	{
		if (initializableCollection is null)
		{
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", initializableCollectionParameterName)} == null{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		}
		else if (!initializableCollection.Any())
		{
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", initializableCollectionParameterName)} {IS_EMPTY}{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		}
		else
		{
			var i = 0;
			foreach (var initializable in initializableCollection)
				initializable.ValidateInitialization(StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", $"{initializableCollectionParameterName}[{i++}]"), ValidationBuilder, GlobalValidationContext, customValidationContext);
		}

		return this;
	}

	public ValidationRuleBuilder IfNullOrEmpty(
		string? parameter,
		[CallerArgumentExpression("parameter")] string? parameterName = null,
		string? detailMessage = null,
		Action<ValidationRuleBuilder>? onSuccess = null)
	{
		if (parameter is null)
		{
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} == null{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
			return this;
		}

		if (string.IsNullOrEmpty(parameter))
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} {IS_EMPTY}{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		else
			onSuccess?.Invoke(this);

		return this;
	}

	public ValidationRuleBuilder IfNullOrWhiteSpace(
		string? parameter,
		[CallerArgumentExpression("parameter")] string? parameterName = null,
		string? detailMessage = null,
		Action<ValidationRuleBuilder>? onSuccess = null)
	{
		if (parameter is null)
		{
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} == null{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
			return this;
		}

		if (string.IsNullOrWhiteSpace(parameter))
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} {IS_WHITE_SPACE}{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		else
			onSuccess?.Invoke(this);

		return this;
	}

	public ValidationRuleBuilder IfNullOrEmpty(
		ICollection? parameter,
		[CallerArgumentExpression("parameter")] string? parameterName = null,
		string? detailMessage = null,
		Action<ValidationRuleBuilder>? onSuccess = null)
	{
		if (parameter is null)
		{
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} == null{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
			return this;
		}

		if (parameter.Count == 0)
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} {IS_EMPTY}{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		else
			onSuccess?.Invoke(this);

		return this;
	}

	public ValidationRuleBuilder IfNullOrEmpty(
		Array? parameter,
		[CallerArgumentExpression("parameter")] string? parameterName = null,
		string? detailMessage = null,
		Action<ValidationRuleBuilder>? onSuccess = null)
	{
		if (parameter is null)
		{
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} == null{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
			return this;
		}

		if (parameter.Length == 0)
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} {IS_EMPTY}{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		else
			onSuccess?.Invoke(this);

		return this;
	}

	public ValidationRuleBuilder IfNullOrEmpty(
		IEnumerable? parameter,
		[CallerArgumentExpression("parameter")] string? parameterName = null,
		string? detailMessage = null,
		Action<ValidationRuleBuilder>? onSuccess = null)
	{
		if (parameter is null)
		{
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} == null{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
			return this;
		}

		if (!parameter.Cast<object>().Any())
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} {IS_EMPTY}{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		else
			onSuccess?.Invoke(this);

		return this;
	}

	public ValidationRuleBuilder IfDefault<T>(
		T parameter,
		[CallerArgumentExpression("parameter")] string? parameterName = null,
		string? detailMessage = null,
		Action<ValidationRuleBuilder>? onSuccess = null)
		where T : struct, IComparable<T>, IComparable
	{
		if (ValidationHelper.IsDefault(parameter))
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} {IS_DEFAULT}{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		else
			onSuccess?.Invoke(this);

		return this;
	}

	public ValidationRuleBuilder IfNullableDefault<T>(
		T? parameter,
		[CallerArgumentExpression("parameter")] string? parameterName = null,
		string? detailMessage = null,
		Action<ValidationRuleBuilder>? onSuccess = null)
		where T : struct, IComparable<T>, IComparable
	{
		if (ValidationHelper.IsDefault(parameter))
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} {IS_DEFAULT}{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		else
			onSuccess?.Invoke(this);

		return this;
	}

	public ValidationRuleBuilder IfNullOrDefault<T>(
		T? parameter,
		[CallerArgumentExpression("parameter")] string? parameterName = null,
		string? detailMessage = null,
		Action<ValidationRuleBuilder>? onSuccess = null)
		where T : struct, IComparable<T>, IComparable
	{
		if (parameter is null)
		{
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} == null{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		}
		else if (ValidationHelper.IsDefault(parameter))
		{
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(Prefix, ".", parameterName)} {IS_DEFAULT}{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		}
		else
			onSuccess?.Invoke(this);

		return this;
	}

	public ValidationRuleBuilder If(
		bool value,
		[CallerArgumentExpression("value")] string? message = null,
		string? detailMessage = null,
		Action<ValidationRuleBuilder>? onSuccess = null)
	{
		if (value)
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{(string.IsNullOrWhiteSpace(Prefix) ? string.Empty : $"{Prefix} > ")} {message}{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		else
			onSuccess?.Invoke(this);

		return this;
	}

	public ValidationRuleBuilder If(
		Func<bool> value,
		string message,
		string? detailMessage = null,
		Action<ValidationRuleBuilder>? onSuccess = null)
	{
		Throw.ArgumentNull(value);

		if (value())
			ValidationBuilder._messages.Add(ValidationMessageFactory.Error($"{(string.IsNullOrWhiteSpace(Prefix) ? string.Empty : $"{Prefix} > ")} {message}{(string.IsNullOrWhiteSpace(detailMessage) ? string.Empty : $" | {detailMessage}")}"));
		else
			onSuccess?.Invoke(this);

		return this;
	}
}
