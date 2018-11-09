using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helios.Common.Concepts.Services;

namespace Caasiope.Node
{
    public class ServiceManager
	{
		private readonly List<IService> services = new List<IService>();

		public T Add<T>(T service) where T : IService
		{
			services.Add(Injector.Add(service));
			return service;
		}

		public void Initialize()
	    {
		    foreach (var service in services)
			    Injector.Inject(service);
            Task.WaitAll(services.Select(service => service.Initialize()).ToArray());
        }

        public void Start()
        {
            Task.WaitAll(services.Select(service => service.Start()).ToArray());
        }

        public void Stop()
        {
            Task.WaitAll(services.Select(service => service.Stop()).ToArray());
        }

        public void SetServiceStart(IService service, bool isStart)
        {
            if (!isStart)
                services.Remove(service);
        }
    }
}