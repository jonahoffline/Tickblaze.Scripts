using Docfx.Plugins;
using System.Collections.Immutable;
using System.Composition;
using System.Reflection;
using System.Text;

namespace Tickblaze.Scripts.Documentation.Plugins;

[Export(nameof(MyProcessor), typeof(IPostProcessor))]
public class MyProcessor : IPostProcessor
{
	public MyProcessor()
	{
		//if (System.Diagnostics.Debugger.IsAttached is false)
		//{
		//	System.Diagnostics.Debugger.Launch();
		//}
	}

	public ImmutableDictionary<string, object> PrepareMetadata(ImmutableDictionary<string, object> metadata)
	{
		var tocPath = Path.Combine(Directory.GetCurrentDirectory(), "api", "toc.yml");
		var tocItems = GetTocItems(tocPath);
		var items = new List<TocItem>();

		var assembly = typeof(Script).Assembly;
		var types = assembly.GetTypes();
		var attributeType = typeof(DocumentationAttribute);
		if (attributeType is not null)
		{
			foreach (var type in types)
			{
				var key = type.FullName!;

				if (tocItems.TryGetValue(key, out var item) is false)
				{
					continue;
				}

				var docAttribute = type.GetCustomAttribute<DocumentationAttribute>(false);
				if (docAttribute is not null)
				{
					var parentUid = docAttribute.Path.Replace('/', '.');

					if (tocItems.TryGetValue(parentUid, out var parentItem))
					{
						parentItem.Items.Add(item);
					}
					else
					{
						var names = docAttribute.Path.Split('/');
						var itemsLast = items;

						foreach (var name in names)
						{
							if (tocItems.TryGetValue(name, out var subItem) is false)
							{
								subItem = new TocItem(null)
								{
									Name = name,
								};
								tocItems.Add(name, subItem);
							}

							itemsLast.Add(subItem);
							itemsLast = subItem.Items;
						}

						itemsLast.Add(item);
					}
				}
				else
				{
					items.Add(item);
				}
			}
		}

		items = items.OrderBy(x => x.Name).ToList();

		foreach (var item in items)
		{
			item.OrderByName();
		}

		var builder = new StringBuilder();

		builder.AppendLine("### YamlMime:TableOfContent");
		builder.AppendLine("items:");
		SerializeTocItems(builder, items, 0);
		builder.AppendLine("memberLayout: SamePage");

		var yml = builder.ToString();

		File.WriteAllText(tocPath, yml);

		return metadata;
	}

	public Manifest Process(Manifest manifest, string outputFolder, CancellationToken cancellationToken)
	{
		return manifest;
	}

	private static Dictionary<string, TocItem> GetTocItems(string path)
	{
		using var reader = new StreamReader(path);

		string line;

		var items = new Dictionary<string, TocItem>();
		var item = (TocItem?)null;

		while ((line = reader.ReadLine()) != null)
		{
			line = line.Trim();

			if (line.StartsWith("- uid: "))
			{
				var uid = line.Substring(7);

				item = new TocItem(uid);
				items.Add(uid, item);
			}
			else if (item is not null)
			{
				if (line.StartsWith("name: "))
				{
					item.Name = line.Substring(6);
				}
				else if (line.StartsWith("type: "))
				{
					item.Type = line.Substring(6);
				}
			}
		}

		return items;
	}

	private static void SerializeTocItems(StringBuilder sb, List<TocItem> tocItems, int indentLevel)
	{
		var indent = indentLevel * 2;

		foreach (var item in tocItems)
		{
			if (!string.IsNullOrEmpty(item.Uid))
			{
				sb.Append(' ', indent);
				sb.AppendLine("- uid: " + item.Uid);

				sb.Append(' ', indent + 2);
				sb.AppendLine("name: " + item.Name);
			}
			else
			{
				sb.Append(' ', indent);
				sb.AppendLine("- name: " + item.Name);
			}

			// Add type if it exists
			if (!string.IsNullOrEmpty(item.Type))
			{
				sb.Append(' ', indent + 2);
				sb.AppendLine("type: " + item.Type);
			}

			// Add nested items if they exist
			if (item.Items.Count > 0)
			{
				sb.Append(' ', indent + 2);
				sb.AppendLine("items:");
				SerializeTocItems(sb, item.Items, indentLevel + 1);
			}
		}
	}

	private class TocItem(string? uid)
	{
		public string? Uid { get; } = uid;
		public string Name { get; set; } = string.Empty;
		public string? Type { get; set; } = null;
		public List<TocItem> Items { get; private set; } = [];

		public void OrderByName()
		{
			if (Items.Count > 0)
			{
				Items = Items.OrderBy(x => x.Name).ToList();
			}

			foreach (var item in Items)
			{
				item.OrderByName();
			}
		}
	}
}
