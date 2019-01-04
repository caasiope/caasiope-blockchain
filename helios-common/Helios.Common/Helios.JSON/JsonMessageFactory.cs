using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Helios.JSON
{
	public class JsonMessageFactory
	{
		private readonly Dictionary<string, Type> NotificationTypes = new Dictionary<string, Type>();
		private readonly Dictionary<string, Type> RequestTypes = new Dictionary<string, Type>();
		private readonly Dictionary<string, Type> ResponseTypes = new Dictionary<string, Type>();
	    private readonly JsonConverter[] converters;

	    public JsonMessageFactory(Assembly assembly)
		{
			ImportTypesFromAssembly(assembly);
		}

		public JsonMessageFactory(Assembly[] assemblies)
		{
			foreach (var assembly in assemblies)
				ImportTypesFromAssembly(assembly);
		}

		public JsonMessageFactory(Assembly assembly, JsonConverter[] converters)
		{
		    ImportTypesFromAssembly(assembly);

            this.converters = converters;
		}

		private void ImportTypesFromAssembly(Assembly assembly, bool isClearing = false)
		{
			if (isClearing)
			{
				NotificationTypes.Clear();
				RequestTypes.Clear();
				ResponseTypes.Clear();
			}

			AddToDictionary(NotificationTypes, GetTypesFromAssembly(assembly, typeof(Notification)));
			AddToDictionary(RequestTypes, GetTypesFromAssembly(assembly, typeof(Request)));
			AddToDictionary(ResponseTypes, GetTypesFromAssembly(assembly, typeof(Response)));
		}

		private IEnumerable<Type> GetTypesFromAssembly(Assembly assembly, Type baseType)
		{
			return assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition && (t.IsSubclassOf(baseType)));
		}

		private void AddToDictionary(Dictionary<string, Type> dictionary, IEnumerable<Type> types)
		{
			foreach (var type in types)
				dictionary.Add(type.Name, type);
		}

		public MessageWrapper DeserializeMessage(string message, out string crid, ref bool isError)
		{
		    crid = null;
		    string typeName = null;
		    string data = null;
		    var isResultEmpty = false;
		    try
		    {
		        var jObject = JObject.Parse(message);
		        typeName = jObject["type"].ToString();
		        if (NotificationTypes.TryGetValue(typeName, out var objectType))
		        {
		            data = jObject["data"].ToString();
		            var json = JsonConvert.DeserializeObject(data, objectType, converters);
		            return new NotificationMessage((Notification)json, typeName);
		        }
		        crid = jObject["crid"].ToString();
		        if (RequestTypes.TryGetValue(typeName, out objectType))
		        {
		            data = jObject["data"].ToString();
		            var json = JsonConvert.DeserializeObject(data, objectType, converters);
		            return new RequestMessage((Request)json, typeName, crid);
		        }
		        if (ResponseTypes.TryGetValue(typeName, out objectType))
		        {
		            data = jObject["data"].ToString();
		            var json = JsonConvert.DeserializeObject(data, objectType, converters);
		            isResultEmpty = jObject["result"] == null;
		            return new ResponseMessage((Response)json, typeName, crid, (byte)jObject["result"]);
		        }
		        if (typeName == "error")
		        {
		            isError = true;
		            isResultEmpty = jObject["result"] == null;
		            return new ErrorMessage(crid, (byte)jObject["result"]);
		        }
		    }
            catch (NullReferenceException)
		    {
		        var rc = ResultCode.Failed;

                if (crid == null)
		            rc = ResultCode.MessageHasNoCrid;
                else if(typeName == null)
                    rc =  ResultCode.MessageHasNoType;
                else if (data == null)
                    rc = ResultCode.MessageHasNoData;
                else if (isResultEmpty)
                    rc = ResultCode.MessageHasNoResult;

                isError = true;
		        return new ErrorMessage(crid, (byte)rc);
		    }
            catch (Exception)
            {
                isError = true;
                return new ErrorMessage(crid, (byte)ResultCode.DeserializationFailed);
            }

            isError = true;
            return new ErrorMessage(crid, (byte)ResultCode.UnknownMessage);
        }

		public string SerializeRequest(RequestMessage message)
		{
			Debug.Assert(RequestTypes.ContainsKey(message.Type));
			return JsonConvert.SerializeObject(message);
		}

		public string SerializeNotification(NotificationMessage message)
		{
			Debug.Assert(NotificationTypes.ContainsKey(message.Type));
			return JsonConvert.SerializeObject(message);
		}

		public string SerializeResponse(ResponseMessage message)
		{
			Debug.Assert(message.ResultCode != 0 || ResponseTypes.ContainsKey(message.Type));
			return JsonConvert.SerializeObject(message);
		}

		public string SerializeError(ErrorMessage message)
		{
            Debug.Assert(message.ResultCode != 0);
            return JsonConvert.SerializeObject(message);
		}
	}
}
