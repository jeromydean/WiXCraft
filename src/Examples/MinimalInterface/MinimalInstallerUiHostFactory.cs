using WiXCraft;

namespace MinimalInterface
{
  public sealed class MinimalInstallerUiHostFactory : InstallerUiHostFactoryBase
  {
    public override IInstallerUiHost CreateHost()
    {
      return new MinimalInstallerUiHost();
    }
  }
}
