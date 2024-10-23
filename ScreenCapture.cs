using System.Drawing;
using System.Runtime.InteropServices;

#pragma warning disable CA1416

namespace rapidclick;

public class ScreenCapture
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetDesktopWindow();

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }   

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

    public static Image CaptureDesktop()
    {
        return CaptureWindow(GetDesktopWindow());
    }

    public static Bitmap CaptureActiveWindow()
    {
        return CaptureWindow(GetForegroundWindow());
    }
    
    public static Rectangle GetWindowRect(IntPtr handle)
    {
        Rect rect = new();
        GetWindowRect(handle, ref rect);
        return new(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public static Bitmap CaptureWindow(IntPtr handle)
    {
        Rect rect = new();
        GetWindowRect(handle, ref rect);
        Rectangle bounds = new(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        Bitmap result = new(bounds.Width, bounds.Height);
        
        using Graphics graphics = Graphics.FromImage(result);
        graphics.CopyFromScreen(new(bounds.Left, bounds.Top), Point.Empty, bounds.Size);

        return result;
    }
}