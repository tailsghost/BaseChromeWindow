using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace BaseChromeWindow;

public static class MonitorHelper
{

    private const uint MONITOR_DEFAULTTONEAREST = 2;
    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public int dwFlags;
    }

    public static MONITORINFO GetMonitorInfoForWindow(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        var hMon = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        var mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        GetMonitorInfo(hMon, ref mi);
        return mi;
    }

    public static double GetDevicePixelRatio(Window window)
    {
        var dpi = VisualTreeHelper.GetDpi(window);
        return dpi.DpiScaleX;
    }
}
