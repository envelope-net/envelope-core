namespace Envelope.Audit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class ApplicationEntryAttribute : Attribute
{
	public string Token { get; }
	public int Version { get; }
	public string? EntityName { get; }
	public string? Description { get; }

	public ApplicationEntryAttribute(string token, int version, string? entityName, string? description = null)
	{
		if (string.IsNullOrWhiteSpace(token))
			throw new ArgumentNullException(nameof(token));

		Token = token;
		Version = version;
		EntityName = entityName;
		Description = description;
	}
}
