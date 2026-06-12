using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WiXCraft.Wpf
{
  public sealed class DispatcherSequenceHookAsyncInvoker : IInstallerSequenceHookAsyncInvoker
  {
    private readonly Dispatcher dispatcher;

    public DispatcherSequenceHookAsyncInvoker(Dispatcher dispatcher)
    {
      this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public SequenceHookResult Invoke(Func<Task<SequenceHookResult>> work)
    {
      if (work == null)
      {
        throw new ArgumentNullException(nameof(work));
      }

      if (!dispatcher.CheckAccess())
      {
        return dispatcher.Invoke(() => Invoke(work));
      }

      return RunOnDispatcher(work);
    }

    private SequenceHookResult RunOnDispatcher(Func<Task<SequenceHookResult>> work)
    {
      SequenceHookResult result = SequenceHookResult.Continue;
      Exception fault = null;
      DispatcherFrame frame = new DispatcherFrame();

      async void RunAsync()
      {
        try
        {
          result = await work().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
          fault = ex;
        }
        finally
        {
          frame.Continue = false;
        }
      }

      RunAsync();
      Dispatcher.PushFrame(frame);

      if (fault != null)
      {
        throw fault;
      }

      return result;
    }
  }
}
