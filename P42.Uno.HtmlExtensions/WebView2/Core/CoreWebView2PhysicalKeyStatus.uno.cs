using System;
using Uno;

namespace P42.Web.WebView2.Core
{
    public struct CoreWebView2PhysicalKeyStatus : IEquatable<CoreWebView2PhysicalKeyStatus>
    {
        public uint RepeatCount;

        public uint ScanCode;

        public int IsExtendedKey;

        public int IsMenuKeyDown;

        public int WasKeyDown;

        public int IsKeyReleased;

        public CoreWebView2PhysicalKeyStatus(uint _RepeatCount, uint _ScanCode, int _IsExtendedKey, int _IsMenuKeyDown, int _WasKeyDown, int _IsKeyReleased)
        {
            RepeatCount = _RepeatCount;
            ScanCode = _ScanCode;
            IsExtendedKey = _IsExtendedKey;
            IsMenuKeyDown = _IsMenuKeyDown;
            WasKeyDown = _WasKeyDown;
            IsKeyReleased = _IsKeyReleased;
        }

        public static bool operator ==(CoreWebView2PhysicalKeyStatus x, CoreWebView2PhysicalKeyStatus y)
        {
            if (x.RepeatCount == y.RepeatCount && x.ScanCode == y.ScanCode && x.IsExtendedKey == y.IsExtendedKey && x.IsMenuKeyDown == y.IsMenuKeyDown && x.WasKeyDown == y.WasKeyDown)
            {
                return x.IsKeyReleased == y.IsKeyReleased;
            }

            return false;
        }

        public static bool operator !=(CoreWebView2PhysicalKeyStatus x, CoreWebView2PhysicalKeyStatus y)
        {
            return !(x == y);
        }

        public bool Equals(CoreWebView2PhysicalKeyStatus other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is CoreWebView2PhysicalKeyStatus)
            {
                CoreWebView2PhysicalKeyStatus coreWebView2PhysicalKeyStatus = (CoreWebView2PhysicalKeyStatus)obj;
                return this == coreWebView2PhysicalKeyStatus;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return RepeatCount.GetHashCode() ^ ScanCode.GetHashCode() ^ IsExtendedKey.GetHashCode() ^ IsMenuKeyDown.GetHashCode() ^ WasKeyDown.GetHashCode() ^ IsKeyReleased.GetHashCode();
        }
    }
}
