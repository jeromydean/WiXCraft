using System;
using System.Collections.Generic;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  internal sealed class InstallerUiContext : IInstallerUiContext
  {
    private readonly InstallProgressCounter progressCounter = new InstallProgressCounter(0.5);
    private readonly IInstallerUiLifecycle lifecycle;
    private bool cancelRequested;

    public InstallerUiContext(
      IInstallerSession session,
      string resourcePath,
      MaintenanceLaunchAction maintenanceLaunchAction,
      InstallerUiModeOptions modeOptions,
      IInstallerUiLifecycle lifecycle)
    {
      Session = session;
      ResourcePath = resourcePath;
      MaintenanceLaunchAction = maintenanceLaunchAction;
      ModeOptions = modeOptions ?? InstallerUiModeOptions.CreateDefault();
      this.lifecycle = lifecycle;
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

    public event EventHandler Initializing;

    public event EventHandler<InstallerInstallStartingEventArgs> InstallStarting;

    public event EventHandler<InstallerInstallCompletedEventArgs> InstallCompleted;

    public InstallerExecuteSequenceObserver ExecuteSequence { get; } =
      new InstallerExecuteSequenceObserver();

    public IInstallerMessageDialogHandler MessageDialogHandler { get; set; }

    public void RaiseInitializing()
    {
      lifecycle?.OnInitializing(this);
      Initializing?.Invoke(this, EventArgs.Empty);
    }

    public bool RaiseInstallStarting(InstallOperation operation)
    {
      InstallerInstallStartingEventArgs args = new InstallerInstallStartingEventArgs(operation);
      lifecycle?.OnInstallStarting(this, args);
      InstallStarting?.Invoke(this, args);
      return !args.Cancel;
    }

    public void RaiseInstallCompleted(bool succeeded)
    {
      InstallerInstallCompletedEventArgs args =
        new InstallerInstallCompletedEventArgs(SelectedOperation, succeeded);
      lifecycle?.OnInstallCompleted(this, args);
      InstallCompleted?.Invoke(this, args);
    }

    public MessageResult HandleInstallMessage(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton)
    {
      progressCounter.ProcessMessage(messageType, messageRecord);
      ExecuteSequence.ProcessMessage(messageType, messageRecord);

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

      if (ModeOptions.HandleEngineDialogs &&
          InstallerMessageDialog.IsInteractive(messageType))
      {
        if (MessageDialogHandler != null)
        {
          args.Result = MessageDialogHandler.ShowDialog(args).Result;
        }
        else
        {
          args.Result = InstallerMessageDialog.GetDefaultResult(buttons, defaultButton);
        }
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