using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Helios.JSON
{
	public class JsonMessage { }

	public class Request : JsonMessage { }

	public class Response : JsonMessage { }

	public class Notification : JsonMessage { }

	public abstract class Request<TResponse> : Request where TResponse : Response { }

	public abstract class Response<TRequest> : Response where TRequest : Request { }

	public class RequestMessage : MessageWrapper
	{
		public RequestMessage(Request request) : base(request) { }
		public RequestMessage(Request request, string type, string crid) : base(request, type, crid) { }
	}

	public class ResponseMessage : MessageWrapper
	{
		[JsonProperty(PropertyName = "result")]
		public readonly byte ResultCode;

		public ResponseMessage(Response response, string type, string crid, byte result) : base(response, type, crid)
		{
			ResultCode = result;
		}

		public ResponseMessage(Response response, string crid, byte result) : base(response, response.GetType().Name, crid)
		{
			ResultCode = result;
		}
	}

	public class ErrorMessage : MessageWrapper
	{
		[JsonProperty(PropertyName = "result")]
		public readonly byte ResultCode;

		public ErrorMessage(string crid, byte result) : base(null, "error", crid)
		{
			ResultCode = result;
		}
	}

	public class NotificationMessage : MessageWrapper
	{
		public NotificationMessage(Notification request) : base(request) { }
		public NotificationMessage(Notification request, string type) : base(request, type, null) { }
	}

	public abstract class MessageWrapper
	{
		[JsonProperty(PropertyName = "type")]
		public readonly string Type;
		[JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
		public readonly object Data;
		[JsonProperty(PropertyName = "crid", NullValueHandling = NullValueHandling.Ignore)]
		public readonly string ClientRequestID;

		protected MessageWrapper(JsonMessage request)
		{
			ClientRequestID = Guid.NewGuid().ToString("N");
			Type = request.GetType().Name;
			Data = request;
		}

		protected MessageWrapper(JsonMessage request, string type, string crid)
		{
			Debug.Assert(request == null || request.GetType().Name == type);
			ClientRequestID = crid;
			Type = type;
			Data = request;
	    }

	    public T GetData<T>() where T : JsonMessage
	    {
	        return (T)Data;
	    }
    }

    public enum ResultCode : byte
    {
        Success = 0,
        Failed = 1,
        DeserializationFailed = 2,
        MessageHasNoType = 3,
        MessageHasNoCrid = 4,
        MessageHasNoData = 5,
        MessageHasNoResult = 6,
        UnknownMessage = 7,
    }
}
