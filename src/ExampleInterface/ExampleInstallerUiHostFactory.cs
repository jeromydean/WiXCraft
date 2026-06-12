using WiXCraft;

namespace ExampleInterface
{
  public sealed class ExampleInstallerUiHostFactory : IInstallerUiHostFactory
  {
    public IInstallerUiHost CreateHost()
    {
      return new ExampleInstallerUiHost();
    }
  }
}