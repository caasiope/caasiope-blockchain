using System.Reflection;

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
	}
}
