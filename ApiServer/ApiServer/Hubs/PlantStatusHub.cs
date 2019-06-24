using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Hubs
{
	public class PlantStatusHub : Hub
	{
		private static IHubContext<PlantStatusHub> _hubContext = null;

		private static RandomValueGenerator _powerGenerator = null;
		private static RandomValueGenerator _defectsGenerator = null;
		private static RandomValueGenerator _outputGenerator = null;
		private static RandomValueGenerator _airGenerator = null;

		static PlantStatusHub()
		{
			_powerGenerator = new RandomValueGenerator(1100, 1300, 3000, async i => await _hubContext?.Clients.All.SendAsync("PowerReading", i));
			_defectsGenerator = new RandomValueGenerator(0, 20, 2000, async i => await _hubContext?.Clients.All.SendAsync("DefectCount", i));
			_outputGenerator = new RandomValueGenerator(50, 60, 5000, async i => await _hubContext?.Clients.All.SendAsync("ProductOutput", i));
			_airGenerator = new RandomValueGenerator(300, 400, 4000, async i => await _hubContext?.Clients.All.SendAsync("AirQualityPpm", i));
		}


		public PlantStatusHub(IHubContext<PlantStatusHub> hubContext)
		{
			_hubContext = hubContext;
		}
	}
}
