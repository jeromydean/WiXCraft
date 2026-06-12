using System;
using System.Collections.Generic;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  internal sealed class InstallerUiContext : IInstallerUiContext
  {
    private readonly InstallProgressCounter progressCounter = new InstallProgressCounter(0.5);
    private bool cancelRequested;

    public InstallerUiContext(
      IInstallerSession session,
      string resourcePath,
      MaintenanceLaunchAction maintenanceLaunchAction,
      InstallerUiModeOptions modeOptions)
    {
      Session = session;
      ResourcePath = resourcePath;
      MaintenanceLaunchAction = maintenanceLaunchAction;
      ModeOptions = modeOptions ?? InstallerUiModeOptions.CreateDefault();
      IsMaintenance =
        session.IsMaintenance ||
        maintenanceLaunchAction == MaintenanceLaunchAction.Change ||
        maintenanceLaunchAction == MaintenanceLaunchAction.Uninstall;
      SelectedOperation = ResolveInitialOperation(maintenanceLaunchAction, session.IsMaintenance);
    }

    public IInstallerSession Session { get; }

    public string ResourcePath { get; }

    public bool IsMaintenance { get; }

    public MaintenanceLaunchAction MaintenanceLaunchAction { get; }

    public InstallerUiModeOptions ModeOptions { get; }

    private IReadOnlyList<InstallerFeatureInfo> features;

    public IReadOnlyList<InstallerFeatureInfo> Features =>
      features ?? (features = Session.GetFeatures());

    public InstallOperation SelectedOperation { get; set; }

    public double Progress => progressCounter.Progress;

    public bool IsCancelRequested => cancelRequested;

    public event EventHandler<InstallMessageEventArgs> InstallMessageReceived;

    public event EventHandler CancelRequested;

    public MessageResult HandleInstallMessage(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton)
    {
      progressCounter.ProcessMessage(messageType, messageRecord);

      InstallMessageEventArgs args = new InstallMessageEventArgs(
        messageType,
        messageRecord,
        buttons,
        icon,
        defaultButton);

      InstallMessageReceived?.Invoke(this, args);

      if (cancelRequested)
      {
        cancelRequested = false;
        return MessageResult.Cancel;
      }

      return args.Result;
    }

    public void RequestCancel()
    {
      cancelRequested = true;
      CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    private static InstallOperation ResolveInitialOperation(
      MaintenanceLaunchAction maintenanceLaunchAction,
      bool isInstalled)
    {
      if (maintenanceLaunchAction == MaintenanceLaunchAction.Uninstall)
      {
        return InstallOperation.Uninstall;
      }

      if (isInstalled || maintenanceLaunchAction == MaintenanceLaunchAction.Change)
      {
        return InstallOperation.Repair;
      }

      return InstallOperation.Install;
    }
  }
}