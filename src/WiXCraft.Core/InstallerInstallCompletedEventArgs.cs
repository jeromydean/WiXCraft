using System;

namespace WiXCraft
{
  public sealed class InstallerInstallCompletedEventArgs : EventArgs
  {
    public InstallerInstallCompletedEventArgs(InstallOperation operation, bool succeeded)
    {
      Operation = operation;
      Succeeded = succeeded;
    }

    public InstallOperation Operation { get; }

    public bool Succeeded { get; }
  }
}
