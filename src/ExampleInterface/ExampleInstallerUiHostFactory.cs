using WiXCraft;

namespace ExampleInterface
{
  public sealed class ExampleInstallerUiHostFactory : InstallerUiHostFactoryBase
  {
    public override IInstallerUiHost CreateHost()
    {
      return new ExampleInstallerUiHost();
    }

    public override InstallerUiModeOptions CreateModeOptions()
    {
      return InstallerUiModeOptions.CreateDefault();
    }
  }
}
