using Envelope.Extensions;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Envelope.Serializer;

public static class XmlSerializerHelper
{
	private static XmlSerializer GetXmlSerializer(
		Type type,
		XmlAttributeOverrides? overrides,
		Type[]? extraTypes,
		XmlRootAttribute? root,
		string? defaultNamespace,
		string? location)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		XmlSerializer serializer;

		if (string.IsNullOrWhiteSpace(defaultNamespace))
			defaultNamespace = null;

		if (string.IsNullOrWhiteSpace(location))
			location = null;

		if (overrides == null && extraTypes == null && root == null && defaultNamespace == null && location == null)
		{
			serializer = new XmlSerializer(type);
		}
		else
		if (overrides != null && extraTypes == null && root == null && defaultNamespace == null && location == null)
		{
			serializer = new XmlSerializer(type, overrides);
		}
		else
		if (overrides == null && extraTypes != null && root == null && defaultNamespace == null && location == null)
		{
			serializer = new XmlSerializer(type, extraTypes);
		}
		else
		if (overrides == null && extraTypes == null && root != null && defaultNamespace == null && location == null)
		{
			serializer = new XmlSerializer(type, root);
		}
		else
		if (overrides == null && extraTypes == null && root == null && defaultNamespace != null && location == null)
		{
			serializer = new XmlSerializer(type, defaultNamespace);
		}
		else
		{
			serializer = new XmlSerializer(type, overrides, extraTypes, root, defaultNamespace, location);
		}

		return serializer;
	}

	public static object? DeserializeFromXmlFile(
		string xmlFilePath,
		Type type,
		Encoding? encoding = null,
		bool unzipXml = false,
		FileAccess fileAccess = FileAccess.Read,
		FileShare fileSahre = FileShare.ReadWrite,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		if (string.IsNullOrWhiteSpace(xmlFilePath))
			throw new ArgumentNullException(nameof(xmlFilePath));

		if (type == null)
			throw new ArgumentNullException(nameof(type));

		encoding ??= Encoding.UTF8;

		var serializer = GetXmlSerializer(type, overrides, extraTypes, root, defaultNamespace, location);

		using var fileStream = new FileStream(xmlFilePath, FileMode.Open, fileAccess, fileSahre);
		if (unzipXml)
		{
			using var gZipStream = new GZipStream(fileStream, CompressionMode.Decompress, false);
			using var textReader = new StreamReader(gZipStream, encoding);
			var objectFromXml = serializer.Deserialize(textReader);
			return objectFromXml;
		}
		else
		{
			using var textReader = new StreamReader(fileStream, encoding);
			var objectFromXml = serializer.Deserialize(textReader);
			return objectFromXml;
		}
	}

	public static T? DeserializeFromXmlFile<T>(
		string xmlFilePath,
		Encoding? encoding = null,
		bool unzipXml = false,
		FileAccess fileAccess = FileAccess.Read,
		FileShare fileSahre = FileShare.ReadWrite,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		var result = DeserializeFromXmlFile(xmlFilePath, typeof(T), encoding, unzipXml, fileAccess, fileSahre, overrides, extraTypes, root, defaultNamespace, location);
		return result == null
			? default
			: (T)result;
	}

	public static object? DeserializeFromString(
		string xmlObject,
		Type type,
		Encoding? encoding = null,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		if (string.IsNullOrWhiteSpace(xmlObject))
			return null;

		if (type == null)
			throw new ArgumentNullException(nameof(type));

		encoding ??= Encoding.UTF8;

		var serializer = GetXmlSerializer(type, overrides, extraTypes, root, defaultNamespace, location);

		using var memoryStream = xmlObject.ToMemoryStream(encoding);
		using var xmlReader = XmlReader.Create(memoryStream);
		var objectFromXml = serializer.Deserialize(xmlReader);
		return objectFromXml;
	}

	public static T? ReadFromString<T>(
		string xmlObject,
		Encoding? encoding = null,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		var result = DeserializeFromString(xmlObject, typeof(T), encoding, overrides, extraTypes, root, defaultNamespace, location);
		return result == null
			? default
			: (T)result;
	}

	public static object? Deserialize(
		byte[] xmlObject,
		Type type,
		bool unzipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		if (xmlObject == null || xmlObject.Length == 0)
			return null;

		if (type == null)
			throw new ArgumentNullException(nameof(type));

		using var memoryStream = new MemoryStream(xmlObject);
		var objectFromXml = Deserialize(memoryStream, type, unzipXml, overrides, extraTypes, root, defaultNamespace, location);
		return objectFromXml;
	}

	public static T? Deserialize<T>(
		byte[] xmlObject,
		bool unzipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		var result = Deserialize(xmlObject, typeof(T), unzipXml, overrides, extraTypes, root, defaultNamespace, location);
		return result == null
			? default
			: (T)result;
	}

	public static object? Deserialize(
		Stream xmlStream,
		Type type,
		bool unzipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		if (xmlStream == null)
			return null;

		if (type == null)
			throw new ArgumentNullException(nameof(type));

		var serializer = GetXmlSerializer(type, overrides, extraTypes, root, defaultNamespace, location);

		if (unzipXml)
		{
			using var tmpStream = new MemoryStream();
			using var gzipStream = new GZipStream(xmlStream, CompressionMode.Decompress, false);
			gzipStream.BlockCopy(tmpStream);
			tmpStream.Seek(0, SeekOrigin.Begin);
			var objectFromXml = serializer.Deserialize(tmpStream);
			return objectFromXml;
		}
		else
		{
			var objectFromXml = serializer.Deserialize(xmlStream);
			return objectFromXml;
		}
	}

	public static T? Deserialize<T>(
		Stream xmlStream,
		bool unzipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		var result = Deserialize(xmlStream, typeof(T), unzipXml, overrides, extraTypes, root, defaultNamespace, location);
		return result == null
			? default
			: (T)result;
	}

	public static void SerializeToXmlFile(
		string xmlFilePath,
		object obj,
		Type type,
		Encoding? encoding = null,
		bool zipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		if (string.IsNullOrWhiteSpace(xmlFilePath))
			throw new ArgumentNullException(nameof(xmlFilePath));

		if (obj == null)
			throw new ArgumentNullException(nameof(obj));

		if (type == null)
			throw new ArgumentNullException(nameof(type));

		encoding ??= Encoding.UTF8;

		var dir = Path.GetDirectoryName(xmlFilePath);
		if (string.IsNullOrWhiteSpace(dir))
			throw new InvalidOperationException($"No directory {xmlFilePath}");

		if (!Directory.Exists(dir))
			Directory.CreateDirectory(dir);

		var serializer = GetXmlSerializer(type, overrides, extraTypes, root, defaultNamespace, location);

		using var fileStream = new FileStream(xmlFilePath, FileMode.Create);
		if (zipXml)
		{
			using var gZipStream = new GZipStream(fileStream, CompressionMode.Compress, false);
			using var textWriter = new StreamWriter(gZipStream, encoding);
			serializer.Serialize(textWriter, obj);
		}
		else
		{
			using var textWriter = new StreamWriter(fileStream, encoding);
			serializer.Serialize(textWriter, obj);
		}
	}

	public static void SerializeToXmlFile(
		string xmlFilePath,
		object obj,
		Encoding? encoding = null,
		bool zipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
		=> SerializeToXmlFile(
			xmlFilePath,
			obj,
			obj?.GetType()!,
			encoding,
			zipXml,
			overrides,
			extraTypes,
			root,
			defaultNamespace,
			location);

	public static void SerializeToXmlFile<T>(
		string xmlFilePath,
		object obj,
		Encoding? encoding = null,
		bool zipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
		=> SerializeToXmlFile(
			xmlFilePath,
			obj,
			typeof(T),
			encoding,
			zipXml,
			overrides,
			extraTypes,
			root,
			defaultNamespace,
			location);

	public static string SerializeToString(
		object obj,
		Type type,
		Encoding? encoding = null,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null,
		bool indent = false,
		bool omitXmlDeclaration = false)
	{
		if (obj == null)
			throw new ArgumentNullException(nameof(obj));

		if (type == null)
			throw new ArgumentNullException(nameof(type));

		encoding ??= Encoding.UTF8;

		var serializer = GetXmlSerializer(type, overrides, extraTypes, root, defaultNamespace, location);

		var sb = new StringBuilder();
		using (var stringWriter = new StringWriter(sb))
		{
			var settings = new XmlWriterSettings()
			{
				Encoding = encoding,
				Indent = indent,
				OmitXmlDeclaration = omitXmlDeclaration
			};

			using var xmlWriter = XmlWriter.Create(stringWriter, settings);
			serializer.Serialize(xmlWriter, obj);
		}

		string result = sb.ToString();

		if (!omitXmlDeclaration)
			result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\" encoding=\"" + encoding.WebName + "\"?>");

		return result;
	}

	public static string SerializeToString(
		object obj,
		Encoding? encoding = null,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null,
		bool indent = false,
		bool omitXmlDeclaration = false)
		=> SerializeToString(
			obj,
			obj?.GetType()!,
			encoding,
			overrides,
			extraTypes,
			root,
			defaultNamespace,
			location,
			indent,
			omitXmlDeclaration);

	public static string SerializeToString<T>(
		object obj,
		Encoding? encoding = null,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null,
		bool indent = false,
		bool omitXmlDeclaration = false)
		=> SerializeToString(
			obj,
			typeof(T),
			encoding,
			overrides,
			extraTypes,
			root,
			defaultNamespace,
			location,
			indent,
			omitXmlDeclaration);

	public static XmlDocument SerializeToXmlDocument(
		object obj,
		Type type,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		if (obj == null)
			throw new ArgumentNullException(nameof(obj));

		if (type == null)
			throw new ArgumentNullException(nameof(type));

		var xmlDocument = new XmlDocument();
		var xPathNavigator = xmlDocument.CreateNavigator()
			?? throw new InvalidOperationException("xPathNavigator = null");

		using var writer = xPathNavigator.AppendChild();
		var serializer = GetXmlSerializer(type, overrides, extraTypes, root, defaultNamespace, location);
		serializer.Serialize(writer, obj);
		return xmlDocument;
	}

	public static XmlDocument SerializeToXmlDocument(
		object obj,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
		=> SerializeToXmlDocument(
			obj,
			obj?.GetType()!,
			overrides,
			extraTypes,
			root,
			defaultNamespace,
			location);

	public static XmlDocument SerializeToXmlDocument<T>(
		object obj,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
		=> SerializeToXmlDocument(
			obj,
			typeof(T),
			overrides,
			extraTypes,
			root,
			defaultNamespace,
			location);

	public static XmlElement? SerializeToXmlElement(
		object obj,
		Type type,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
		=> SerializeToXmlDocument(obj, type, overrides, extraTypes, root, defaultNamespace, location).DocumentElement;

	public static XmlElement? SerializeToXmlElement(
		object obj,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
		=> SerializeToXmlDocument(obj, obj?.GetType()!, overrides, extraTypes, root, defaultNamespace, location).DocumentElement;

	public static XmlElement? SerializeToXmlElement<T>(
		object obj,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
		=> SerializeToXmlDocument(obj, typeof(T), overrides, extraTypes, root, defaultNamespace, location).DocumentElement;

	public static MemoryStream SerializeToStream(
		object obj,
		Type type,
		Encoding? encoding = null,
		bool zipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		if (obj == null)
			throw new ArgumentNullException(nameof(obj));

		if (type == null)
			throw new ArgumentNullException(nameof(type));

		encoding ??= Encoding.UTF8;

		var serializer = GetXmlSerializer(type, overrides, extraTypes, root, defaultNamespace, location);

		var memoryStream = new MemoryStream();
		var memoryStreamWriter = new StreamWriter(memoryStream, encoding);

		if (zipXml)
		{
			using var tmpStream = new MemoryStream();
			serializer.Serialize(tmpStream, obj);
			tmpStream.Seek(0, SeekOrigin.Begin);
			using var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true);
			tmpStream.BlockCopy(gzipStream);
		}
		else
		{
			serializer.Serialize(memoryStreamWriter, obj);
		}

		memoryStream.Seek(0, SeekOrigin.Begin);

		return memoryStream;
	}

	public static MemoryStream SerializeToStream(
		object obj,
		Encoding? encoding = null,
		bool zipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
		=> SerializeToStream(
			obj,
			obj?.GetType()!,
			encoding,
			zipXml,
			overrides,
			extraTypes,
			root,
			defaultNamespace,
			location);

	public static MemoryStream SerializeToStream<T>(
		object obj,
		Encoding? encoding = null,
		bool zipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
		=> SerializeToStream(
			obj,
			typeof(T),
			encoding,
			zipXml,
			overrides,
			extraTypes,
			root,
			defaultNamespace,
			location);

	public static byte[] SerializeToByteArray(
		object obj,
		Type type,
		Encoding? encoding = null,
		bool zipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		if (obj == null)
			throw new ArgumentNullException(nameof(obj));

		if (type == null)
			throw new ArgumentNullException(nameof(type));

		using MemoryStream memoryStream = SerializeToStream(obj, type, encoding, zipXml, overrides, extraTypes, root, defaultNamespace, location);
		return memoryStream.ToArray();
	}

	public static byte[] SerializeToByteArray(
		object obj,
		Encoding? encoding = null,
		bool zipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		if (obj == null)
			throw new ArgumentNullException(nameof(obj));

		using MemoryStream memoryStream = SerializeToStream(obj, obj?.GetType()!, encoding, zipXml, overrides, extraTypes, root, defaultNamespace, location);
		return memoryStream.ToArray();
	}

	public static byte[] SerializeToByteArray<T>(
		object obj,
		Encoding? encoding = null,
		bool zipXml = false,
		XmlAttributeOverrides? overrides = null,
		Type[]? extraTypes = null,
		XmlRootAttribute? root = null,
		string? defaultNamespace = null,
		string? location = null)
	{
		if (obj == null)
			throw new ArgumentNullException(nameof(obj));

		using MemoryStream memoryStream = SerializeToStream(obj, typeof(T), encoding, zipXml, overrides, extraTypes, root, defaultNamespace, location);
		return memoryStream.ToArray();
	}
}
