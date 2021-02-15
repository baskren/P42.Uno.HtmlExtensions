using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace P42.Uno.HtmlExtensions
{
    class Timer
    {
        class MainThreadTimer
        {
            readonly DispatcherTimer dispatcherTimer;
            readonly Func<bool> Func;

            public MainThreadTimer(TimeSpan span, Func<bool> func)
            {
                Func = func;
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += DispatcherTimer_Tick;
                dispatcherTimer.Interval = span;
                dispatcherTimer.Start();
            }

            void DispatcherTimer_Tick(object sender, object e)
            {
                if (!Func.Invoke())
                {
                    dispatcherTimer.Stop();
                }
            }
        }

        class AsyncMainThreadTimer
        {
            readonly DispatcherTimer dispatcherTimer;
            readonly Func<Task<bool>> task;

            public AsyncMainThreadTimer(TimeSpan span, Func<Task<bool>> task)
            {
                this.task = task;
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += DispatcherTimer_Tick;
                dispatcherTimer.Interval = span;
                dispatcherTimer.Start();
            }

            async void DispatcherTimer_Tick(object sender, object e)
            {
                if (!await task.Invoke())
                {
                    dispatcherTimer.Stop();
                }
            }
        }

        public void StartTimer(TimeSpan timeSpan, Func<bool> func)
            => new MainThreadTimer(timeSpan, func);

        public void StartTimer(TimeSpan timeSpan, Func<Task<bool>> task)
            => new AsyncMainThreadTimer(timeSpan, task);

    }
}
