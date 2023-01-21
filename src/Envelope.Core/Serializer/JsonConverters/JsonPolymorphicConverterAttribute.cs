#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;

namespace Envelope.Serializer;

[AttributeUsage(AttributeTargets.Interface)]
public class JsonPolymorphicConverterAttribute : JsonConverterAttribute
{
	public override JsonConverter? CreateConverter(Type typeToConvert)
		=> new JsonPolymorphicConverterFactory(true);
}
#endif
