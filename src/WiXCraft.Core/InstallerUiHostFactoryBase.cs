namespace WiXCraft
{
  public abstract class InstallerUiHostFactoryBase : IInstallerUiHostFactory
  {
    public abstract IInstallerUiHost CreateHost();

    public virtual InstallerUiModeOptions CreateModeOptions()
    {
      return InstallerUiModeOptions.CreateDefault();
    }

    public virtual IInstallerUiLifecycle CreateLifecycle()
    {
      return null;
    }
  }
}
