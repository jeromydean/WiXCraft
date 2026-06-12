using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ExampleInterface.Windows
{
  internal static class Windows11WindowHelper
  {
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwaSystemBackdropType = 38;
    private const int DwmwcpRound = 2;
    private const int DwmsbtMainWindow = 2;

    public static void TryApply(Window window)
    {
      if (window == null || !IsWindows11OrLater())
      {
        return;
      }

      window.SourceInitialized += Window_SourceInitialized;
      if (window.IsLoaded)
      {
        ApplyToHandle(new WindowInteropHelper(window).Handle);
      }
    }

    private static void Window_SourceInitialized(object sender, EventArgs e)
    {
      Window window = (Window)sender;
      window.SourceInitialized -= Window_SourceInitialized;
      ApplyToHandle(new WindowInteropHelper(window).Handle);
    }

    private static bool IsWindows11OrLater()
    {
      return Environment.OSVersion.Version.Build >= 22000;
    }

    private static void ApplyToHandle(IntPtr handle)
    {
      if (handle == IntPtr.Zero)
      {
        return;
      }

      try
      {
        int roundCorners = DwmwcpRound;
        DwmSetWindowAttribute(handle, DwmwaWindowCornerPreference, ref roundCorners, sizeof(int));

        int micaBackdrop = DwmsbtMainWindow;
        DwmSetWindowAttribute(handle, DwmwaSystemBackdropType, ref micaBackdrop, sizeof(int));
      }
      catch (DllNotFoundException)
      {
      }
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
      IntPtr hwnd,
      int dwAttribute,
      ref int pvAttribute,
      int cbAttribute);
  }
}
