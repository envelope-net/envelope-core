namespace Envelope.IOUtils;

public static class DirectoryHelper
{
	public static List<string>? GetParentsList(string path, int depth)
	{
		if (string.IsNullOrWhiteSpace(path))
			throw new ArgumentNullException(nameof(path));

		if (depth < 0)
			throw new ArgumentOutOfRangeException(nameof(depth));

		path = path.Trim();

		if (depth == 0)
			return new List<string>();

		if (depth == 1)
		{
			var p = Directory.GetParent(path)?.Name;
			return string.IsNullOrWhiteSpace(p)
				? new List<string>()
				: new List<string> { p! };
		}

		var currentDepth = 1;
		var parts = new List<string>();
		var parent = Directory.GetParent(path);
		while (parent != null && currentDepth <= depth)
		{
			parts.Add(parent.Name);
			parent = parent.Parent;
			currentDepth++;
		}

		parts.Reverse();
		return parts;
	}

	public static string? GetParents(string path, int depth, string delimiter)
	{
		var parts = GetParentsList(path, depth);

		if (parts == null)
			return null;

		return string.Join(delimiter, parts);
	}
}
