using System.Text;

namespace Envelope;

public static class EnvelopeConfiguration
{
	public static Action<StringBuilder, Exception>? SerializeFaultException { get; set; }
}
