using System;
using System.Threading.Tasks;

namespace WiXCraft
{
  public interface IInstallerSequenceHookAsyncInvoker
  {
    SequenceHookResult Invoke(Func<Task<SequenceHookResult>> work);
  }
}
