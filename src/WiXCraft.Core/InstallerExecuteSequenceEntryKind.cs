using System;

namespace WiXCraft
{
  public enum InstallerExecuteSequenceEntryKind
  {
    ExecuteStarted,
    ExecuteEnded,
    ActionStarted,
    ActionProgress,
    ActionCompleted,
  }
}
