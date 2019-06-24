using IotObjects;
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LightSensorApp
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private ObservableCollection<string> _messages = new ObservableCollection<string>();
		private Random _random = new Random();


		public MainPage()
		{
			this.InitializeComponent();
		}

		private int GetNextReading()
		{
			return _random.Next(0, 100);
		}


		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			_lvMessages.ItemsSource = _messages;
			_deviceClient = DeviceClient.CreateFromConnectionString(ConnectionString, TransportType.Mqtt);

		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);
		}

		private void _btnStart_Click(object sender, RoutedEventArgs e)
		{
			StartListening();
		}

		private void _btnStop_Click(object sender, RoutedEventArgs e)
		{
			StopListening();
		}


		private GpioController _gpio = null;
		private GpioPin _lightPin = null;
		private GpioPin _buttonPin = null;

		private DeviceClient _deviceClient = null;
		private const string ConnectionString =
			"<your iot hub connection string>";



		private void StartListening()
		{
			_gpio = GpioController.GetDefault();


			_lightPin = _gpio.OpenPin(4);
			_lightPin.SetDriveMode(GpioPinDriveMode.Output);

			_buttonPin = _gpio.OpenPin(21);
			if (_buttonPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
				_buttonPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
			else
				_buttonPin.SetDriveMode(GpioPinDriveMode.Input);
			_buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

			var buttonState = _buttonPin.Read();
			_lightPin.Write((buttonState == GpioPinValue.High) ? GpioPinValue.Low : GpioPinValue.High);

			_buttonPin.ValueChanged += (s, e) =>
			{
				// we are just going to check to see if the button is being pressed first
				if (e.Edge == GpioPinEdge.FallingEdge)
				{
					var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
					{
						_messages.Add($"button pressed {_buttonPin.Read().ToString()}");
						var pinValue = _buttonPin.Read();
						Debug.WriteLine($"Button pressed: {pinValue.ToString()}");
						var currentLightValue = _lightPin.Read();
						var newLightValue = (currentLightValue == GpioPinValue.High) ? GpioPinValue.Low : GpioPinValue.High;
						Debug.WriteLine($"light set to: {newLightValue.ToString()}");
						_lightPin.Write(newLightValue);
						await SendMessageToIOTHub((newLightValue == GpioPinValue.High), "There was an alert");
					});
				}
			};
		}


		private async Task SendMessageToIOTHub(bool pinState, string message)
		{
			try
			{
				//var msgContent = new DeviceReadingMessage(pinState, message, GetNextReading());
				var msgContent = new IotDataPoint()
				{
					Camera = new double[] { 75, -40, 10 },
					CameraUpVector = new double[] { 0, 0, 1 },
					Latitude = 0,
					Longitude = 0,
					Message = message,
					Priority = pinState ? MessagePriority.Critical : MessagePriority.Info,
					Target = new double[] { 70, 0, 0 }
				};
				var deviceMessage = new Message(Encoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(msgContent)));
				deviceMessage.Properties.Add("buttonAlert", pinState.ToString());
				await _deviceClient.SendEventAsync(deviceMessage);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.ToString());
			}
		}



		private void StopListening()
		{
			_deviceClient.Dispose();
			_deviceClient = null;

			if (_buttonPin != null)
			{
				_buttonPin.Dispose();
				_buttonPin = null;
			}

			if (_lightPin != null)
			{
				_lightPin.Dispose();
				_lightPin = null;
			}
		}

		private async void _btnSoftware_Click(object sender, RoutedEventArgs e)
		{
			await SendMessageToIOTHub(true, "some kind of event");
		}
	}
}
