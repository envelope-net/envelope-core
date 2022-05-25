namespace Envelope.Exceptions;

public sealed class EnvelopeException : Exception
{
	public EnvelopeException()
		:base()
	{ }

	public EnvelopeException(string? message)
		: base(message)
	{ }

	public EnvelopeException(string? message, Exception? innerException)
		: base(message, innerException)
	{ }
}
