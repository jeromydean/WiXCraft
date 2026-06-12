namespace WiXCraft
{
  public interface IInstallerUiLifecycle
  {
    void OnInitializing(IInstallerUiContext context);

    void OnInstallStarting(IInstallerUiContext context, InstallerInstallStartingEventArgs args);

    void OnInstallCompleted(IInstallerUiContext context, InstallerInstallCompletedEventArgs args);
  }
}
