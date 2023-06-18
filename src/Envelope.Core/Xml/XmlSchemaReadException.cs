using System.Xml.Schema;

namespace Envelope.Xml;

public class XmlSchemaReadException : Exception
{
	public IReadOnlyList<ValidationEventArgs> ValidationEventArgs { get; }

	public XmlSchemaReadException(IReadOnlyList<ValidationEventArgs> validationEventArgs)
		: base()
	{
		ValidationEventArgs = validationEventArgs ?? throw new ArgumentNullException(nameof(validationEventArgs));
	}

	public XmlSchemaReadException(IReadOnlyList<ValidationEventArgs> validationEventArgs, string? message)
		: base(message)
	{
		ValidationEventArgs = validationEventArgs ?? throw new ArgumentNullException(nameof(validationEventArgs));
	}

	public XmlSchemaReadException(IReadOnlyList<ValidationEventArgs> validationEventArgs, string? message, Exception? innerException)
		: base(message, innerException)
	{
		ValidationEventArgs = validationEventArgs ?? throw new ArgumentNullException(nameof(validationEventArgs));
	}
}
