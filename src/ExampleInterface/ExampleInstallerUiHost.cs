using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ExampleInterface.DependencyInjection;
using ExampleInterface.Views;
using Microsoft.Extensions.DependencyInjection;
using WiXCraft;
using WixToolset.Dtf.WindowsInstaller;

namespace ExampleInterface
{
  public sealed class ExampleInstallerUiHost : IInstallerUiHost
  {
    private readonly Action<IServiceCollection> configureServices;
    private SetupWizardView wizardView;
    private ServiceProvider serviceProvider;

    public ExampleInstallerUiHost()
      : this(services => ExampleInterfaceServiceCollectionExtensions.AddExampleInterfaceUi(services))
    {
    }

    public ExampleInstallerUiHost(Action<IServiceCollection> configureServices)
    {
      this.configureServices = configureServices ??
        throw new ArgumentNullException(nameof(configureServices));
    }

    public void Run(IInstallerUiContext context, ManualResetEvent installStartEvent)
    {
      ServiceCollection services = new ServiceCollection();
      configureServices(services);
      services.AddSingleton(context);
      services.AddSingleton(installStartEvent);

      serviceProvider = services.BuildServiceProvider();

      Application app = InstallerApplication.Create();
      wizardView = serviceProvider.GetRequiredService<SetupWizardView>();
      context.MessageDialogHandler = new Dialogs.WpfInstallerMessageDialogHandler(() => wizardView);
      context.RaiseInitializing();
      wizardView.ViewModel.CloseAction = wizardView.Close;

      try
      {
        app.Run(wizardView);
      }
      finally
      {
        serviceProvider.Dispose();
        serviceProvider = null;
      }
    }

    public MessageResult ProcessMessage(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton)
    {
      if (wizardView?.Dispatcher == null)
      {
        return MessageResult.OK;
      }

      if (wizardView.Dispatcher.CheckAccess())
      {
        return wizardView.ProcessMessage(messageType, messageRecord, buttons, icon, defaultButton);
      }

      return (MessageResult)wizardView.Dispatcher.Invoke(
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
      if (wizardView?.Dispatcher == null)
      {
        return;
      }

      wizardView.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new System.Action(wizardView.EnableExit));
    }

    private delegate MessageResult ProcessMessageDelegate(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton);
  }
}