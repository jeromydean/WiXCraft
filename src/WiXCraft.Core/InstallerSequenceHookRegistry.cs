using System;
using System.Collections.Generic;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  public sealed class InstallerSequenceHookRegistry
  {
    private readonly Dictionary<string, InstallerSequenceHookHandler> handlers =
      new Dictionary<string, InstallerSequenceHookHandler>(StringComparer.OrdinalIgnoreCase);

    public event EventHandler<InstallerSequenceHookContext> HookInvoked;

    public void Register(string hookId, InstallerSequenceHookHandler handler)
    {
      if (string.IsNullOrWhiteSpace(hookId))
      {
        throw new ArgumentException("Hook id is required.", nameof(hookId));
      }

      if (handler == null)
      {
        throw new ArgumentNullException(nameof(handler));
      }

      handlers[hookId] = handler;
    }

    public void RegisterAsync(string hookId, InstallerSequenceHookAsyncHandler handler)
    {
      if (string.IsNullOrWhiteSpace(hookId))
      {
        throw new ArgumentException("Hook id is required.", nameof(hookId));
      }

      if (handler == null)
      {
        throw new ArgumentNullException(nameof(handler));
      }

      Register(hookId, ctx => InvokeAsyncHandler(ctx, handler));
    }

    public bool IsRegistered(string hookId)
    {
      return !string.IsNullOrWhiteSpace(hookId) && handlers.ContainsKey(hookId);
    }

    public bool TryHandle(
      IInstallerUiContext context,
      InstallMessageEventArgs message,
      out MessageResult messageResult)
    {
      messageResult = MessageResult.OK;

      if (message.MessageType != InstallMessage.User ||
          message.MessageRecord == null ||
          message.MessageRecord.FieldCount < 1)
      {
        return false;
      }

      string messageId = message.MessageRecord.GetString(1);
      if (!InstallerSequenceHookProtocol.TryParseHookMessageId(messageId, out string hookId))
      {
        return false;
      }

      if (!handlers.TryGetValue(hookId, out InstallerSequenceHookHandler handler))
      {
        messageResult = MessageResult.OK;
        return true;
      }

      string payload = message.MessageRecord.FieldCount >= 2
        ? message.MessageRecord.GetString(2) ?? string.Empty
        : string.Empty;

      InstallerSequenceHookContext hookContext =
        new InstallerSequenceHookContext(context, hookId, payload);

      SequenceHookResult hookResult = handler(hookContext);
      HookInvoked?.Invoke(this, hookContext);

      if (hookContext.Cancel || hookResult == SequenceHookResult.Cancel)
      {
        messageResult = MessageResult.Cancel;
        return true;
      }

      messageResult = MessageResult.OK;
      return true;
    }

    private static SequenceHookResult InvokeAsyncHandler(
      InstallerSequenceHookContext context,
      InstallerSequenceHookAsyncHandler handler)
    {
      IInstallerSequenceHookAsyncInvoker invoker = context.Context.SequenceHookAsyncInvoker;
      if (invoker == null)
      {
        throw new InvalidOperationException(
          "SequenceHookAsyncInvoker is not configured on the UI context. " +
          "Call ConfigureSequenceHookAsyncInvoker from WiXCraft.Wpf in IInstallerUiHost.Run, " +
          "or use RegisterAsync(registry, hookId, dispatcher, handler) when you have a Dispatcher.");
      }

      return invoker.Invoke(() => handler(context));
    }
  }
}
