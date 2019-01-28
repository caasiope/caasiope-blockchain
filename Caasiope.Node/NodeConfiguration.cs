using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Caasiope.Protocol.Types;
using Helios.Common.Configurations;

namespace Caasiope.Node
{
    public class NodeConfiguration
    {
        private static NodeConfiguration Instance;

        private readonly string configurationPath;
        private readonly string dataPath;
        private readonly Network network;

        public static Network GetNetwork()
        {
            return Instance.network;
        }

        public static string GetPath(string path)
        {
            return Instance.configurationPath + path;
        }

        public static string GetCertificatesPath()
        {
            return Path.Combine(Instance.dataPath, "certificates\\");
        }

        public static string GetDataPath()
        {
            return Instance.dataPath;
        }

        private NodeConfiguration(string name, string dataPath)
        {
            this.dataPath = dataPath;
            var networks = GetNetworkInstances();
            network = networks.Single(n => n.Name == name);
            configurationPath = $"config/{name}/";
        }

        public static bool IsInitialized() => Instance != null;

        public static void Initialize()
        {
            var name = new UrlConfiguration("config/network.txt").Lines[0];
            var dataPath = GetFullDataPath(new DictionaryConfiguration("config/config.txt").GetValue("DataPath"), name);
            Initialize(name, dataPath);
        }

        private static string GetFullDataPath(string settingsPath, string network)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Regex.Replace(settingsPath, "%AppData%", appData, RegexOptions.IgnoreCase);
            if (path.Contains('%'))
                throw new Exception($"Alias is not supported. Data path is not valid {path}");
            return Path.Combine(path, network + Path.DirectorySeparatorChar);
        }

        public static void Initialize(string name, string dataPath)
        {
            Instance = new NodeConfiguration(name, dataPath);
            Directory.CreateDirectory(GetCertificatesPath());
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