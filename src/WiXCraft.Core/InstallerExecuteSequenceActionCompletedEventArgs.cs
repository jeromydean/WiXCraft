using System;
using System.Collections.Generic;

namespace WiXCraft
{
  public sealed class InstallerExecuteSequenceActionCompletedEventArgs : EventArgs
  {
    public InstallerExecuteSequenceActionCompletedEventArgs(
      string actionName,
      DateTimeOffset timestamp)
    {
      ActionName = actionName ?? string.Empty;
      Timestamp = timestamp;
    }

    public string ActionName { get; }

    public DateTimeOffset Timestamp { get; }
  }
}
