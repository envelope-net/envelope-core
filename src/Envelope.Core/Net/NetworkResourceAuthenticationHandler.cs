using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

namespace Envelope.Net;

/*
USAGE:
try
{
	networkResourceAuthenticationHandler = new NetworkResourceAuthenticationHandler(
		AppSettings.Instance.ServiceBusSettings.JobSettings.DownloadEdeskJob.LocalEdeskFolder,
		new NetworkCredential(
			AppSettings.Instance.ServiceBusSettings.JobSettings.DownloadEdeskJob.LocalEdeskFolderUserName,
			AppSettings.Instance.ServiceBusSettings.JobSettings.DownloadEdeskJob.LocalEdeskFolderUserPassword,
			AppSettings.Instance.ServiceBusSettings.JobSettings.DownloadEdeskJob.LocalEdeskFolderDomainName ?? string.Empty));
}
finally
{
	networkResourceAuthenticationHandler.Dispose();
}
*/

/// <summary>
/// FileShare, Shared folder auth
/// </summary>
public class NetworkResourceAuthenticationHandler : IDisposable
{
	readonly string _networkName;

	public NetworkResourceAuthenticationHandler(string networkName, NetworkCredential credentials)
	{
		if (string.IsNullOrWhiteSpace(networkName))
			throw new ArgumentNullException(nameof(networkName));

		if (credentials == null)
			throw new ArgumentNullException(nameof(credentials));

		_networkName = networkName;

		var netResource = new NetResource
		{
			Scope = ResourceScope.GlobalNetwork,
			ResourceType = ResourceType.Disk,
			DisplayType = ResourceDisplaytype.Share,
			RemoteName = networkName
		};

		var userName = string.IsNullOrEmpty(credentials.Domain)
			? credentials.UserName
			: string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

		var result = WNetAddConnection2(
			netResource,
			credentials.Password,
			userName,
			0);

		//https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes
		if (result != 0)
			throw new Win32Exception(result, $"Error '{result}' connecting to network resource - {networkName} with credentials for user {credentials.UserName}");
	}

	~NetworkResourceAuthenticationHandler()
	{
		Dispose(false);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		_ = WNetCancelConnection2(_networkName, 0, true);
	}

	[DllImport("mpr.dll")]
	private static extern int WNetAddConnection2(NetResource netResource,
		string password, string username, int flags);

	[DllImport("mpr.dll")]
	private static extern int WNetCancelConnection2(string name, int flags,
		bool force);

	[StructLayout(LayoutKind.Sequential)]
	public class NetResource
	{
		public ResourceScope Scope;
		public ResourceType ResourceType;
		public ResourceDisplaytype DisplayType;
		public int Usage;
		public string LocalName;
		public string RemoteName;
		public string Comment;
		public string Provider;
	}

	public enum ResourceScope : int
	{
		Connected = 1,
		GlobalNetwork,
		Remembered,
		Recent,
		Context
	};

	public enum ResourceType : int
	{
		Any = 0,
		Disk = 1,
		Print = 2,
		Reserved = 8,
	}

	public enum ResourceDisplaytype : int
	{
		Generic = 0x0,
		Domain = 0x01,
		Server = 0x02,
		Share = 0x03,
		File = 0x04,
		Group = 0x05,
		Network = 0x06,
		Root = 0x07,
		Shareadmin = 0x08,
		Directory = 0x09,
		Tree = 0x0a,
		Ndscontainer = 0x0b
	}
}
