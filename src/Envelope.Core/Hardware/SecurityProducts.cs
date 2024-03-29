﻿using System.Text;

namespace Envelope.Hardware;

public class SecurityProducts : Serializer.IDictionaryObject, Serializer.ITextSerializer
{
	public List<SecurityProduct> Products { get; set; }

	public SecurityProducts()
	{
		Products = new List<SecurityProduct>();
	}

	public IDictionary<string, object?> ToDictionary(Serializer.ISerializer? serializer = null)
	{
		var dict = new Dictionary<string, object?>();

		if (Products != null)
			for (int i = 0; i < Products.Count; i++)
				dict.Add($"{nameof(Products)}[{i}]", Products[i]?.ToDictionary());

		return dict;
	}

	public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
	{
		if (before != null)
			sb.AppendLine(before);

		foreach (var secProduct in Products)
			secProduct.WriteTo(sb);

		if (after != null)
			sb.AppendLine(after);
	}
}
