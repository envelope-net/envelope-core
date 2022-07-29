namespace Envelope.Calendar;

public class CronTimerSettings
{
	public string Expression { get; }

	public bool IncludeSeconds { get; }

	public CronExpression CronExpression { get; }

	public CronTimerSettings(string expression, bool includeSeconds)
	{
		Expression = !string.IsNullOrWhiteSpace(expression)
			? expression
			: throw new ArgumentNullException(nameof(expression));

		IncludeSeconds = includeSeconds;
		CronExpression = CronExpression.Parse(expression, includeSeconds ? CronFormat.IncludeSeconds : CronFormat.Standard);
	}
}
