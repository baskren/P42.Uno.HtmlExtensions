using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;

namespace P42.Uno.HtmlExtensions
{
    static class MainThread
    {
        internal static void WatchForError(this IAsyncAction self) =>
            self.AsTask().WatchForError();

        internal static void WatchForError<T>(this IAsyncOperation<T> self) =>
            self.AsTask().WatchForError();

        internal static void WatchForError(this Task self)
        {
            var context = SynchronizationContext.Current;
            if (context == null)
                return;

            self.ContinueWith(
                t =>
                {
                    var exception = t.Exception.InnerExceptions.Count > 1 ? t.Exception : t.Exception.InnerException;

                    context.Post(e => { throw (Exception)e; }, exception);
                }, CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
        }

        public static bool IsMainThread
        {
            get
            {
                try
                {
                    if (CoreApplication.MainView?.CoreWindow == null)
                        return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Unable to validate MainView creation. {ex.Message}");
                    return true;
                }

                return CoreApplication.MainView.CoreWindow.Dispatcher?.HasThreadAccess ?? false;
            }
        }

        public static void BeginInvokeOnMainThread(Action action)
        {
            var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;

            if (dispatcher == null)
                throw new InvalidOperationException("Unable to find main thread.");
            dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).WatchForError();
        }

    }
}
