using System;
using System.Collections.Generic;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  public sealed class InstallerExecuteSequenceObserver
  {
    private readonly List<InstallerExecuteSequenceEntry> entries = new List<InstallerExecuteSequenceEntry>();
    private readonly Dictionary<string, List<Action<InstallerExecuteSequenceActionEventArgs>>> actionHandlers =
      new Dictionary<string, List<Action<InstallerExecuteSequenceActionEventArgs>>>(StringComparer.OrdinalIgnoreCase);

    private string currentActionName;

    public event EventHandler ExecuteSequenceStarted;

    public event EventHandler ExecuteSequenceEnded;

    public event EventHandler<InstallerExecuteSequenceActionEventArgs> ActionStarted;

    public event EventHandler<InstallerExecuteSequenceActionCompletedEventArgs> ActionCompleted;

    public event EventHandler<InstallerExecuteSequenceActionEventArgs> ActionProgress;

    public event EventHandler<InstallerExecuteSequenceEntry> EntryAdded;

    public IReadOnlyList<InstallerExecuteSequenceEntry> Entries => entries;

    public void WhenAction(string actionName, Action<InstallerExecuteSequenceActionEventArgs> handler)
    {
      if (string.IsNullOrWhiteSpace(actionName))
      {
        throw new ArgumentException("Action name is required.", nameof(actionName));
      }

      if (handler == null)
      {
        throw new ArgumentNullException(nameof(handler));
      }

      if (!actionHandlers.TryGetValue(actionName, out List<Action<InstallerExecuteSequenceActionEventArgs>> handlers))
      {
        handlers = new List<Action<InstallerExecuteSequenceActionEventArgs>>();
        actionHandlers[actionName] = handlers;
      }

      handlers.Add(handler);
    }

    public void Clear()
    {
      entries.Clear();
      currentActionName = null;
    }

    public void ProcessMessage(InstallMessage messageType, Record messageRecord)
    {
      switch (messageType)
      {
        case InstallMessage.InstallStart:
          AddEntry(
            InstallerExecuteSequenceEntryKind.ExecuteStarted,
            string.Empty,
            "Execute sequence started");
          ExecuteSequenceStarted?.Invoke(this, EventArgs.Empty);
          break;

        case InstallMessage.InstallEnd:
          MarkActionCompleted(currentActionName);
          AddEntry(
            InstallerExecuteSequenceEntryKind.ExecuteEnded,
            string.Empty,
            "Execute sequence ended");
          ExecuteSequenceEnded?.Invoke(this, EventArgs.Empty);
          currentActionName = null;
          break;

        case InstallMessage.ActionStart:
          ProcessActionStart(messageRecord);
          break;

        case InstallMessage.ActionData:
          ProcessActionData(messageRecord);
          break;
      }
    }

    private void ProcessActionStart(Record messageRecord)
    {
      string actionName = GetRecordString(messageRecord, 1);
      string description = GetRecordString(messageRecord, 2);
      if (string.IsNullOrWhiteSpace(actionName))
      {
        actionName = description;
      }

      if (!string.IsNullOrWhiteSpace(currentActionName) &&
          !string.Equals(currentActionName, actionName, StringComparison.OrdinalIgnoreCase))
      {
        MarkActionCompleted(currentActionName);
      }

      currentActionName = actionName;
      DateTimeOffset timestamp = DateTimeOffset.Now;
      InstallerExecuteSequenceActionEventArgs args =
        new InstallerExecuteSequenceActionEventArgs(actionName, description, timestamp);

      AddEntry(
        InstallerExecuteSequenceEntryKind.ActionStarted,
        actionName,
        string.IsNullOrWhiteSpace(description) ? actionName : description);

      ActionStarted?.Invoke(this, args);
      InvokeWhenActionHandlers(actionName, args);
    }

    private void MarkActionCompleted(string actionName)
    {
      if (string.IsNullOrWhiteSpace(actionName))
      {
        return;
      }

      DateTimeOffset timestamp = DateTimeOffset.Now;
      AddEntry(
        InstallerExecuteSequenceEntryKind.ActionCompleted,
        actionName,
        "Completed");

      ActionCompleted?.Invoke(
        this,
        new InstallerExecuteSequenceActionCompletedEventArgs(actionName, timestamp));
    }

    private void InvokeWhenActionHandlers(
      string actionName,
      InstallerExecuteSequenceActionEventArgs args)
    {
      if (!actionHandlers.TryGetValue(actionName, out List<Action<InstallerExecuteSequenceActionEventArgs>> handlers))
      {
        return;
      }

      foreach (Action<InstallerExecuteSequenceActionEventArgs> handler in handlers.ToArray())
      {
        handler(args);
      }
    }

    private void ProcessActionData(Record messageRecord)
    {
      string actionData = GetRecordString(messageRecord, 1);
      if (string.IsNullOrWhiteSpace(actionData))
      {
        return;
      }

      string actionName = currentActionName ?? string.Empty;
      DateTimeOffset timestamp = DateTimeOffset.Now;
      InstallerExecuteSequenceActionEventArgs args =
        new InstallerExecuteSequenceActionEventArgs(actionName, actionData, timestamp);

      AddEntry(
        InstallerExecuteSequenceEntryKind.ActionProgress,
        actionName,
        actionData);

      ActionProgress?.Invoke(this, args);
    }

    private void AddEntry(
      InstallerExecuteSequenceEntryKind kind,
      string actionName,
      string detail)
    {
      InstallerExecuteSequenceEntry entry = new InstallerExecuteSequenceEntry(
        kind,
        actionName,
        detail,
        DateTimeOffset.Now);

      entries.Add(entry);
      EntryAdded?.Invoke(this, entry);
    }

    private static string GetRecordString(Record messageRecord, int fieldIndex)
    {
      if (messageRecord == null || messageRecord.FieldCount < fieldIndex)
      {
        return string.Empty;
      }

      return messageRecord.GetString(fieldIndex) ?? string.Empty;
    }
  }
}
