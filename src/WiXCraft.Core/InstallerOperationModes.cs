using System;

namespace WiXCraft
{
  [Flags]
  public enum InstallerOperationModes
  {
    None = 0,
    FreshInstall = 1,
    Repair = 2,
    Modify = 4,
    Uninstall = 8,
    Upgrade = 16,
    All = FreshInstall | Repair | Modify | Uninstall | Upgrade,
  }
}
