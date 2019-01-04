using System.Reflection;
using Newtonsoft.Json;

namespace Helios.JSON
{
	public class JsonApi
	{
		public readonly JsonMessageFactory JsonMessageFactory;

		public JsonApi(Assembly assembly)
		{
			JsonMessageFactory = new JsonMessageFactory(assembly);
		}

		public JsonApi(Assembly[] assemblies)
		{
			JsonMessageFactory = new JsonMessageFactory(assemblies);
		}

		public JsonApi(Assembly assembly, JsonConverter[] converters)
		{
			JsonMessageFactory = new JsonMessageFactory(assembly, converters);
		}
	}
}
