using Envelope.IOUtils;
using System.Runtime.CompilerServices;

namespace Envelope.Audit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class ApplicationEntryAttribute : Attribute
{
	private const string _directoryDelimiter = "\\";

	public string Token { get; }
	public int Version { get; }
	public string SourceFilePath { get; }
	public string? EntityName { get; }
	public string? Description { get; }

	public ApplicationEntryAttribute(
		string token,
		int version,
		string? entityName,
		string? description = null,
		[CallerFilePath] string sourceFilePath = "")
	{
		if (string.IsNullOrWhiteSpace(token))
			throw new ArgumentNullException(nameof(token));

		if (string.IsNullOrWhiteSpace(sourceFilePath))
			throw new ArgumentNullException(nameof(sourceFilePath));

		Token = token;
		Version = version;
		EntityName = entityName;
		Description = description;

		if (sourceFilePath.Trim().EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase))
		{
			var fileName = Path.GetFileName(sourceFilePath);
			var dir = DirectoryHelper.GetParents(sourceFilePath, 3, _directoryDelimiter);
			SourceFilePath = $"{dir}{_directoryDelimiter}{fileName}";
		}
		else
		{
			SourceFilePath = sourceFilePath;
		}
	}
}
