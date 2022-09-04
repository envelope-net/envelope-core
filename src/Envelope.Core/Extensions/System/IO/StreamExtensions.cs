using Envelope.Streams;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Envelope.Extensions;

public static class StreamExtensions
{
	public static void BlockCopy(this Stream source, Stream target, int blockSize = 65536)
	{
		int read;
		byte[] buffer = new byte[blockSize];
		while ((read = source.Read(buffer, 0, blockSize)) > 0)
		{
			target.Write(buffer, 0, read);
		}
	}

	public static byte[] ToArray(this Stream stream)
		=> StreamHelper.ToArray(stream);


	[return: NotNullIfNotNull("stream")]
	public static string? ToString(this Stream stream, Encoding? encoding = null, bool seek = false)
		=> StreamHelper.ToString(stream, encoding, seek);

	[return: NotNullIfNotNull("stream")]
	public static Task<string?> ToStringAsync(this Stream stream, Encoding? encoding = null, bool seek = false)
		=> StreamHelper.ToStringAsync(stream, encoding, seek);
}
