using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WiXCraft;
using WixToolset.Dtf.WindowsInstaller;

namespace MinimalInterface
{
  public sealed class MinimalInstallerUiHost : IInstallerUiHost
  {
    private IInstallerUiContext context;
    private MainWindow window;

    public void Run(IInstallerUiContext context, ManualResetEvent installStartEvent)
    {
      this.context = context;
      Application app = new Application();
      window = new MainWindow(context, installStartEvent);
      app.Run(window);
    }

    public MessageResult ProcessMessage(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton)
    {
      if (context == null)
      {
        return MessageResult.OK;
      }

      MessageResult result = context.HandleInstallMessage(
        messageType,
        messageRecord,
        buttons,
        icon,
        defaultButton);

      if (window != null)
      {
        double progress = context.Progress;
        window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => window.UpdateProgress(progress)));
      }

      return result;
    }

    public void EnableExit()
    {
      window?.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(window.ShowComplete));
    }
  }
}
