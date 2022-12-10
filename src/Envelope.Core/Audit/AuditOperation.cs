namespace Envelope.Audit;

public enum AuditOperation
{
	None = 1,
	Write = 2,
	Delete = 4,
	Write_OR_Delete = 6,
	ReadData = 8
}
