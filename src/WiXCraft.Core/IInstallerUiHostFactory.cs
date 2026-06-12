namespace WiXCraft
{
  public interface IInstallerUiHostFactory
  {
    IInstallerUiHost CreateHost();

    InstallerUiModeOptions CreateModeOptions();
  }
}