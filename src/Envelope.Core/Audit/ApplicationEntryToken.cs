using Envelope.Converters;
using Envelope.Serializer;

namespace Envelope.Audit;

public class ApplicationEntryToken : IDictionaryObject
{
	public Guid IdApplicationEntryToken { get; }
	public string Token { get; }
	public int Version { get; }
	public string SourceFilePath { get; }
	public string? MethodInfo { get; set; }
	public string? MainEntityName { get; set; }
	public string? Description { get; set; }
	public string? TokenHistory { get; set; }

	public ApplicationEntryToken(Guid idApplicationEntryToken, string token, int version, string sourceFilePath)
	{
		if (string.IsNullOrWhiteSpace(token))
			throw new ArgumentNullException(nameof(token));

		if (string.IsNullOrWhiteSpace(sourceFilePath))
			throw new ArgumentNullException(nameof(sourceFilePath));

		IdApplicationEntryToken = idApplicationEntryToken;
		Token = token;
		Version = version;
		SourceFilePath = sourceFilePath;
	}

	public ApplicationEntryToken(string token, int version, string sourceFilePath)
	{
		if (string.IsNullOrWhiteSpace(token))
			throw new ArgumentNullException(nameof(token));

		if (string.IsNullOrWhiteSpace(sourceFilePath))
			throw new ArgumentNullException(nameof(sourceFilePath));

		Token = token;
		Version = version;
		SourceFilePath = sourceFilePath;
		IdApplicationEntryToken = GuidConverter.ToGuid($"{Token}_{Version}_{SourceFilePath}");
	}

	public ApplicationEntryToken WriteToHistory()
	{
		if (string.IsNullOrWhiteSpace(TokenHistory))
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1
			TokenHistory = Newtonsoft.Json.JsonConvert.SerializeObject(new List<ApplicationEntryTokenHistory>
			{
				new ApplicationEntryTokenHistory
				{
					MethodInfo = MethodInfo,
					MainEntityName = MainEntityName,
					Description = Description
				}
			});
#elif NET6_0_OR_GREATER
			TokenHistory = System.Text.Json.JsonSerializer.Serialize(new List<ApplicationEntryTokenHistory>
			{
				new ApplicationEntryTokenHistory
				{
					MethodInfo = MethodInfo,
					MainEntityName = MainEntityName,
					Description = Description
				}
			});
#endif
		}
		else
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1
			var historyList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ApplicationEntryTokenHistory>>(TokenHistory!);
			if (historyList == null)
			{
				TokenHistory = Newtonsoft.Json.JsonConvert.SerializeObject(new List<ApplicationEntryTokenHistory>
				{
					new ApplicationEntryTokenHistory
					{
						MethodInfo = MethodInfo,
						MainEntityName = MainEntityName,
						Description = Description
					}
				});
			}
			else
			{
				if (!historyList.Any(x => x.MethodInfo == MethodInfo && x.MainEntityName == MainEntityName && x.Description == Description))
				{
					historyList.Add(
						new ApplicationEntryTokenHistory
						{
							MethodInfo = MethodInfo,
							MainEntityName = MainEntityName,
							Description = Description
						});

					TokenHistory = Newtonsoft.Json.JsonConvert.SerializeObject(historyList);
				}
			}
#elif NET6_0_OR_GREATER
			var historyList = System.Text.Json.JsonSerializer.Deserialize<List<ApplicationEntryTokenHistory>>(TokenHistory);
			if (historyList == null)
			{
				TokenHistory = System.Text.Json.JsonSerializer.Serialize(new List<ApplicationEntryTokenHistory>
				{
					new ApplicationEntryTokenHistory
					{
						MethodInfo = MethodInfo,
						MainEntityName = MainEntityName,
						Description = Description
					}
				});
			}
			else
			{
				if (!historyList.Any(x => x.MethodInfo == MethodInfo && x.MainEntityName == MainEntityName && x.Description == Description))
				{
					historyList.Add(
						new ApplicationEntryTokenHistory
						{
							MethodInfo = MethodInfo,
							MainEntityName = MainEntityName,
							Description = Description
						});

					TokenHistory = System.Text.Json.JsonSerializer.Serialize(historyList);
				}
			}
#endif
		}

		return this;
	}

	public IDictionary<string, object?> ToDictionary(ISerializer? serializer = null)
	{
		var dict = new Dictionary<string, object?>
		{
			{ nameof(IdApplicationEntryToken), IdApplicationEntryToken },
			{ nameof(Version), Version }
		};

		if (!string.IsNullOrWhiteSpace(Token))
			dict.Add(nameof(Token), Token);

		if (!string.IsNullOrWhiteSpace(SourceFilePath))
			dict.Add(nameof(SourceFilePath), SourceFilePath);

		if (!string.IsNullOrWhiteSpace(MethodInfo))
			dict.Add(nameof(MethodInfo), MethodInfo);

		if (!string.IsNullOrWhiteSpace(MainEntityName))
			dict.Add(nameof(MainEntityName), MainEntityName);

		if (!string.IsNullOrWhiteSpace(Description))
			dict.Add(nameof(Description), Description);

		if (!string.IsNullOrWhiteSpace(TokenHistory))
			dict.Add(nameof(TokenHistory), TokenHistory);

		return dict;
	}

	public override string? ToString()
		=> Token;
}
