using System.Runtime.InteropServices;
using System.Windows;

namespace BaseChromeWindow;

public static class MouseHelper
{
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    public static Point GetMousePositionOnScreen()
    {
        POINT p;
        GetCursorPos(out p);

        return new Point(p.X, p.Y);
    }
}

