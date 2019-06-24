using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace ApiServer.Hubs
{
	public class RandomValueGenerator
	{
		private Timer _timer = null;
		private Random _random = null;
		private Action<int> _func = null;
		private int _min = 0;
		private int _max = 100;

		public RandomValueGenerator(int min, int max, double interval, Action<int> f)
		{
			_min = min;
			_max = max;
			_random = new Random();
			_timer = new Timer();
			_func = f;

			_timer.Elapsed += (s, e) =>
			{
				int newValue = GetNextValue();
				_func(newValue);
			};

			_timer.Interval = interval;
			_timer.Enabled = true;
		}

		private int GetNextValue()
		{
			return _random.Next(_min, _max);
		}

		public bool Enabled
		{
			get => _timer.Enabled;
			set => _timer.Enabled = value;
		}

		public double Interval
		{
			get => _timer.Interval;
			set => _timer.Interval = value;
		}
	}
}
