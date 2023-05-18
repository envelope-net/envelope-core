using System.Runtime.InteropServices;

namespace Envelope.Infrastructure;

public static class OSPlatformHelper
{
	public const string BROWSER = "BROWSER";

	public static bool IsWindows()
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	public static bool IsOSX()
		=> RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	public static bool IsLinux()
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	public static bool IsBrowser()
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Create(BROWSER));

#if (NETSTANDARD2_0 || NETSTANDARD2_1 || NET6_0_OR_GREATER)
	public static string GetOSPlatform()
	{
		var platform = OSPlatform.Create("Other Platform");

		var isWindows = IsWindows();
		platform = isWindows ? OSPlatform.Windows : platform;

		var isOsx = IsOSX();
		platform = isOsx ? OSPlatform.OSX : platform;

		var isLinux = IsLinux();
		platform = isLinux ? OSPlatform.Linux : platform;

		if (IsBrowser())
			return BROWSER;

		return platform.ToString();
	}
#else
	public static string GetOSPlatform() { return Environment.OSVersion.Platform.ToString(); }
#endif
}
