using System.Runtime.Serialization;

namespace Envelope.Exceptions;

public class TransactionObserverConnectionException : Exception
{
	public TransactionObserverConnectionException()
		: base()
	{ }

	public TransactionObserverConnectionException(string? message)
		: base(message)
	{ }

	public TransactionObserverConnectionException(string? message, Exception? innerException)
		: base(message, innerException)
	{ }

	protected TransactionObserverConnectionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
