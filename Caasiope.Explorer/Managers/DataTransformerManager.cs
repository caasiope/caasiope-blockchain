﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Transformers;
using Caasiope.Node;

namespace Caasiope.Explorer.Managers
{
    public class DataTransformerManager
    {
        private readonly Dictionary<string, IDataTransformerService> services = new Dictionary<string, IDataTransformerService>();

        public void Transform(DataTransformationContext context, TableLedgerHeight table)
        {
            services[table.TableName].ProcessNext(context);
        }

        // TODO this is a hack
        public List<TableLedgerHeight> GetInitialTableHeights()
        {
            var results = new List<TableLedgerHeight>();
            foreach (var table in services.Keys)
            {
                results.Add(new TableLedgerHeight(table, -1));
            }
            return results;
        }

        public void RegisterOnProcessed(TableLedgerHeight table, Action<string, long> callback)
        {
            services[table.TableName].RegisterOnProcessed(callback);
        }

        public void Initialize()
        {
            services.Clear();

            var instances = CreateInstances();

            instances.ForEach(service => Injector.Inject(service));

            Task.WaitAll(instances.Select(service => service.Initialize()).ToArray());

            instances.ForEach(service => services.Add(service.TableName, service));
        }

        public void Start()
        {
            Task.WaitAll(services.Values.Select(service => service.Start()).ToArray());
        }

        public void Stop()
        {
            Task.WaitAll(services.Values.Select(service => service.Stop()).ToArray());
        }

        private List<IDataTransformerService> CreateInstances()
        {
            return GetAllServiceTypes().Select(CreateInstance).ToList();
        }

        private static IDataTransformerService CreateInstance(Type type)
        {
            return (IDataTransformerService)Activator.CreateInstance(type);
        }

        private static IEnumerable<Type> GetAllServiceTypes()
        {
            return typeof(Explorer.Managers.DataTransformerManager).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract
                                      && t.Namespace == typeof(DeclarationTransformerService).Namespace
                                      && t.IsSubclassOf(typeof(DataTransformerService)));
        }
    }
}