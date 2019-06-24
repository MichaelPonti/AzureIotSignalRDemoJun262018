using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using IotObjects;

namespace PlantDashboardIOTFuncs
{
	public static class Function1
	{
		/// <summary>
		/// This function is called to get authorization for using the signalr service.
		/// called by the client that is consuming the SignalR service. We are just using
		/// anonymous access for this sample.
		/// </summary>
		/// <param name="request">http api call</param>
		/// <param name="info">this is the signalr auth, just passed back out</param>
		/// <param name="log">logging</param>
		/// <returns>signalr connection information</returns>
		[FunctionName("negotiate")]
		public static SignalRConnectionInfo ClientNegotiate(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequest request,
			[SignalRConnectionInfo(HubName = "plantdashboard", ConnectionStringSetting = "AzureSignalRConnectionString")] SignalRConnectionInfo info,
			ILogger log)
		{
			log.LogInformation("negotiate function accessed");
			return info;
		}


		/// <summary>
		/// This function is hooked up to the IoT hub through the use of the [IoTHubTrigger] attribute
		/// in the function call. Messages are then pumped out through the collector object to SignalR.
		/// </summary>
		/// <param name="message">This is the event that is passed from IoTHub</param>
		/// <param name="messages">SignalR object for pumping messages out to subscribed clients. HubName is the name of the SignalR service</param>
		/// <param name="log">logging for your function</param>
		/// <returns></returns>
		[FunctionName("messages")]
		public static async Task Run(
			[IoTHubTrigger("messages/events", Connection = "plantdashboardstorage")]EventData message,
			[SignalR(HubName = "plantdashboard")] IAsyncCollector<SignalRMessage> messages,
			ILogger log)
		{
			// need to convert to string first
			var messageString = Encoding.UTF8.GetString(message.Body.Array);
			log.LogInformation($"IoT Hub Trigger processed message: {messageString}");

			// now we can deserialize it, can be handy for debugging, but in reality, we
			// are just going to pump the string out to signalr.
			var messageData = JsonConvert.DeserializeObject<IotDataPoint>(messageString);

			await messages.AddAsync(new SignalRMessage()
			{
				// "notify" is the event that SignalR clients listen for
				Target = "notify",
				Arguments = new[] { messageString }
			});
		}
	}
}