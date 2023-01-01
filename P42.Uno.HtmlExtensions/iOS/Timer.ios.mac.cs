using Foundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace P42.Uno.HtmlExtensions
{
    static class Timer
    {
		public static void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			NSTimer timer = NSTimer.CreateRepeatingTimer(interval, t =>
			{
				if (!callback())
					t.Invalidate();
			});
			NSRunLoop.Main.AddTimer(timer, NSRunLoopMode.Common);
		}

	}
}
