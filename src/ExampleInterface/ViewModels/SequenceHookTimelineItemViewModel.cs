using System;
using WiXCraft;

namespace ExampleInterface.ViewModels
{
  public sealed class SequenceHookTimelineItemViewModel
  {
    public SequenceHookTimelineItemViewModel(InstallerSequenceHookContext context, SequenceHookResult result)
    {
      HookId = context.HookId;
      Payload = context.Payload;
      Result = result;
      Cancelled = context.Cancel || result == SequenceHookResult.Cancel;
      Timestamp = DateTimeOffset.Now;
    }

    public string HookId { get; }

    public string Payload { get; }

    public SequenceHookResult Result { get; }

    public bool Cancelled { get; }

    public DateTimeOffset Timestamp { get; }

    public string TimestampText => Timestamp.ToString("HH:mm:ss.fff");

    public string DisplayText =>
      string.IsNullOrWhiteSpace(Payload)
        ? HookId
        : string.Concat(HookId, " — ", Payload);
  }
}
