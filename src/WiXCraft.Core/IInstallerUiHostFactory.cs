namespace WiXCraft
{
  public interface IInstallerUiHostFactory
  {
    IInstallerUiHost CreateHost();

    InstallerUiModeOptions CreateModeOptions();

    IInstallerUiLifecycle CreateLifecycle();

    void ConfigureSequenceHooks(IInstallerUiContext context);
  }
}