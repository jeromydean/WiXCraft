using System;
using System.Threading;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft.CustomActions
{
  public static class CustomActions
  {
    /// <summary>
    /// Deferred CA invoked before finalize. When the embedded UI user cancels, it creates a named mutex;
    /// finding that mutex here tells MSI to abort the install sequence.
    /// </summary>
    [CustomAction]
    public static ActionResult CheckEmbeddedUICancellation(Session session)
    {
      try
      {
        string cancellationMutexName = GetCancellationMutexName(session);
        if (string.IsNullOrEmpty(cancellationMutexName))
        {
          return ActionResult.Success;
        }

        if (CancellationMutexExists(cancellationMutexName))
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

    private static string GetCancellationMutexName(Session session)
    {
      if (session.CustomActionData != null)
      {
        foreach (string key in session.CustomActionData.Keys)
        {
          if (string.Equals(key, "EMBEDDEDUICANCELLATIONMUTEXNAME", StringComparison.OrdinalIgnoreCase))
          {
            return session.CustomActionData[key];
          }
        }
      }

      return session["EMBEDDEDUICANCELLATIONMUTEXNAME"];
    }

    private static bool CancellationMutexExists(string cancellationMutexName)
    {
      try
      {
        using (Mutex.OpenExisting(cancellationMutexName))
        {
          return true;
        }
      }
      catch (WaitHandleCannotBeOpenedException)
      {
        return false;
      }
    }
  }
}
