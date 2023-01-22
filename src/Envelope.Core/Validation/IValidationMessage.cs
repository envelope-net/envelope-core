namespace Envelope.Validation;

#if NET6_0_OR_GREATER
[Envelope.Serializer.JsonPolymorphicConverter]
#endif
public interface IValidationMessage
{
	ValidationSeverity Severity { get; }

	string? Code { get; }

	string? Message { get; }
}
