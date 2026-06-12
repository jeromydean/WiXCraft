using System;
using System.Threading;

namespace WiXCraft
{
  internal sealed class CancellationCoordinator : IDisposable
  {
    private readonly string mutexName;
    private Mutex cancellationMutex;

    public CancellationCoordinator(string mutexName)
    {
      this.mutexName = mutexName;
    }

    public void Arm(IInstallerSession session)
    {
      if (string.IsNullOrEmpty(mutexName))
      {
        return;
      }

      session["EMBEDDEDUICANCELLATIONMUTEXNAME"] = mutexName;
    }

    public void SignalCancel()
    {
      if (string.IsNullOrEmpty(mutexName))
      {
        return;
      }

      if (cancellationMutex == null)
      {
        cancellationMutex = new Mutex(false, mutexName, out bool createdNew);
        if (!createdNew)
        {
          try
          {
            cancellationMutex.WaitOne(0);
          }
          catch (AbandonedMutexException)
          {
          }
        }
      }
    }

    public void Dispose()
    {
      cancellationMutex?.Dispose();
      cancellationMutex = null;
    }
  }
}
