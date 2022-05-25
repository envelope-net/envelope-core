namespace Envelope.Web;

public static class UserAgentHelper
{
	public static BrowserInfo? GetBrowserInfo(string userAgent)
	{
		if (string.IsNullOrWhiteSpace(userAgent))
			return null;

		userAgent = userAgent.ToLower();

		var result =
			IsIE(userAgent) ??
			IsEdge(userAgent) ??
			IsChrome(userAgent) ??
			IsFirefox(userAgent) ??
			IsSafari(userAgent) ??
			IsOpera(userAgent);

		return result;
	}

	public static BrowserInfo? IsIE(string userAgent)
	{
		if (userAgent.Contains("ie 11.0"))
			return new BrowserInfo
			{
				Type = BrowserType.IE,
				Version = new Version("11.0")
			};

		var ie10 = "msie";
		if (userAgent.Contains(ie10))
		{
			var first = userAgent.IndexOf(ie10);
#if NETSTANDARD2_0 || NETSTANDARD2_1
			var cut = userAgent.Substring(first + ie10.Length + 1);
			var version = cut.Substring(0, cut.IndexOf(';'));
#elif NET6_0_OR_GREATER
			var cut = userAgent[(first + ie10.Length + 1)..];
			var version = cut[..cut.IndexOf(';')];
#endif
			return new BrowserInfo
			{
				Version = ToVersion(version),
				Type = BrowserType.IE
			};
		}

		return null;
	}

	public static BrowserInfo? IsFirefox(string userAgent)
	{
		var firefox = BrowserType.Firefox.ToString().ToLower();
		if (userAgent.Contains(firefox))
		{
			var first = userAgent.IndexOf(firefox);
#if NETSTANDARD2_0 || NETSTANDARD2_1
			var version = userAgent.Substring(first + firefox.Length + 1);
#elif NET6_0_OR_GREATER
			var version = userAgent[(first + firefox.Length + 1)..];
#endif
			return new BrowserInfo
			{
				Version = ToVersion(version),
				Type = BrowserType.Firefox
			};
		}

		return null;
	}

	public static BrowserInfo? IsChrome(string userAgent)
	{
		var chrome = BrowserType.Chrome.ToString().ToLower();
		if (userAgent.Contains(chrome))
		{
			var first = userAgent.IndexOf(chrome);
#if NETSTANDARD2_0 || NETSTANDARD2_1
			var cut = userAgent.Substring(first + chrome.Length + 1);
			var version = cut.Substring(0, cut.IndexOf(' '));
#elif NET6_0_OR_GREATER
			var cut = userAgent[(first + chrome.Length + 1)..];
			var version = cut[..cut.IndexOf(' ')];
#endif
			return new BrowserInfo
			{
				Version = ToVersion(version),
				Type = BrowserType.Chrome
			};
		}

		return null;
	}

	public static BrowserInfo? IsEdge(string userAgent)
	{
		var edge = BrowserType.Edge.ToString().ToLower();
		if (userAgent.Contains(edge))
		{
			var first = userAgent.IndexOf(edge);
#if NETSTANDARD2_0 || NETSTANDARD2_1
			var version = userAgent.Substring(first + edge.Length + 1);
#elif NET6_0_OR_GREATER
			var version = userAgent[(first + edge.Length + 1)..];
#endif
			return new BrowserInfo
			{
				Version = ToVersion(version),
				Type = BrowserType.Edge
			};
		}

		edge = "edg";
		if (userAgent.Contains(edge))
		{
			var first = userAgent.IndexOf(edge);
#if NETSTANDARD2_0 || NETSTANDARD2_1
			var version = userAgent.Substring(first + edge.Length + 1);
#elif NET6_0_OR_GREATER
			var version = userAgent[(first + edge.Length + 1)..];
#endif
			return new BrowserInfo
			{
				Version = ToVersion(version),
				Type = BrowserType.Edge
			};
		}

		return null;
	}

	public static BrowserInfo? IsSafari(string userAgent)
	{
		var safari = BrowserType.Safari.ToString().ToLower();
		if (userAgent.Contains(safari))
		{
			var first = userAgent.IndexOf(safari);
#if NETSTANDARD2_0 || NETSTANDARD2_1
			var version = userAgent.Substring(first + safari.Length + 1);
#elif NET6_0_OR_GREATER
			var version = userAgent[(first + safari.Length + 1)..];
#endif
			return new BrowserInfo
			{
				Version = ToVersion(version),
				Type = BrowserType.Safari
			};
		}

		return null;
	}

	public static BrowserInfo? IsOpera(string userAgent)
	{
		var opera12 = BrowserType.Opera.ToString().ToLower();
		if (userAgent.Contains(opera12))
		{
			var first = userAgent.IndexOf("version");
#if NETSTANDARD2_0 || NETSTANDARD2_1
			var version = userAgent.Substring(first + "version".Length + 1);
#elif NET6_0_OR_GREATER
			var version = userAgent[(first + "version".Length + 1)..];
#endif
			return new BrowserInfo
			{
				Version = ToVersion(version),
				Type = BrowserType.Opera
			};
		}

		var opera15 = "opr";
		if (userAgent.Contains(opera15))
		{
			var first = userAgent.IndexOf(opera15);
#if NETSTANDARD2_0 || NETSTANDARD2_1
			var version = userAgent.Substring(first + opera15.Length + 1);
#elif NET6_0_OR_GREATER
			var version = userAgent[(first + opera15.Length + 1)..];
#endif
			return new BrowserInfo
			{
				Version = ToVersion(version),
				Type = BrowserType.Opera
			};
		}

		return null;
	}

	private static Version ToVersion(string version)
	{
		version = version.Contains(' ')
			? version.Replace(" ", "")
			: version;

		return Version.TryParse(version, out var parsedVersion) ?
			parsedVersion :
			new Version(0, 0);
	}
}
