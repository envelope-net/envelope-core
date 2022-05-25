namespace Envelope.Validation;

public interface IValidationResult
{
	IReadOnlyList<IBaseValidationFailure> Errors { get; }
	bool Interrupted { get; }
}
