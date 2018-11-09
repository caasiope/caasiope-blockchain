using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Caasiope.Protocol.Types;
using Helios.Common.Configurations;

namespace Caasiope.Node
{
    public class NodeConfiguration
    {
        private static NodeConfiguration Instance;

        private readonly string ConfigurationPath;
        private readonly Network Network;

        public static Network GetNetwork()
        {
            return Instance.Network;
        }

        public static string GetPath(string path)
        {
            return Instance.ConfigurationPath + path;
        }

        private NodeConfiguration(string name)
        {
            var networks = GetNetworkInstances();
            Network = networks.Single(n => n.Name == name);
            ConfigurationPath = $"config/{name}/";
        }

        public static bool IsInitialized() => Instance != null;

        public static void Initialize()
        {
            var name = new UrlConfiguration("config/network.txt").Lines[0];
            Initialize(name);
        }

        public static void Initialize(string name)
        {
            Instance = new NodeConfiguration(name);
        }

        private static List<Network> GetNetworkInstances()
        {
            var network = typeof(Network);
            var types = GetTypesFromAssembly(network.Assembly, network);
            return types.Select(type => (Network) type.GetField("Instance").GetValue(null)).ToList();
        }

        private static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition && (t.IsSubclassOf(baseType)));
        }
    }
}