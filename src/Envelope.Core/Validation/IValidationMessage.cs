namespace Envelope.Validation;

public interface IValidationMessage
{
	ValidationSeverity Severity { get; }

	string? Code { get; }

	string? Message { get; }
}
