using System.Diagnostics.CodeAnalysis;

namespace Envelope.Extensions;

public static class DateTimeExtensions
{
	public static DateTime ToEndOfDay(this DateTime dateTime)
		=> dateTime.Date.AddDays(1).AddMilliseconds(-1);

	[return: NotNullIfNotNull("dateTime")]
	public static DateTime? ToEndOfDay(this DateTime? dateTime)
		=> dateTime?.Date.AddDays(1).AddMilliseconds(-1);

	public static string ToUtcFormat(this DateTime dateTime)
		=> dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); //The last K on string will be changed to 'Z' if the date is UTC or with timezone (+-hh:mm) if is local.

	public static string ToWebFormat(this DateTime dateTime)
		=> dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"); //The last K on string will be changed to 'Z' if the date is UTC or with timezone (+-hh:mm) if is local.
	public static DateTimeOffset ToEndOfDay(this DateTimeOffset dateTimeOffset)
		=> dateTimeOffset.Date.AddDays(1).AddMilliseconds(-1);

	[return: NotNullIfNotNull("dateTime")]
	public static DateTimeOffset? ToEndOfDay(this DateTimeOffset? dateTimeOffset)
		=> dateTimeOffset?.Date.AddDays(1).AddMilliseconds(-1);

	public static string ToUtcFormat(this DateTimeOffset dateTimeOffset)
		=> dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); //The last K on string will be changed to 'Z' if the date is UTC or with timezone (+-hh:mm) if is local.

	public static string ToWebFormat(this DateTimeOffset dateTimeOffset)
		=> dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffK"); //The last K on string will be changed to 'Z' if the date is UTC or with timezone (+-hh:mm) if is local.

	public static DateTime? SetKindUtc(this DateTime? dateTime)
	{
		if (dateTime.HasValue)
		{
			return dateTime.Value.SetKindUtc();
		}
		else
		{
			return null;
		}
	}

	public static DateTime SetKindUtc(this DateTime dateTime)
	{
		if (dateTime.Kind == DateTimeKind.Utc)
			return dateTime;

		return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
	}
}
