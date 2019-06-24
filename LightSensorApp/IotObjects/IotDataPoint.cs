using System;

namespace IotObjects
{
	public enum MessagePriority
	{
		Info = 0x00,
		Warning = 0x02,
		Danger = 0x04,
		Critical = 0x08,
	}


	public class IotDataPoint
	{
		public DateTime TimeStamp { get; private set; } = DateTime.UtcNow;

		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public double[] Camera { get; set; }
		public double[] Target { get; set; }
		public double[] CameraUpVector { get; set; }

		public string Message { get; set; }
		public MessagePriority Priority { get; set; } = MessagePriority.Info;
	}
}
