namespace Envelope.Validation.Internal;

internal class ValidationMessage : IValidationMessage
{
	public ValidationSeverity Severity { get; set; }

	public string? Code { get; set; }

	public string Message { get; set; }

	public ValidationMessage(ValidationSeverity severity, string message)
	{
		if (string.IsNullOrWhiteSpace(message))
			throw new ArgumentNullException(nameof(message));

		Severity = severity;
		Message = message;
	}

	public override string ToString()
	{
		if (Severity == ValidationSeverity.Error)
		{
			if (string.IsNullOrWhiteSpace(Code))
				return Message;
			else
				return $"{Code}: {Message}";
		}
		else
		{
			if (string.IsNullOrWhiteSpace(Code))
				return $"{Severity}: {Message}";
			else
				return $"{Severity}: {Code}: {Message}";
		}
	}
}
