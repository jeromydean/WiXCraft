using System.Threading.Tasks;
using WiXCraft;

namespace ExampleInterface
{
  internal static class ExampleInstallerUiSequenceHooks
  {
    public static void Configure(IInstallerUiContext context)
    {
      context.SequenceHooks.RegisterAsync("BeforeInstallFiles", async ctx =>
      {
        ctx.Context.InstallProperties.TrySet("SEQUENCE_HOOK_BEFORE_INSTALLFILES", "1");

        await Task.Delay(250).ConfigureAwait(true);

        if (string.IsNullOrWhiteSpace(ctx.Payload))
        {
          return SequenceHookResult.Continue;
        }

        return SequenceHookResult.Continue;
      });
    }
  }
}
