using System;
using System.Threading.Tasks;
using System.Windows;
using WiXCraft;

namespace HookInterface
{
  internal static class HookInstallerUiSequenceHooks
  {
    public static void Configure(IInstallerUiContext context, Action<string> reportStatus)
    {
      if (context == null)
      {
        throw new ArgumentNullException(nameof(context));
      }

      context.SequenceHooks.RegisterAsync("BeforeInstallFiles", async ctx =>
      {
        reportStatus?.Invoke("Async hook: checking prerequisites...");
        ctx.Context.InstallProperties.TrySet("HOOK_BEFORE_INSTALLFILES", "1");

        await Task.Delay(1500).ConfigureAwait(true);

        MessageBoxResult result = MessageBox.Show(
          ctx.Payload,
          "Async hook: BeforeInstallFiles",
          MessageBoxButton.OKCancel,
          MessageBoxImage.Question);

        return result == MessageBoxResult.Cancel
          ? SequenceHookResult.Cancel
          : SequenceHookResult.Continue;
      });

      context.SequenceHooks.Register("BeforeApplySessionProperties", ctx =>
      {
        ctx.Context.InstallProperties.TrySet("HOOK_BEFORE_DEFERRED", "1");

        string message = string.Concat(
          ctx.Payload,
          Environment.NewLine,
          Environment.NewLine,
          "The next step is WiXCraft_ApplySessionProperties — a deferred custom action ",
          "(Execute=deferred, Impersonate=no) in the elevated execute sequence.");

        MessageBoxResult result = MessageBox.Show(
          message,
          "Hook before elevated deferred action",
          MessageBoxButton.OKCancel,
          MessageBoxImage.Information);

        return result == MessageBoxResult.Cancel
          ? SequenceHookResult.Cancel
          : SequenceHookResult.Continue;
      });
    }
  }
}
