using System;
using System.Windows;
using ExampleInterface.ViewModels;
using WixToolset.Dtf.WindowsInstaller;

namespace ExampleInterface.Views
{
  public partial class SetupWizardView : Window
  {
    private readonly SetupWizardViewModel viewModel;

    public SetupWizardView(SetupWizardViewModel viewModel)
    {
      this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
      InitializeComponent();
      DataContext = viewModel;
    }

    internal SetupWizardViewModel ViewModel => viewModel;

    public MessageResult ProcessMessage(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton)
    {
      return viewModel.ProcessMessage(messageType, messageRecord, buttons, icon, defaultButton);
    }

    internal void EnableExit()
    {
      viewModel.EnableExit();
    }
  }
}