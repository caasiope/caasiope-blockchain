using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Helios.Common.Configurations
{
	public class UrlConfiguration
	{
		public readonly List<string> Lines = new List<string>();
		public UrlConfiguration(string path)
		{
			// read file
			var lines = File.ReadAllLines(path);
			Lines.AddRange(lines.Where(line => !line.StartsWith("#")));
		}
	}

	public class DictionaryConfiguration
	{
		private readonly Dictionary<string, string> Lines = new Dictionary<string, string>();

		public DictionaryConfiguration(string path)
		{
			foreach (var line in new UrlConfiguration(path).Lines)
			{
				var index = line.IndexOf("=");
				if (index > 0)
				{
					var offset = index + 1;
					var key = line.Substring(0, index);
					var value = line.Substring(offset, line.Length - offset);
					Lines.Add(key, value);
				}
			}
		}

		public string GetValue(string key)
		{
		    return Lines.TryGetValue(key, out var value) ? value : string.Empty;
		}
	}
}
