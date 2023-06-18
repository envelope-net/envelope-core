namespace Envelope;

public class GlobalContext : IGlobalContext
{
	private static readonly Lazy<IGlobalContext> _instanceFactory = new(() => new GlobalContext());

	private static IGlobalContext _instance;
	public static IGlobalContext Instance
	{
		get => _instance ??= _instanceFactory.Value;
		set => _instance = value;
	}

	/// <summary>
	/// Gets a System.DateTime object that is set to the current date and time on this computer, expressed as the local time.
	/// </summary>
	public DateTime Now => DateTime.Now;

	/// <summary>
	/// Gets the current date.
	/// </summary>
	public DateTime Today => DateTime.Today;

	/// <summary>
	/// Gets a System.DateTime object that is set to the current date and time on this computer, expressed as the Coordinated Universal Time (UTC)
	/// </summary>
	public DateTime UtcNow => DateTime.UtcNow;

	/// <summary>
	/// Initializes a new instance of the System.Guid structure.
	/// </summary>
	/// <returns>A new GUID object.</returns>
	public Guid NewGuid() => Guid.NewGuid();
}
