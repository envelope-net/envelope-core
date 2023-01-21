namespace Envelope.Validation;

#if NET6_0_OR_GREATER
[Envelope.Serializer.JsonPolymorphicConverter]
#endif
public interface IValidationResult
{
	IReadOnlyList<IBaseValidationFailure> Errors { get; }
	bool Interrupted { get; }
}
