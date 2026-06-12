using System;
using System.Collections.Generic;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  public interface IInstallerUiContext
  {
    IInstallerSession Session { get; }

    string ResourcePath { get; }

    bool IsMaintenance { get; }

    MaintenanceLaunchAction MaintenanceLaunchAction { get; }

    InstallerUiModeOptions ModeOptions { get; }

    IReadOnlyList<InstallerFeatureInfo> Features { get; }

    InstallOperation SelectedOperation { get; set; }

    double Progress { get; }

    bool IsCancelRequested { get; }

    event EventHandler<InstallMessageEventArgs> InstallMessageReceived;

    MessageResult HandleInstallMessage(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton);

    void RequestCancel();

    event EventHandler CancelRequested;

    event EventHandler Initializing;

    event EventHandler<InstallerInstallStartingEventArgs> InstallStarting;

    event EventHandler<InstallerInstallCompletedEventArgs> InstallCompleted;

    void RaiseInitializing();

    bool RaiseInstallStarting(InstallOperation operation);

    void RaiseInstallCompleted(bool succeeded);

    InstallerExecuteSequenceObserver ExecuteSequence { get; }

    InstallerPropertyBag InstallProperties { get; }

    int SetInstallProperties(IEnumerable<KeyValuePair<string, string>> properties);

    InstallerSequenceHookRegistry SequenceHooks { get; }

    IInstallerSequenceHookAsyncInvoker SequenceHookAsyncInvoker { get; set; }

    IInstallerMessageDialogHandler MessageDialogHandler { get; set; }
  }
}