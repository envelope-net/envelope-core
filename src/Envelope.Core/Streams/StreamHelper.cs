using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Envelope.Streams;

public static class StreamHelper
{
	public static byte[] ToArray(Stream stream)
	{
		if (stream == null)
			throw new ArgumentNullException(nameof(stream));

		byte[]? byteArray = (stream as MemoryStream)?.ToArray();

		if (byteArray == null)
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				byteArray = ms.ToArray();
			}

		return byteArray;
	}

	public static MemoryStream ToStream(byte[] bytes)
	{
		if (bytes == null)
			throw new ArgumentNullException(nameof(bytes));

		var stream = new MemoryStream(bytes);
		stream.Seek(0, SeekOrigin.Begin);
		return stream;
	}

	public static Stream WriteToStream(byte[] sourceBytes, Stream destinationStream)
	{
		if (sourceBytes == null)
			throw new ArgumentNullException(nameof(sourceBytes));

		if (destinationStream == null)
			return ToStream(sourceBytes);

		using (var writer = new BinaryWriter(destinationStream))
		{
			writer.Write(sourceBytes);
		}
		return destinationStream;
	}

	//public static byte[] ToArrayForDotNet3_5(Stream stream)
	//{
	//	if (stream == null)
	//		throw new ArgumentNullException(nameof(stream));

	//	byte[] buffer = new byte[32768];
	//	using (MemoryStream ms = new MemoryStream())
	//	{
	//		while (true)
	//		{
	//			int read = stream.Read(buffer, 0, buffer.Length);
	//			if (read <= 0)
	//				return ms.ToArray();
	//			ms.Write(buffer, 0, read);
	//		}
	//	}
	//}

	[return: NotNullIfNotNull("stream")]
	public static string? ToString(Stream stream, Encoding? encoding = null, bool seek = false)
	{
		if (stream == null)
			return null;

		if (encoding == null)
			encoding = Encoding.UTF8;

		if (seek)
			stream.Seek(0, SeekOrigin.Begin);

		using var reader = new StreamReader(stream, encoding);
		return reader.ReadToEnd();
	}

	[return: NotNullIfNotNull("stream")]
	public static Task<string?> ToStringAsync(Stream stream, Encoding? encoding = null, bool seek = false)
	{
		if (stream == null)
			return Task.FromResult((string?)null);

		if (encoding == null)
			encoding = Encoding.UTF8;

		if (seek)
			stream.Seek(0, SeekOrigin.Begin);

		using var reader = new StreamReader(stream, encoding);
		return reader.ReadToEndAsync()!;
	}
}
