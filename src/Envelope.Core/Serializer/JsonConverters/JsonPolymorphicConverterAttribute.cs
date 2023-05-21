#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;

namespace Envelope.Serializer;

[AttributeUsage(AttributeTargets.Interface)]
public class JsonPolymorphicConverterAttribute : JsonConverterAttribute
{
	public Type? ReturnType { get; }

	public JsonPolymorphicConverterAttribute(Type? returnType = null)
	{
		ReturnType = returnType;
	}

	public override JsonConverter? CreateConverter(Type typeToConvert)
		=> new JsonPolymorphicConverterFactory(true, ReturnType);
}
#endif
