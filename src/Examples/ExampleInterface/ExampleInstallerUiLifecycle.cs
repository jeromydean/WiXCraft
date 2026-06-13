using System.Collections.Generic;
using WiXCraft;

namespace ExampleInterface
{
  public sealed class ExampleInstallerUiLifecycle : IInstallerUiLifecycle
  {
    public void OnInitializing(IInstallerUiContext context)
    {
      context.InstallProperties.TrySet("UI_INITIALIZED", "1");
    }

    public void OnInstallStarting(IInstallerUiContext context, InstallerInstallStartingEventArgs args)
    {
      context.InstallProperties.TrySet(
        string.Concat("PENDING_", args.Operation.ToString().ToUpperInvariant()),
        "1");
    }

    public void OnInstallCompleted(IInstallerUiContext context, InstallerInstallCompletedEventArgs args)
    {
      context.InstallProperties.TrySet("LAST_INSTALL_SUCCEEDED", args.Succeeded ? "1" : "0");
      context.InstallProperties.TrySet("LAST_INSTALL_OPERATION", args.Operation.ToString());
    }
  }
}
