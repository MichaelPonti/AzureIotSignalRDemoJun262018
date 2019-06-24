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
			// var messageData = JsonConvert.DeserializeObject<IotDataPoint>(messageString);

			await messages.AddAsync(new SignalRMessage()
			{
				// "notify" is the event that SignalR clients listen for
				Target = "notify",
				Arguments = new[] { messageString }
			});
		}

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;
using System;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

