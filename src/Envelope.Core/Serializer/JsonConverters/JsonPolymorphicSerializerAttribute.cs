#if NET6_0_OR_GREATER
namespace Envelope.Serializer;

[AttributeUsage(AttributeTargets.Interface)]
public class JsonPolymorphicSerializerAttribute : Attribute
{
}
#endif
