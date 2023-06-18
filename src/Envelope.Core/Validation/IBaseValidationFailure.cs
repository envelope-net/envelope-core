using Envelope.Reflection.ObjectPaths;

namespace Envelope.Validation;

#if NET6_0_OR_GREATER
[Envelope.Serializer.JsonPolymorphicConverter]
#endif
public interface IBaseValidationFailure : IValidationMessage
{
	IObjectPath ObjectPath { get; }
	string MessageWithPropertyName { get; }
	bool HasServerCondition { get; }
	string? DetailInfo { get; }
}
