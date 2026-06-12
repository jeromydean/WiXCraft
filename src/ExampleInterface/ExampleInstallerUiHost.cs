using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WiXCraft;
using WixToolset.Dtf.WindowsInstaller;

namespace ExampleInterface
{
  public sealed class ExampleInstallerUiHost : IInstallerUiHost
  {
    private SetupWizard wizard;

    public void Run(IInstallerUiContext context, ManualResetEvent installStartEvent)
    {
      var app = new Application();
      wizard = new SetupWizard(context, installStartEvent);
      wizard.InitializeComponent();
      app.Run(wizard);
    }

    public MessageResult ProcessMessage(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton)
    {
      if (wizard?.Dispatcher == null)
      {
        return MessageResult.OK;
      }

      if (wizard.Dispatcher.CheckAccess())
      {
        return wizard.ProcessMessage(messageType, messageRecord, buttons, icon, defaultButton);
      }

      return (MessageResult)wizard.Dispatcher.Invoke(
        DispatcherPriority.Send,
        new ProcessMessageDelegate(ProcessMessage),
        messageType,
        messageRecord,
        buttons,
        icon,
        defaultButton);
    }

    public void EnableExit()
    {
      if (wizard?.Dispatcher == null)
      {
        return;
      }

      wizard.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new System.Action(wizard.EnableExit));
    }

    private delegate MessageResult ProcessMessageDelegate(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton);
  }
}
