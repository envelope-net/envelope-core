using Envelope.Reflection.ObjectPaths;

namespace Envelope.Validation;

#if NET6_0_OR_GREATER
[Envelope.Serializer.JsonPolymorphicConverter]
#endif
public interface IBaseValidationFailure
{
	IObjectPath ObjectPath { get; }
	ValidationSeverity Severity { get; }
	string Message { get; }
	string MessageWithPropertyName { get; }
	bool HasServerCondition { get; }
	string? DetailInfo { get; }
}
