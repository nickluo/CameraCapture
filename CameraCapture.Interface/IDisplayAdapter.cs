using System;

namespace CameraCapture.Interface
{
    public interface IDisplayAdapter
    {
        void InitDisplay(IntPtr hVideo, IntPtr hEvent);
        void OnResize();
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled);
    }
}
