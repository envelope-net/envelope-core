namespace Envelope.Serializer;

public interface IFormDataSerializable
{
	List<KeyValuePair<string, string>> Serialize(string? prefix);
}
