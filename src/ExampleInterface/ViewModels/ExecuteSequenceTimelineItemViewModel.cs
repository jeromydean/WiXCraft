using System;
using WiXCraft;

namespace ExampleInterface.ViewModels
{
  public sealed class ExecuteSequenceTimelineItemViewModel
  {
    public ExecuteSequenceTimelineItemViewModel(InstallerExecuteSequenceEntry entry)
    {
      Kind = entry.Kind;
      ActionName = entry.ActionName;
      Detail = entry.Detail;
      Timestamp = entry.Timestamp;
    }

    public InstallerExecuteSequenceEntryKind Kind { get; }

    public string ActionName { get; }

    public string Detail { get; }

    public DateTimeOffset Timestamp { get; }

    public string TimestampText => Timestamp.ToString("HH:mm:ss.fff");

    public string KindLabel
    {
      get
      {
        switch (Kind)
        {
          case InstallerExecuteSequenceEntryKind.ExecuteStarted:
            return "Start";

          case InstallerExecuteSequenceEntryKind.ExecuteEnded:
            return "End";

          case InstallerExecuteSequenceEntryKind.ActionStarted:
            return "Action";

          case InstallerExecuteSequenceEntryKind.ActionProgress:
            return "Progress";

          case InstallerExecuteSequenceEntryKind.ActionCompleted:
            return "Done";

          default:
            return Kind.ToString();
        }
      }
    }

    public string DisplayText
    {
      get
      {
        switch (Kind)
        {
          case InstallerExecuteSequenceEntryKind.ActionStarted:
            return string.IsNullOrWhiteSpace(ActionName)
              ? Detail
              : string.Concat(ActionName, string.IsNullOrWhiteSpace(Detail) ? string.Empty : string.Concat(" — ", Detail));

          case InstallerExecuteSequenceEntryKind.ActionProgress:
            return string.IsNullOrWhiteSpace(ActionName)
              ? Detail
              : string.Concat(ActionName, ": ", Detail);

          case InstallerExecuteSequenceEntryKind.ActionCompleted:
            return string.Concat(ActionName, " completed");

          default:
            return Detail;
        }
      }
    }
  }
}
