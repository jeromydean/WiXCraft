using System;
using System.Threading;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  public sealed class EmbeddedUiEngine : IDisposable
  {
    private readonly IInstallerUiHostFactory hostFactory;
    private readonly ManualResetEvent installStartEvent = new ManualResetEvent(false);
    private readonly ManualResetEvent installExitEvent = new ManualResetEvent(false);

    private IInstallerUiHost host;
    private InstallerUiContext context;
    private CancellationCoordinator cancellation;
    private Thread appThread;
    private Session session;

    public EmbeddedUiEngine(IInstallerUiHostFactory hostFactory)
    {
      this.hostFactory = hostFactory ?? throw new ArgumentNullException(nameof(hostFactory));
    }

    public bool Initialize(Session session, string resourcePath, ref InstallUIOptions internalUILevel)
    {
      this.session = session;

      if (session != null)
      {
        if ((internalUILevel & InstallUIOptions.Full) != InstallUIOptions.Full)
        {
          return false;
        }
      }

      InstallerSession installerSession = new InstallerSession(session);
      MaintenanceLaunchAction maintenanceLaunchAction =
        PrepareMaintenanceLaunch(session, installerSession);
      context = new InstallerUiContext(installerSession, resourcePath, maintenanceLaunchAction);
      cancellation = new CancellationCoordinator(Guid.NewGuid().ToString("N"));
      cancellation.Arm(installerSession);
      context.CancelRequested += (_, __) => cancellation.SignalCancel();

      host = hostFactory.CreateHost();
      appThread = new Thread(RunHost);
      appThread.SetApartmentState(ApartmentState.STA);
      appThread.IsBackground = true;
      appThread.Start();

      int waitResult = WaitHandle.WaitAny(new WaitHandle[] { installStartEvent, installExitEvent });
      if (waitResult == 1)
      {
        throw new InstallCanceledException();
      }

      ApplySelectedOperation(session, context.SelectedOperation);

      internalUILevel = InstallUIOptions.NoChange | InstallUIOptions.SourceResolutionOnly;
      return true;
    }

    public MessageResult ProcessMessage(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton)
    {
      if (host == null)
      {
        return MessageResult.OK;
      }

      return host.ProcessMessage(messageType, messageRecord, buttons, icon, defaultButton);
    }

    public void Shutdown()
    {
      host?.EnableExit();
      appThread?.Join();
    }

    public void Dispose()
    {
      cancellation?.Dispose();
      installStartEvent?.Dispose();
      installExitEvent?.Dispose();
    }

    private void RunHost()
    {
      try
      {
        host.Run(context, installStartEvent);
      }
      finally
      {
        installExitEvent.Set();
      }
    }

    private static MaintenanceLaunchAction PrepareMaintenanceLaunch(
      Session session,
      IInstallerSession installerSession)
    {
      if (session == null)
      {
        return MaintenanceLaunchAction.None;
      }

      if (string.Equals(session["REMOVE"], "All", StringComparison.OrdinalIgnoreCase))
      {
        session["REMOVE"] = string.Empty;
        return MaintenanceLaunchAction.Uninstall;
      }

      if (installerSession.IsMaintenance)
      {
        return MaintenanceLaunchAction.Change;
      }

      return MaintenanceLaunchAction.None;
    }

    private static void ApplySelectedOperation(Session session, InstallOperation operation)
    {
      if (session == null)
      {
        return;
      }

      session["REINSTALL"] = string.Empty;
      session["REMOVE"] = string.Empty;

      switch (operation)
      {
        case InstallOperation.Repair:
          session["REINSTALL"] = "ALL";
          session["REINSTALLMODE"] = "ecmus";
          break;

        case InstallOperation.Modify:
          break;

        case InstallOperation.Uninstall:
          session["REMOVE"] = "ALL";
          break;
      }
    }
  }
}