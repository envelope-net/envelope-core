using System.Xml.Serialization;

namespace Envelope.Collections;

[XmlRoot("dictionary")]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue?>, IXmlSerializable
	where TKey : notnull
{
	public System.Xml.Schema.XmlSchema? GetSchema()
	{
		return null;
	}

	public void ReadXml(System.Xml.XmlReader reader)
	{
		var keySerializer = new XmlSerializer(typeof(TKey));
		var valueSerializer = new XmlSerializer(typeof(TValue));

		bool wasEmpty = reader.IsEmptyElement;
		reader.Read();

		if (wasEmpty)
			return;

		while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
		{
			reader.ReadStartElement("item");

			reader.ReadStartElement("key");

			if (keySerializer.Deserialize(reader) is not TKey key)
				throw new InvalidOperationException("Missing key");

			reader.ReadEndElement();

			reader.ReadStartElement("value");

			if (valueSerializer.Deserialize(reader) is not TValue value)
				value = default!;

			reader.ReadEndElement();

			this.Add(key, value);

			reader.ReadEndElement();
			reader.MoveToContent();
		}
		reader.ReadEndElement();
	}

	public void WriteXml(System.Xml.XmlWriter writer)
	{
		var keySerializer = new XmlSerializer(typeof(TKey));
		var valueSerializer = new XmlSerializer(typeof(TValue));

		foreach (TKey key in this.Keys)
		{
			writer.WriteStartElement("item");

			writer.WriteStartElement("key");
			keySerializer.Serialize(writer, key);
			writer.WriteEndElement();

			writer.WriteStartElement("value");
			TValue? value = this[key];
			valueSerializer.Serialize(writer, value);
			writer.WriteEndElement();

			writer.WriteEndElement();
		}
	}
}
