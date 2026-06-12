using System;
using System.Windows;
using System.Windows.Media.Animation;
using ExampleInterface.ViewModels;
using ExampleInterface.Windows;
using MahApps.Metro.Controls;
using WixToolset.Dtf.WindowsInstaller;

namespace ExampleInterface.Views
{
  public partial class SetupWizardView : MetroWindow
  {
    private readonly SetupWizardViewModel viewModel;

    public SetupWizardView(SetupWizardViewModel viewModel)
    {
      this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
      Opacity = 0;
      InitializeComponent();
      DataContext = viewModel;
      Windows11WindowHelper.TryApply(this);
      ContentRendered += SetupWizardView_InitialContentRendered;
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

    private void SetupWizardView_InitialContentRendered(object sender, EventArgs e)
    {
      ContentRendered -= SetupWizardView_InitialContentRendered;

      DoubleAnimation fadeIn = new DoubleAnimation
      {
        From = 0,
        To = 1,
        Duration = TimeSpan.FromMilliseconds(650),
        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
      };

      BeginAnimation(OpacityProperty, fadeIn);
    }
  }
}
