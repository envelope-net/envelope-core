namespace Envelope.Validation;

public interface IInitializationValidable
{
	/// <summary>
	/// Self validation after initialization
	/// </summary>
	/// <param name="propertyPrefix">Parent property prefix - parent property path</param>
	/// <param name="validationBuilder">Validation rule builder</param>
	/// <param name="globalValidationContext">Global objects to control validation</param>
	/// <param name="customValidationContext">Custom objects to control validation</param>
	/// <returns>Null, if no error, otherwise error collection</returns>
	List<IValidationMessage>? ValidateInitialization(
		string? propertyPrefix = null,
		ValidationBuilder? validationBuilder = null,
		Dictionary<string, object>? globalValidationContext = null,
		Dictionary<string, object>? customValidationContext = null);
}
