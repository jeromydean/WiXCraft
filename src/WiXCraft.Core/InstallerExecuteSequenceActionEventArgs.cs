using System;

namespace WiXCraft
{
  public sealed class InstallerExecuteSequenceActionEventArgs : EventArgs
  {
    public InstallerExecuteSequenceActionEventArgs(
      string actionName,
      string description,
      DateTimeOffset timestamp)
    {
      ActionName = actionName ?? string.Empty;
      Description = description ?? string.Empty;
      Timestamp = timestamp;
    }

    public string ActionName { get; }

    public string Description { get; }

    public DateTimeOffset Timestamp { get; }
  }
}
