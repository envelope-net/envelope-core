namespace Envelope.Serializer;

public interface IDictionaryObject
{
	IDictionary<string, object?> ToDictionary(ISerializer? serializer = null);
}
