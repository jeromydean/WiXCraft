using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using WiXCraft;

namespace ExampleInterface.Windows
{
  internal static class InstallerElevationHelper
  {
    private const int ErrorCancelled = 1223;

    public static bool IsProcessElevated()
    {
      using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
      {
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
      }
    }

    public static bool IsInstallerPrivileged(IInstallerSession session)
    {
      return session != null &&
        string.Equals(session["Privileged"], "1", StringComparison.Ordinal);
    }

    public static bool IsElevated(IInstallerSession session)
    {
#if (DEBUG)
      return false;
#else
      return IsInstallerPrivileged(session) || IsProcessElevated();
#endif
    }

    public static bool TryRestartElevated(string msiPath, out string errorMessage)
    {
      errorMessage = null;

      if (string.IsNullOrWhiteSpace(msiPath))
      {
        errorMessage = "Could not determine the installer package path.";
        return false;
      }

      try
      {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
          FileName = "msiexec.exe",
          Arguments = string.Concat("/i \"", msiPath, "\""),
          UseShellExecute = true,
          Verb = "runas",
        };

        Process.Start(startInfo);
        return true;
      }
      catch (Win32Exception ex) when (ex.NativeErrorCode == ErrorCancelled)
      {
        errorMessage = "Elevation was cancelled.";
        return false;
      }
      catch (Exception ex)
      {
        errorMessage = ex.Message;
        return false;
      }
    }
  }
}
