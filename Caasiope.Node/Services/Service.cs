namespace Caasiope.Services
{
    public abstract class Service : Helios.Common.Concepts.Services.Service
    {
        private readonly ServiceList services;

        protected Service(ServiceList services)
        {
            this.services = services;
        }

        protected ILiveService LiveService           { get { return services.LiveService; } }
        protected IDatabaseService DatabaseService   { get { return services.DatabaseService; } }
    }
}