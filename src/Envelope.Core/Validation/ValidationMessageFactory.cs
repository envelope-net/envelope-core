using Envelope.Validation.Internal;

namespace Envelope.Validation;

public static class ValidationMessageFactory
{
	public static IValidationMessage Error(string message, string? code = null)
		=> new ValidationMessage(ValidationSeverity.Error, message)
		{
			Code = code
		};

	public static IValidationMessage Warning(string message, string? code = null)
		=> new ValidationMessage(ValidationSeverity.Warning, message)
		{
			Code = code
		};
}
