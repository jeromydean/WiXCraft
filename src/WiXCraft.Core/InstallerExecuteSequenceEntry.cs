using System;

namespace WiXCraft
{
  public sealed class InstallerExecuteSequenceEntry
  {
    public InstallerExecuteSequenceEntry(
      InstallerExecuteSequenceEntryKind kind,
      string actionName,
      string detail,
      DateTimeOffset timestamp)
    {
      Kind = kind;
      ActionName = actionName ?? string.Empty;
      Detail = detail ?? string.Empty;
      Timestamp = timestamp;
    }

    public InstallerExecuteSequenceEntryKind Kind { get; }

    public string ActionName { get; }

    public string Detail { get; }

    public DateTimeOffset Timestamp { get; }
  }
}
