namespace Envelope;

public static class Constants
{
	public static class Guids
	{
		private static readonly Lazy<Guid> _FULL = new(() => new("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"));
		public static Guid FULL => _FULL.Value;
	}
}
