#if NET6_0_OR_GREATER
namespace Envelope.Serializer;

public static class JsonDefaultSerializerOptions
{
	public static readonly System.Text.Json.JsonSerializerOptions JsonSerializerOptions =
		new()
		{
			WriteIndented = false,
			ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never //System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
		};
}
#endif
