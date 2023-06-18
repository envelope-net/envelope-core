using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Envelope.Xml;

public static class XmlHelper
{
	public static XNamespace? GetRootXmlns(Stream xmlStream)
	{
		if (xmlStream == null)
			throw new ArgumentNullException(nameof(xmlStream));

		try
		{
			if (xmlStream.CanSeek == true)
				xmlStream.Seek(0, SeekOrigin.Begin);

			var xdoc = XDocument.Load(xmlStream);
			var namespaces =
				xdoc.Root.Attributes()
					.Where(a => a.IsNamespaceDeclaration)
					.GroupBy(a => a.Name.Namespace == XNamespace.None ? string.Empty : a.Name.LocalName,
							a => XNamespace.Get(a.Value))
					.ToDictionary(g => g.Key,
						g => g.First());

			if (namespaces.TryGetValue(string.Empty, out XNamespace xnamespace))
			{
				return xnamespace;
			}
			else
			{
				return null;
			}
		}
		finally
		{
			if (xmlStream.CanSeek == true)
			{
				try
				{
					xmlStream.Seek(0, SeekOrigin.Begin);
				}
				catch { }
			}
		}
	}

	public static ValidationEventArgs[]? ValidateXml(Stream xmlStream, params Stream[] xsdStreams)
	{
		if (xmlStream == null)
			throw new ArgumentNullException(nameof(xmlStream));

		if (xsdStreams == null || xsdStreams.Length == 0)
			throw new ArgumentNullException(nameof(xsdStreams));

		try
		{
			if (xmlStream.CanSeek == true)
				xmlStream.Seek(0, SeekOrigin.Begin);

			var result = new List<ValidationEventArgs>();

			var settings = new XmlReaderSettings
			{
				ValidationType = ValidationType.Schema
			};
			settings.ValidationEventHandler += (sender, validationEventArgs) => result.Add(validationEventArgs);

			var i = 0;
			foreach (var xsdStream in xsdStreams)
			{
				if (xsdStream.CanSeek == true)
					xsdStream.Seek(0, SeekOrigin.Begin);

				var schema = XmlSchema.Read(xsdStream, (sender, validationEventArgs) => result.Add(validationEventArgs));

				var isValid = result.Count == 0;

				if (isValid)
					settings.Schemas.Add(schema);
				else
					throw new XmlSchemaReadException(result, $"{nameof(xsdStreams)}[{i}]");

				i++;
			}

			var xmlFile = XmlReader.Create(xmlStream, settings);

			while (xmlFile.Read()) ;

			return 0 < result.Count
				? result.ToArray()
				: null; //no error, no warning
		}
		finally
		{
			if (xmlStream.CanSeek == true)
			{
				try
				{
					xmlStream.Seek(0, SeekOrigin.Begin);
				}
				catch { }
			}

			if (0 < xsdStreams?.Length)
			{
				foreach (var xsdStream in xsdStreams)
				{
					if (xsdStream.CanSeek == true)
					{
						try
						{
							xsdStream.Seek(0, SeekOrigin.Begin);
						}
						catch { }
					}
				}
			}
		}
	}
}
