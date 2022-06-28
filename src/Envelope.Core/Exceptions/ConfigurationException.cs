using Envelope.Validation;
using System.Runtime.Serialization;

namespace Envelope.Exceptions;

public class ConfigurationException : Exception
{
	public ConfigurationException()
		:base()
	{ }

	public ConfigurationException(List<IValidationMessage>? messages)
		: base((messages != null && 0 < messages.Count)
			? string.Join(Environment.NewLine, messages.Select(x => x.ToString()))
			: throw new ArgumentNullException(nameof(messages)))
	{ }

	public ConfigurationException(string? message)
		: base(message)
	{ }

	public ConfigurationException(string? message, Exception? innerException)
		: base(message, innerException)
	{ }

	protected ConfigurationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
