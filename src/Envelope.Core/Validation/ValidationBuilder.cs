namespace Envelope.Validation;

public class ValidationBuilder
{
	internal readonly List<IValidationMessage> _messages;

	public IReadOnlyList<IValidationMessage> Messages => _messages;

	public ValidationBuilder()
	{
		_messages = new();
	}

	public List<IValidationMessage> Build()
		=> _messages;

	public ValidationRuleBuilder SetValidationMessages(string? prefix, Dictionary<string, object>? globalValidationContext)
		=> new(prefix, globalValidationContext!, this);
}
