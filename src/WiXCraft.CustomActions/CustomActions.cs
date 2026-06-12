using System;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft.CustomActions
{
  public static class CustomActions
  {
    private const string HookIdPrefix = "WIXCRAFT_HOOK:";

    /// <summary>
    /// Deferred CA invoked before finalize. When the embedded UI user cancels, it creates a named mutex;
    /// finding that mutex here tells MSI to abort the install sequence.
    /// </summary>
    [CustomAction]
    public static ActionResult CheckEmbeddedUICancellation(Session session)
    {
      try
      {
        string cancellationMutexName = GetCancellationMutexName(session);
        if (string.IsNullOrEmpty(cancellationMutexName))
        {
          return ActionResult.Success;
        }

        if (CancellationMutexExists(cancellationMutexName))
        {
          session.Log("Installation cancelled by user.");
          return ActionResult.UserExit;
        }

        return ActionResult.Success;
      }
      catch (Exception ex)
      {
        session.Log($"CheckEmbeddedUICancellation failed: {ex}");
        return ActionResult.Failure;
      }
    }

    /// <summary>
    /// Immediate CA that raises a WiXCraft sequence hook message to the embedded UI.
    /// Expects session property WiXCraft_SequenceHook=HOOKID=...;PAYLOAD=...;BUTTONS=...
    /// </summary>
    [CustomAction]
    public static ActionResult SequenceHook(Session session)
    {
      try
      {
        string hookData = session["WiXCraft_SequenceHook"];
        if (string.IsNullOrWhiteSpace(hookData))
        {
          hookData = GetCustomActionDataValue(session, "WiXCraft_SequenceHook");
        }

        if (string.IsNullOrWhiteSpace(hookData))
        {
          session.Log("SequenceHook skipped: WiXCraft_SequenceHook is missing.");
          return ActionResult.Success;
        }

        ParseHookData(
          hookData,
          out string hookId,
          out string payload,
          out string buttons);

        if (string.IsNullOrWhiteSpace(hookId))
        {
          session.Log("SequenceHook skipped: HOOKID is missing.");
          return ActionResult.Success;
        }

        session.Log(string.Concat("SequenceHook invoking embedded UI hook: ", hookId));

        using (Record record = new Record(2))
        {
          record.SetString(1, string.Concat(HookIdPrefix, hookId));
          record.SetString(2, payload);

          InstallMessage messageType = InstallMessage.User
            | (InstallMessage)(int)MapButtons(buttons)
            | (InstallMessage)(int)MessageIcon.Information
            | (InstallMessage)(int)MessageDefaultButton.Button1;

          session.Message(messageType, record);
        }

        return ActionResult.Success;
      }
      catch (InstallCanceledException)
      {
        session.Log("SequenceHook cancelled by user.");
        return ActionResult.UserExit;
      }
      catch (Exception ex)
      {
        session.Log($"SequenceHook failed: {ex}");
        return ActionResult.Failure;
      }
    }

    /// <summary>
    /// Deferred stub that documents the Tier 2 property pattern. UI should set public properties
    /// before install starts; WiX standard actions consume them declaratively.
    /// </summary>
    [CustomAction]
    public static ActionResult ApplySessionProperties(Session session)
    {
      try
      {
        session.Log(
          "WiXCraft ApplySessionProperties: deferred stub. " +
          "Configure MSI declaratively from UI session properties set before install starts.");
        return ActionResult.Success;
      }
      catch (Exception ex)
      {
        session.Log($"ApplySessionProperties failed: {ex}");
        return ActionResult.Failure;
      }
    }

    private static string GetCancellationMutexName(Session session)
    {
      if (TryGetCustomActionDataValue(session, "EMBEDDEDUICANCELLATIONMUTEXNAME", out string mutexName))
      {
        return mutexName;
      }

      try
      {
        return session["EMBEDDEDUICANCELLATIONMUTEXNAME"] ?? string.Empty;
      }
      catch (InstallerException)
      {
        return string.Empty;
      }
    }

    private static void ParseHookData(
      string hookData,
      out string hookId,
      out string payload,
      out string buttons)
    {
      hookId = string.Empty;
      payload = string.Empty;
      buttons = "OKCancel";

      string[] parts = hookData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      foreach (string part in parts)
      {
        int separatorIndex = part.IndexOf('=');
        if (separatorIndex <= 0)
        {
          continue;
        }

        string key = part.Substring(0, separatorIndex).Trim();
        string value = part.Substring(separatorIndex + 1).Trim();
        if (string.Equals(key, "HOOKID", StringComparison.OrdinalIgnoreCase))
        {
          hookId = value;
        }
        else if (string.Equals(key, "PAYLOAD", StringComparison.OrdinalIgnoreCase))
        {
          payload = value;
        }
        else if (string.Equals(key, "BUTTONS", StringComparison.OrdinalIgnoreCase))
        {
          buttons = value;
        }
      }
    }

    private static string GetCustomActionDataValue(Session session, string key)
    {
      if (TryGetCustomActionDataValue(session, key, out string value))
      {
        return value;
      }

      try
      {
        return session[key] ?? string.Empty;
      }
      catch (InstallerException)
      {
        return string.Empty;
      }
    }

    private static bool TryGetCustomActionDataValue(Session session, string key, out string value)
    {
      value = string.Empty;

      if (session.CustomActionData != null)
      {
        foreach (string dataKey in session.CustomActionData.Keys)
        {
          if (string.Equals(dataKey, key, StringComparison.OrdinalIgnoreCase))
          {
            value = session.CustomActionData[dataKey] ?? string.Empty;
            return true;
          }
        }
      }

      if (TryReadCustomActionDataString(session, out string customActionData) &&
          TryParseCustomActionDataEntry(customActionData, key, out value))
      {
        return true;
      }

      return false;
    }

    private static bool TryReadCustomActionDataString(Session session, out string customActionData)
    {
      customActionData = string.Empty;

      try
      {
        customActionData = session["CustomActionData"];
        return !string.IsNullOrEmpty(customActionData);
      }
      catch (InstallerException)
      {
        return false;
      }
    }

    private static bool TryParseCustomActionDataEntry(string customActionData, string key, out string value)
    {
      value = string.Empty;

      if (string.IsNullOrWhiteSpace(customActionData))
      {
        return false;
      }

      string[] parts = customActionData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      foreach (string part in parts)
      {
        int separatorIndex = part.IndexOf('=');
        if (separatorIndex <= 0)
        {
          continue;
        }

        string partKey = part.Substring(0, separatorIndex).Trim();
        if (!string.Equals(partKey, key, StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        value = separatorIndex + 1 < part.Length
          ? part.Substring(separatorIndex + 1).Trim()
          : string.Empty;
        return true;
      }

      return false;
    }

    private static MessageButtons MapButtons(string buttons)
    {
      switch (buttons.ToUpperInvariant())
      {
        case "OK":
          return MessageButtons.OK;

        case "YESNO":
          return MessageButtons.YesNo;

        case "RETRYCANCEL":
          return MessageButtons.RetryCancel;

        default:
          return MessageButtons.OKCancel;
      }
    }

    private static bool CancellationMutexExists(string cancellationMutexName)
    {
      try
      {
        using (System.Threading.Mutex.OpenExisting(cancellationMutexName))
        {
          return true;
        }
      }
      catch (System.Threading.WaitHandleCannotBeOpenedException)
      {
        return false;
      }
    }
  }
}
