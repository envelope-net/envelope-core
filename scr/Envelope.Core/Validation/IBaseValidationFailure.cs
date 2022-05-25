using Envelope.Reflection.ObjectPaths;

namespace Envelope.Validation;

public interface IBaseValidationFailure
{
	IObjectPath ObjectPath { get; }
	ValidationSeverity Severity { get; }
	string Message { get; }
	string MessageWithPropertyName { get; }
	bool HasServerCondition { get; }
	string? DetailInfo { get; }
}
