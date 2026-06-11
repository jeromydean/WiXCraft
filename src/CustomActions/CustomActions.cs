using System;
using System.Threading;
using WixToolset.Dtf.WindowsInstaller;

namespace CustomActions
{
  public static class CustomActions
  {
    /// <summary>
    /// Deferred CA invoked before finalize. When the embedded UI user cancels, it holds a named mutex;
    /// acquiring an existing mutex here tells MSI to abort the install sequence.
    /// </summary>
    [CustomAction]
    public static ActionResult CheckEmbeddedUICancellation(Session session)
    {
      try
      {
        string? cancellationMutexName = session.CustomActionData["EMBEDDEDUICANCELLATIONMUTEXNAME"];
        if (string.IsNullOrEmpty(cancellationMutexName))
        {
          cancellationMutexName = session["EMBEDDEDUICANCELLATIONMUTEXNAME"];
        }

        if (string.IsNullOrEmpty(cancellationMutexName))
        {
          return ActionResult.Success;
        }

        using Mutex cancellationMutex = new Mutex(false, cancellationMutexName, out bool createdNew);
        if (!createdNew)
        {
          session.Log("Installation cancelled by user.");
          return ActionResult.UserExit;
        }

        return ActionResult.Success;
      }
      catch (Exception ex)
      {
        session.Log($"CheckEmbeddedUICancellation failed: {ex}");
        return ActionResult.Failure;
      }
    }
  }
}
