namespace Envelope.MathUtils;

public static class MathHelper
{
	public static string? GetDecimalSeparator()
	{
		if (Thread.CurrentThread != null
			&& Thread.CurrentThread.CurrentCulture != null
			&& Thread.CurrentThread.CurrentCulture.NumberFormat != null
			&& !string.IsNullOrWhiteSpace(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator))
		{
			return Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
		}
		else
		{
			return null;
		}
	}

	public static int? IntParseSafe(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return null;

		if (int.TryParse(text, out int value))
			return value;

		return null;
	}

	public static long? LongParseSafe(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return null;

		if (long.TryParse(text, out long value))
			return value;

		return null;
	}
}
