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

    public override IInstallerUiLifecycle CreateLifecycle()
    {
      return new ExampleInstallerUiLifecycle();
    }

    public override void ConfigureSequenceHooks(IInstallerUiContext context)
    {
      ExampleInstallerUiSequenceHooks.Configure(context);
    }
  }
}
