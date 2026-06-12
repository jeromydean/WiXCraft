using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WiXCraft.Wpf
{
  public static class InstallerSequenceHookWpfExtensions
  {
    public static void ConfigureSequenceHookAsyncInvoker(
      this IInstallerUiContext context,
      Dispatcher dispatcher)
    {
      if (context == null)
      {
        throw new ArgumentNullException(nameof(context));
      }

      context.SequenceHookAsyncInvoker =
        new DispatcherSequenceHookAsyncInvoker(dispatcher);
    }

    public static void RegisterAsync(
      this InstallerSequenceHookRegistry registry,
      string hookId,
      Dispatcher dispatcher,
      Func<InstallerSequenceHookContext, Task<SequenceHookResult>> handler)
    {
      if (registry == null)
      {
        throw new ArgumentNullException(nameof(registry));
      }

      if (dispatcher == null)
      {
        throw new ArgumentNullException(nameof(dispatcher));
      }

      if (handler == null)
      {
        throw new ArgumentNullException(nameof(handler));
      }

      IInstallerSequenceHookAsyncInvoker invoker =
        new DispatcherSequenceHookAsyncInvoker(dispatcher);

      registry.Register(hookId, ctx => invoker.Invoke(() => handler(ctx)));
    }
  }
}
