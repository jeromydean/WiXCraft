using System;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  internal sealed class InstallerUiContext : IInstallerUiContext
  {
    private readonly InstallProgressCounter progressCounter = new InstallProgressCounter(0.5);
    private bool cancelRequested;

    public InstallerUiContext(IInstallerSession session, string resourcePath)
    {
      Session = session;
      ResourcePath = resourcePath;
      IsMaintenance = session.IsMaintenance;
      SelectedOperation = InstallOperation.Install;
    }

    public IInstallerSession Session { get; }

    public string ResourcePath { get; }

    public bool IsMaintenance { get; }

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

      var args = new InstallMessageEventArgs(
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
  }
}