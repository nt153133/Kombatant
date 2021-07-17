using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kombatant.Settings;

namespace Kombatant.Helpers
{
	internal class PerformanceLogger : IDisposable
	{
		private readonly string _blockName;
		private readonly bool _forceLog;
		private readonly Stopwatch _stopwatch;
		private bool _isDisposed;

		public PerformanceLogger(string blockName, bool forceLog = false)
		{
			_forceLog = forceLog;
			_blockName = blockName;
			_stopwatch = new Stopwatch();
			_stopwatch.Start();
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (_isDisposed) return;
			_isDisposed = true;
			_stopwatch.Stop();
			if (BotBase.Instance.PerformanceLogger && (_stopwatch.Elapsed.TotalMilliseconds > BotBase.Instance.MinLogMs || _forceLog))
			{
				LogHelper.Instance.Log("[Performance] Execution of \"{0}\" took {1:00.00000}ms.", _blockName,
					_stopwatch.Elapsed.TotalMilliseconds);
			}
			_stopwatch.Reset();
		}

		#endregion

		~PerformanceLogger()
		{
			Dispose();
		}
	}
}
