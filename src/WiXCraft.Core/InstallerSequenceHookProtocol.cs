namespace WiXCraft
{
  public static class InstallerSequenceHookProtocol
  {
    public const string HookIdPrefix = "WIXCRAFT_HOOK:";

    public static string FormatHookMessageId(string hookId)
    {
      return string.Concat(HookIdPrefix, hookId);
    }

    public static bool TryParseHookMessageId(string messageId, out string hookId)
    {
      hookId = null;
      if (string.IsNullOrWhiteSpace(messageId))
      {
        return false;
      }

      if (!messageId.StartsWith(HookIdPrefix, System.StringComparison.OrdinalIgnoreCase))
      {
        return false;
      }

      hookId = messageId.Substring(HookIdPrefix.Length);
      return !string.IsNullOrWhiteSpace(hookId);
    }
  }
}
