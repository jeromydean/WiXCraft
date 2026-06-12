using System.Threading.Tasks;

namespace WiXCraft
{
  public delegate Task<SequenceHookResult> InstallerSequenceHookAsyncHandler(
    InstallerSequenceHookContext context);
}
