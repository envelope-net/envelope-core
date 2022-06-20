namespace Envelope.Web;

public static class ContentTypeHelper
{
	private static readonly Lazy<IReadOnlyList<string>> _stringContentTypes = new(() => new List<string>
		{
			"application/x-www-form-urlencoded",
			"application/ecmascript",
			"application/javascript",
			"application/json",
			"application/problem+json",
			"application/jsonml+json",
			"application/plain",
			"application/vnd.dvb.service",
			"application/xhtml+xml",
			"application/x-javascript",
			"application/xml",
			"text/ecmascript",
			"text/html",
			"text/javascript",
			"text/plain",
			"text/xml"
		});

	public static IReadOnlyList<string> StringContentTypes => _stringContentTypes.Value;
}
