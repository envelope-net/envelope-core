namespace Envelope.Validation;

public interface IValidable
{
	/// <summary>
	/// Self validation
	/// </summary>
	/// <param name="propertyPrefix">Parent property prefix - parent property path</param>
	/// <param name="parentErrorBuffer">Errors catched by parent</param>
	/// <param name="validationContext">Custom objects to control validation</param>
	/// <returns>Null, if no error, StringBuilder with formatted errors</returns>
	List<IValidationMessage>? Validate(string? propertyPrefix = null, List<IValidationMessage>? parentErrorBuffer = null, Dictionary<string, object>? validationContext = null);
}
