using Android.OS;
using System;
using System.Collections.Generic;
using System.Text;

namespace P42.Uno.HtmlExtensions
{
    static class MainThread
    {
        static volatile Handler handler;

        public static bool IsMainThread
        {
            get
            {
                if ((int)Build.VERSION.SdkInt >= (int)BuildVersionCodes.M)
                    return Looper.MainLooper.IsCurrentThread;

                return Looper.MyLooper() == Looper.MainLooper;
            }
        }

        public static void BeginInvokeOnMainThread(Action action)
        {
            if (handler?.Looper != Looper.MainLooper)
                handler = new Handler(Looper.MainLooper);

            handler.Post(action);
        }
    }
}
