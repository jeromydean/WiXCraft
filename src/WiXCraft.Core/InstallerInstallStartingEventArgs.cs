using System;

namespace WiXCraft
{
  public sealed class InstallerInstallStartingEventArgs : EventArgs
  {
    public InstallerInstallStartingEventArgs(InstallOperation operation)
    {
      Operation = operation;
    }

    public InstallOperation Operation { get; }

    public bool Cancel { get; set; }
  }
}
