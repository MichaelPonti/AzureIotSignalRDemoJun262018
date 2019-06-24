		[FunctionName("negotiate")]
		public static SignalRConnectionInfo ClientNegotiate(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequest request,
			[SignalRConnectionInfo(HubName = "plantdashboard", ConnectionStringSetting = "AzureSignalRConnectionString")] SignalRConnectionInfo info,
			ILogger log)
		{
			log.LogInformation("negotiate function accessed");
			return info;
		}

