using WiXCraft;

namespace ExampleInterface
{
  public sealed class ExampleInstallerUiLifecycle : IInstallerUiLifecycle
  {
    public void OnInitializing(IInstallerUiContext context)
    {
      context.Session.TrySetProperty("WIXCRAFT_UI_INITIALIZED", "1");
    }

    public void OnInstallStarting(IInstallerUiContext context, InstallerInstallStartingEventArgs args)
    {
      context.Session.TrySetProperty(
        string.Concat("WIXCRAFT_PENDING_", args.Operation.ToString().ToUpperInvariant()),
        "1");
    }

    public void OnInstallCompleted(IInstallerUiContext context, InstallerInstallCompletedEventArgs args)
    {
      context.Session.TrySetProperty("WIXCRAFT_LAST_INSTALL_SUCCEEDED", args.Succeeded ? "1" : "0");
      context.Session.TrySetProperty("WIXCRAFT_LAST_INSTALL_OPERATION", args.Operation.ToString());
    }
  }
}
