using System.Text;

namespace Envelope.Serializer;

public interface ITextSerializer
{
	void WriteTo(StringBuilder sb, string? before = null, string? after = null);
}
