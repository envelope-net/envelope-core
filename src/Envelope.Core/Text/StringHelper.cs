using Envelope.Extensions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Envelope.Text;

public class CharInfo
{
	public char? Char { get; private set; }
	public int Int => Char ?? -1;
	public string? Hex { get; private set; }

	public CharInfo()
	{
	}

	public CharInfo(char character)
	{
		Char = character;
		Hex = Convert.ToInt32(character).ToString("x");
	}

	public CharInfo(int character)
		: this((char)character)
	{
	}

	public CharInfo(string hex)
	{
		Hex = hex;
		int value = Convert.ToInt32(hex, 16);
		//string stringValue = Char.ConvertFromUtf32(value);
		Char = (char)value;
	}

	public override string ToString()
	{
		return Char + " = 0x" + Hex;
	}
}

public static class StringHelper
{
	private static readonly Lazy<List<string>> _xmlBeautifyContentTypes = new(() => new List<string>
	{
		"xhtml",
		"xml"
	});

	private static readonly Lazy<List<string>> _jsonBeautifyContentTypes = new(() => new List<string>
	{
		"json"
	});

	public class CharDefinition
	{
		public char Char { get; }
		public bool IsWhiteSpace { get; }
		public bool IsDigit { get; }
		public bool IsLower { get; }
		public bool IsUpper { get; }
		public bool IsInvalid { get; }

		public CharDefinition(char @char)
		{
			Char = @char;
			IsDigit = DIGITS.Contains(@char);
			if (IsDigit)
			{
				IsLower = false;
				IsUpper = false;
				IsWhiteSpace = false;
			}
			else
			{
				IsLower = LOWER_CHARS.Contains(@char);
				if (IsLower)
				{
					IsUpper = false;
					IsWhiteSpace = false;
				}
				else
				{
					IsUpper = UPPER_CHARS.Contains(@char);
					if (IsUpper)
					{
						IsWhiteSpace = false;
					}
					else
					{
						IsWhiteSpace = char.IsWhiteSpace(@char);
						if (!IsWhiteSpace)
							IsInvalid = true;
					}
				}
			}
		}
	}

	public static class CharDefinitionCache
	{
		private static readonly ConcurrentDictionary<char, CharDefinition> _cache = new();

		public static CharDefinition GetCharDefinition(char @char)
			=> _cache.AddOrGet(@char, ch => new CharDefinition(ch));
	}

	public const string DIGITS = @"0123456789";
	public const string AVAILABLE_FIRST_CHARS = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ_";
	public const string UPPER_CHARS = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	public const string LOWER_CHARS = @"abcdefghijklmnopqrstuvwxyz";
	public const string AVAILABLE_CHARS = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789";
	public const string AVAILABLE_CHARS_NoUnderscore = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

	public static string ToCammelCase(string text, bool strictCammelCase = false, bool removeUnderscores = true, bool throwIfEmpty = true)
	{
		if (string.IsNullOrWhiteSpace(text))
			return text;

		var digits = new List<char>(DIGITS.ToCharArray());
		var firstChars = new List<char>(AVAILABLE_FIRST_CHARS.ToCharArray());
		firstChars.AddRange(AVAILABLE_FIRST_CHARS.ToLower().ToCharArray());
		List<char> allChars;
		if (removeUnderscores)
		{
			allChars = new List<char>(AVAILABLE_CHARS_NoUnderscore.ToCharArray());
			allChars.AddRange(AVAILABLE_CHARS_NoUnderscore.ToLower().ToCharArray());
		}
		else
		{
			allChars = new List<char>(AVAILABLE_CHARS.ToCharArray());
			allChars.AddRange(AVAILABLE_CHARS_NoUnderscore.ToLower().ToCharArray());
		}
		var normalizedTextBuilder = new StringBuilder();
		bool isFirst = true;
		bool toUpper = false;
		text = RemoveAccents(text);
		foreach (var ch in text.ToCharArray())
		{
			if (isFirst)
			{
				if (firstChars.Contains(ch))
				{
					normalizedTextBuilder.Append(char.ToUpper(ch));
					isFirst = false;
				}
				else if (digits.Contains(ch))
				{
					normalizedTextBuilder.Append($"_{ch}");
					isFirst = false;
				}
			}
			else
			{
				if (allChars.Contains(ch))
				{
					if (toUpper)
					{
						normalizedTextBuilder.Append(char.ToUpper(ch));
						toUpper = false;
					}
					else
					{
						normalizedTextBuilder.Append(ch);
					}
				}
				else
				{
					toUpper = true;
				}
			}
		}

		string normalizedText = normalizedTextBuilder.ToString();

		if (string.IsNullOrWhiteSpace(normalizedText))
		{
			if (throwIfEmpty || removeUnderscores)
				throw new Exception("Text '" + text + "' cannot be normalized.");
			else
				normalizedText = "_";
		}

		string result;
		if (strictCammelCase)
		{
			var resultBuilder = new StringBuilder();
			bool previousWasUpper = false;
			foreach (var ch in normalizedText)
			{
				if (ch == '_')
					continue;

				if (char.IsUpper(ch))
				{
					if (previousWasUpper)
						resultBuilder.Append(char.ToLower(ch));
					else
						resultBuilder.Append(ch);

					previousWasUpper = true;
				}
				else
				{
					resultBuilder.Append(ch);
					previousWasUpper = false;
				}
			}
			result = resultBuilder.ToString();
		}
		else
		{
			result = normalizedText;
		}

		if (string.IsNullOrWhiteSpace(result))
			return "_";

		return result;
	}

	public static string ToSnakeCase(string text, bool throwIfEmpty = true)
		=> ToDelimitedCase(text, '_', throwIfEmpty);

	public static string ToKebabCase(string text, bool throwIfEmpty = true)
		=> ToDelimitedCase(text, '-', throwIfEmpty);

	public static string ToDelimitedCase(string text, char delimiter, bool throwIfEmpty = true)
	{
		if (string.IsNullOrWhiteSpace(text))
			return text;

		var sb = new StringBuilder();

		text = RemoveAccents(text);
		var charArray = text.ToCharArray();
		var charDefinitionArray = new CharDefinition[charArray.Length];
		for (int i = 0; i < charArray.Length; i++)
			charDefinitionArray[i] = CharDefinitionCache.GetCharDefinition(charArray[i]);

		var current = charDefinitionArray[0];
		if (current.IsDigit || current.IsLower)
		{
			sb.Append(current.Char);
		}
		else if (current.IsUpper)
		{
			sb.Append(char.ToLower(current.Char));
		}

		for (int i = 1; i < charDefinitionArray.Length; i++)
		{
			current = charDefinitionArray[i];
			var before = charDefinitionArray[i - 1];

			if (i == charDefinitionArray.Length - 1) //last
			{
				if (current.IsLower)
				{
					if (before.IsLower)
					{
						sb.Append(current.Char);
					}
					else if (before.IsUpper)
					{
						sb.Append(current.Char);
					}
					else if (before.IsDigit)
					{
						AppendWithDelimiter(sb, delimiter, current.Char);
					}
					else
					{
						AppendWithDelimiter(sb, delimiter, current.Char);
					}
				}
				else if (current.IsUpper)
				{
					if (before.IsLower)
					{
						AppendWithDelimiter(sb, delimiter, char.ToLower(current.Char));
					}
					else if (before.IsUpper)
					{
						sb.Append(char.ToLower(current.Char));
					}
					else if (before.IsDigit)
					{
						AppendWithDelimiter(sb, delimiter, char.ToLower(current.Char));
					}
					else
					{
						AppendWithDelimiter(sb, delimiter, char.ToLower(current.Char));
					}
				}
				else if (current.IsDigit)
				{
					if (before.IsLower)
					{
						AppendWithDelimiter(sb, delimiter, current.Char);
					}
					else if (before.IsUpper)
					{
						AppendWithDelimiter(sb, delimiter, current.Char);
					}
					else if (before.IsDigit)
					{
						sb.Append(current.Char);
					}
					else
					{
						AppendWithDelimiter(sb, delimiter, current.Char);
					}
				}
			}
			else //in the middle
			{
				var after = charDefinitionArray[i + 1];
				if (current.IsLower)
				{
					if (before.IsLower)
					{
						sb.Append(current.Char);
					}

					else if (before.IsUpper)
					{
						sb.Append(current.Char);
					}

					else if (before.IsDigit)
					{
						AppendWithDelimiter(sb, delimiter, current.Char);
					}
					else
					{
						AppendWithDelimiter(sb, delimiter, current.Char);
					}
				}
				else if (current.IsUpper)
				{
					if (before.IsLower)
					{
						AppendWithDelimiter(sb, delimiter, char.ToLower(current.Char));
					}

					else if (before.IsUpper)
					{
						if (after.IsLower)
						{
							AppendWithDelimiter(sb, delimiter, char.ToLower(current.Char));
						}
						else if (after.IsUpper)
						{
							sb.Append(char.ToLower(current.Char));
						}
						else if (after.IsDigit)
						{
							sb.Append(char.ToLower(current.Char));
						}
						else
						{
							sb.Append(char.ToLower(current.Char));
						}
					}

					else if (before.IsDigit)
					{
						AppendWithDelimiter(sb, delimiter, char.ToLower(current.Char));
					}
					else
					{
						AppendWithDelimiter(sb, delimiter, char.ToLower(current.Char));
					}
				}
				else if (current.IsDigit)
				{
					if (before.IsLower)
					{
						AppendWithDelimiter(sb, delimiter, current.Char);
					}

					else if (before.IsUpper)
					{
						AppendWithDelimiter(sb, delimiter, current.Char);
					}

					else if (before.IsDigit)
					{
						sb.Append(current.Char);
					}
					else
					{
						AppendWithDelimiter(sb, delimiter, current.Char);
					}
				}
			}
		}

		string normalizedText = sb.ToString();

		if (string.IsNullOrWhiteSpace(normalizedText))
		{
			if (throwIfEmpty)
				throw new Exception("Text '" + text + "' cannot be normalized.");
			else
				normalizedText = "_";
		}

		if (string.IsNullOrWhiteSpace(normalizedText))
			return "_";

		return normalizedText;
	}

	private static void AppendWithDelimiter(StringBuilder sb, char delimiter, char @char)
	{
		if (sb.Length == 0)
			sb.Append(@char);
		else
			sb.Append($"{delimiter}{@char}");
	}

	public static List<CharInfo>? GetCharInfos(List<char> chars)
	{
		if (chars == null)
			return null;

		var result = new List<CharInfo>();
		foreach (char ch in chars)
			result.Add(new CharInfo(ch));

		return result;
	}

	public static List<CharInfo>? GetCharInfos(List<int> ints)
	{
		if (ints == null)
			return null;

		return GetCharInfos(ints.Select(x => (char)x).ToList());
	}

	public static List<CharInfo>? GetCharInfos(string text)
	{
		if (text == null)
			return null;

		return GetCharInfos(text.ToCharArray().ToList());
	}

	public static List<CharInfo>? GetCharInfos(List<string> hexCodes)
	{
		if (hexCodes == null)
			return null;

		var result = new List<CharInfo>();
		foreach (string hexCode in hexCodes)
		{
			var chInfo = new CharInfo(hexCode);
			result.Add(chInfo);
		}
		return result;
	}

	public static string? CharInfosToString(List<CharInfo> charInfos)
	{
		if (charInfos == null)
			return null;

		if (charInfos.Count == 0)
			return string.Empty;

		var sb = new StringBuilder();
		foreach (var chInfo in charInfos)
			sb.Append(chInfo.Char);

		return sb.ToString();
	}

	[return: NotNullIfNotNull("format")]
	public static string? FormatSafe(string format, params object[] args)
	{
		if (format == null || args == null)
			return format;

		try
		{
			return string.Format(format, args);
		}
		catch
		{
			return format;
		}
	}

	[return: NotNullIfNotNull("format")]
	public static string? FormatSafe(string format, object arg0)
	{
		if (format == null || arg0 == null)
			return format;

		try
		{
			return string.Format(format, arg0);
		}
		catch
		{
			return format;
		}
	}

	[return: NotNullIfNotNull("format")]
	public static string? FormatSafe(IFormatProvider provider, string format, params object[] args)
	{
		if (format == null || args == null)
			return format;
		
		try
		{
			return string.Format(provider, format, args);
		}
		catch
		{
			return format;
		}
	}

	[return: NotNullIfNotNull("format")]
	public static string? FormatSafe(string format, object arg0, object arg1)
	{
		if (format == null || (arg0 == null && arg1 == null))
			return format;

		try
		{
			return string.Format(format, arg0, arg1);
		}
		catch
		{
			return format;
		}
	}

	[return: NotNullIfNotNull("format")]
	public static string? FormatSafe(string format, object arg0, object arg1, object arg2)
	{
		if (format == null || (arg0 == null && arg1 == null && arg2 == null))
			return format;

		try
		{
			return string.Format(format, arg0, arg1, arg2);
		}
		catch
		{
			return format;
		}
	}

	public static string Combine(string text1, string text2, string? joiner = null)
	{
		if (text1 == null)
			return text2;

		if (string.IsNullOrEmpty(text1))
		{
			return text2 ?? text1;
		}

		if (string.IsNullOrEmpty(text2))
		{
			return text1;
		}

		return joiner == null
			? $"{text1}{text2}"
			: $"{text1}{joiner}{text2}";
	}

	[return: NotNullIfNotNull("text")]
	public static string? TrimPrefix(string text, string prefix, bool ignoreCase = false)
	{
		if (text == null)
			return null;

		if (string.IsNullOrEmpty(prefix))
			return text;

		if (text.StartsWith(prefix, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
#if NETSTANDARD2_0 || NETSTANDARD2_1
			return text.Substring(prefix.Length);
#elif NET6_0_OR_GREATER
			return text[prefix.Length..];
#endif

		return text;
	}

	[return: NotNullIfNotNull("text")]
	public static string? TrimPostfix(string text, string postfix, bool ignoreCase = false)
	{
		if (text == null)
			return null;

		if (string.IsNullOrEmpty(postfix))
			return text;

		if (text.EndsWith(postfix, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
#if NETSTANDARD2_0 || NETSTANDARD2_1
			return text.Substring(0, text.Length - postfix.Length);
#elif NET6_0_OR_GREATER
		return text[..^postfix.Length];
#endif

		return text;
	}

	public static string? TrimLength(string text, int maxLength, string? postfix = null)
	{
		if (text == null)
			return null;

		if (maxLength <= 0)
			return "";

		if (text.Length <= maxLength)
			return text;

		if (string.IsNullOrEmpty(postfix))
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1
			return text.Substring(0, maxLength);
#elif NET6_0_OR_GREATER
			return text[..maxLength];
#endif
		}
		else
		{
			if (maxLength < postfix!.Length)
				throw new ArgumentException($"Invalid {nameof(postfix)} length.", nameof(postfix));

			var newLength = maxLength - postfix.Length;
			if (newLength == 0)
				return postfix;
			else 
				return $"{text.SubstringSafe(0, newLength)}{postfix}";
		}
	}

	[return: NotNullIfNotNull("text")]
	public static string? Replace(string text, Dictionary<string, string> data)
	{
		if (text == null || data == null || data.Count == 0)
			return text;

		foreach (var kvp in data)
			text = text.Replace(kvp.Key, kvp.Value);

		return text;
	}

	public static string RemoveAccents(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return text;

		text = text.Normalize(NormalizationForm.FormD);
		var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
		return new string(chars).Normalize(NormalizationForm.FormC);
	}

	public static string EscapeVerbatimString(string str)
	{
		if (str == null)
			throw new ArgumentNullException(nameof(str));

		return str.Replace("\"", "\"\"");
	}

	public static string EscapeString(string str)
	{
		if (str == null)
			throw new ArgumentNullException(nameof(str));

		return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\t", "\\t");
	}

	public static string DelimitString(string value)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));

		return value.Contains(Environment.NewLine)
			? "@\"" + EscapeVerbatimString(value) + "\""
			: "\"" + EscapeString(value) + "\"";
	}


	public static string GenerateLiteral(bool value)
		=> value ? "true" : "false";

	public static string GenerateLiteral(byte value)
		=> value.ToString(CultureInfo.InvariantCulture);

	public static string GenerateLiteral(byte[] value)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));

		return "new byte[] {" + string.Join(", ", value) + "}";
	}

	public static string GenerateLiteral(sbyte value)
		=> value.ToString(CultureInfo.InvariantCulture);

	public static string GenerateLiteral(short value)
		=> value.ToString(CultureInfo.InvariantCulture);

	public static string GenerateLiteral(ushort value)
		=> value.ToString(CultureInfo.InvariantCulture);

	public static string GenerateLiteral(int value)
		=> value.ToString(CultureInfo.InvariantCulture);

	public static string GenerateLiteral(uint value)
		=> value.ToString(CultureInfo.InvariantCulture);

	public static string GenerateLiteral(char value)
	{
		string stringValue = value.ToString();

		if (value == '\a') stringValue = "\\a";
		else if (value == '\b') stringValue = "\\b";
		else if (value == '\f') stringValue = "\\f";
		else if (value == '\n') stringValue = "\\n";
		else if (value == '\r') stringValue = "\\r";
		else if (value == '\t') stringValue = "\\t";
		else if (value == '\v') stringValue = "\\v";
		else if (value == '\'') stringValue = "\\'";
		else if (value == '\"') stringValue = "\\\"";
		else if (value == '\\') stringValue = "\\\\";

		return "'" + stringValue + "'";
	}

	public static string GenerateLiteral(long value)
		=> value.ToString(CultureInfo.InvariantCulture) + "L";

	public static string GenerateLiteral(ulong value)
		=> value.ToString(CultureInfo.InvariantCulture);

	public static string GenerateLiteral(decimal value)
		=> value.ToString(CultureInfo.InvariantCulture) + "M";

	public static string GenerateLiteral(float value)
		=> value.ToString(CultureInfo.InvariantCulture) + "F";

	public static string GenerateLiteral(double value)
		=> value.ToString(CultureInfo.InvariantCulture) + "D";

	public static string GenerateLiteral(TimeSpan value)
		=> "new TimeSpan(" + value.Ticks + ")";

	public static string GenerateLiteral(DateTime value)
		=> "new DateTime(" + value.Ticks + ", DateTimeKind."
		   + Enum.GetName(typeof(DateTimeKind), value.Kind) + ")";

	public static string GenerateLiteral(DateTimeOffset value)
		=> "new DateTimeOffset(" + value.Ticks + ", "
		   + GenerateLiteral(value.Offset) + ")";

	public static string GenerateLiteral(Guid value)
		=> "new Guid(" + GenerateLiteral(value.ToString()) + ")";

	public static string GenerateLiteral(string value)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));

		return "\"" + EscapeString(value) + "\"";
	}

	public static string GenerateVerbatimStringLiteral(string value)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));

		return "@\"" + EscapeVerbatimString(value) + "\"";
	}

	public static string GenerateLiteral(object value)
	{
		if (value == null || value == DBNull.Value)
			return "null";

		Type type = value.GetType();
		if (type.GetTypeInfo().IsEnum)
		{
			return type.Name + "." + Enum.Format(type, value, "G");
		}

		if (type == typeof(bool)) return GenerateLiteral((bool)value);
		else if (type == typeof(byte)) return GenerateLiteral((byte)value);
		else if (type == typeof(byte[])) return GenerateLiteral((byte[])value);
		else if (type == typeof(sbyte)) return GenerateLiteral((sbyte)value);
		else if (type == typeof(short)) return GenerateLiteral((short)value);
		else if (type == typeof(ushort)) return GenerateLiteral((ushort)value);
		else if (type == typeof(int)) return GenerateLiteral((int)value);
		else if (type == typeof(uint)) return GenerateLiteral((uint)value);
		else if (type == typeof(char)) return GenerateLiteral((char)value);
		else if (type == typeof(long)) return GenerateLiteral((long)value);
		else if (type == typeof(ulong)) return GenerateLiteral((ulong)value);
		else if (type == typeof(decimal)) return GenerateLiteral((decimal)value);
		else if (type == typeof(float)) return GenerateLiteral((float)value);
		else if (type == typeof(double)) return GenerateLiteral((double)value);
		else if (type == typeof(TimeSpan)) return GenerateLiteral((TimeSpan)value);
		else if (type == typeof(DateTime)) return GenerateLiteral((DateTime)value);
		else if (type == typeof(DateTimeOffset)) return GenerateLiteral((DateTimeOffset)value);
		else if (type == typeof(Guid)) return GenerateLiteral((Guid)value);
		else if (type == typeof(string)) return GenerateLiteral((string)value);

		return string.Format(CultureInfo.InvariantCulture, "{0}", value);
	}

	public static string BeautifyJson(string json)
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
		var formatted = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
		return formatted;
#elif NET6_0_OR_GREATER
		using var document = System.Text.Json.JsonDocument.Parse(json);
		using var stream = new System.IO.MemoryStream();

		//var testSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All);

		using var writer =
			new System.Text.Json.Utf8JsonWriter(
				stream,
				new System.Text.Json.JsonWriterOptions
				{
					Indented = true,
					Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
				});

		document.WriteTo(writer);
		writer.Flush();
		return Encoding.UTF8.GetString(stream.ToArray());
#endif
	}

	public static string BeautifyXml(string xml)
	{
		if (string.IsNullOrWhiteSpace(xml))
			return xml;

		try
		{
			var doc = System.Xml.Linq.XDocument.Parse(xml);
			return doc.ToString();
		}
		catch
		{
			return xml;
		}
	}

	public static string? BeautifyContent(string content, string contentType)
	{
		if (string.IsNullOrWhiteSpace(content))
			return content ?? "";

		if (string.IsNullOrWhiteSpace(contentType))
			return content.Replace("\\r\\n", "\r\n").Replace("\\n", "\n");

		var newContent = content;
		if (_jsonBeautifyContentTypes.Value.Any(x => -1 < contentType.IndexOf(x, StringComparison.InvariantCultureIgnoreCase)))
		{
			try
			{
				newContent = BeautifyJson(content);
			}
			catch
			{
			}
		}
		else if (_xmlBeautifyContentTypes.Value.Any(x => -1 < contentType.IndexOf(x, StringComparison.InvariantCultureIgnoreCase)))
		{
			newContent = BeautifyXml(content);
		}

		return newContent?.Replace("\\r\\n", "\r\n").Replace("\\n", "\n") ?? content.Replace("\\r\\n", "\r\n").Replace("\\n", "\n");
	}

	[return: NotNullIfNotNull("text")]
	public static MemoryStream? ToStream(string text, Encoding? encoding = null)
	{
		if (text == null)
			return null;

		if (encoding == null)
			encoding = Encoding.UTF8;

		var bytes = encoding.GetBytes(text);
		var ms = new MemoryStream(bytes);
		ms.Seek(0, SeekOrigin.Begin);
		return ms;
	}

	/// <summary>
	/// ReduceWhitespaces
	/// </summary>
	/// <param name="value">Input string</param>
	/// <param name="reduceToWhitespace">All whitespace replaces with this char. If null, first whitespace will be used</param>
	/// <returns>String without multiple whitespaces</returns>
	public static string ReduceWhitespaces(string value, char? reduceToWhitespace = ' ')
	{
		var newString = new StringBuilder();
		bool previousIsWhitespace = false;
		for (int i = 0; i < value.Length; i++)
		{
			var val = value[i];
			if (char.IsWhiteSpace(val))
			{
				if (previousIsWhitespace)
					continue;

				previousIsWhitespace = true;

				if (reduceToWhitespace.HasValue)
					val = reduceToWhitespace.Value;
			}
			else
			{
				previousIsWhitespace = false;
			}

			newString.Append(val);
		}

		return newString.ToString();
	}

	[return: NotNullIfNotNull("source")]
	[return: NotNullIfNotNull("text")]
	public static string? ConcatIfNotNullOrEmpty(this string? source, string? text)
	{
		if (string.IsNullOrEmpty(source))
			return text ?? source;

		if (string.IsNullOrEmpty(text))
			return source ?? text;

		return string.Concat(source, text);
	}

	[return: NotNullIfNotNull("source")]
	[return: NotNullIfNotNull("text")]
	public static string? ConcatIfNotNullOrEmpty(this string? source, string delimiter, string? text)
	{
		if (string.IsNullOrEmpty(source))
			return text ?? source;

		if (string.IsNullOrEmpty(text))
			return source ?? text;

		return string.Concat(source, delimiter, text);
	}

	[return: NotNullIfNotNull("values")]
	public static string? ConcatIfNotNullOrEmpty(string delimiter, IEnumerable<string?> values)
	{
		if (values == null)
			return null;

		if (!values.Any())
			return string.Empty;

		var sb = new StringBuilder();
		bool empty = true;

		foreach (var value in values)
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (!empty)
					sb.Append(delimiter);

				sb.Append(value);
				empty = false;
			}
		}

		return sb.ToString();
	}

	[return: NotNullIfNotNull("values")]
	public static string? ConcatIfNotNullOrEmpty(string delimiter, params string?[] values)
		=> ConcatIfNotNullOrEmpty(delimiter, (IEnumerable<string?>)values);

	[return: NotNullIfNotNull("text")]
	public static string? ToXmlValueString(string text)
	{
		if (text == null)
			return text;

		return new System.Xml.Linq.XElement("t", text).LastNode!.ToString();
	}

	public static string ConvertToString(object? value, CultureInfo cultureInfo)
	{
		if (value == null)
			return string.Empty;

		if (value is Enum)
		{
			var name = Enum.GetName(value.GetType(), value);
			if (name != null)
			{
				var field = IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
				if (field != null)
				{
					if (CustomAttributeExtensions.GetCustomAttribute(field, typeof(EnumMemberAttribute)) is EnumMemberAttribute attribute)
						return attribute.Value ?? name;
				}

				var converted = Convert.ToString(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), cultureInfo));
				return converted ?? string.Empty;
			}
		}
		else if (value is bool boolean)
		{
			return Convert.ToString(boolean, cultureInfo).ToLowerInvariant();
		}
		else if (value is byte[] bytes)
		{
			return Convert.ToBase64String(bytes);
		}
		else if (value.GetType().IsArray)
		{
			var array = Enumerable.OfType<object>((Array)value);
			return string.Join(",", Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
		}

		var result = Convert.ToString(value, cultureInfo);
		return result ?? string.Empty;
	}
}
