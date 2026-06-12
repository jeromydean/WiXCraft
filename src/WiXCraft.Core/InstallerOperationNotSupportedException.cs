using System;

namespace WiXCraft
{
  public sealed class InstallerOperationNotSupportedException : InvalidOperationException
  {
    public InstallerOperationNotSupportedException(InstallOperation operation)
      : base(string.Concat("The embedded UI does not support the selected operation: ", operation, "."))
    {
      Operation = operation;
    }

    public InstallOperation Operation { get; }
  }
}
