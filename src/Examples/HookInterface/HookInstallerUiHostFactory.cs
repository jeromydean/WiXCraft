using WiXCraft;

namespace HookInterface
{
  public sealed class HookInstallerUiHostFactory : InstallerUiHostFactoryBase
  {
    public override IInstallerUiHost CreateHost()
    {
      return new HookInstallerUiHost();
    }
  }
}
