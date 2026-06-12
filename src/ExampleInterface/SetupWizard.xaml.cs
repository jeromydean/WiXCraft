using System;
using System.Threading;
using System.Windows;
using WiXCraft;
using WixToolset.Dtf.WindowsInstaller;

namespace ExampleInterface
{
  public partial class SetupWizard : Window
  {
    private readonly IInstallerUiContext context;
    private readonly ManualResetEvent installStartEvent;
    private bool installStarted;

    public SetupWizard(IInstallerUiContext context, ManualResetEvent installStartEvent)
    {
      this.context = context;
      this.installStartEvent = installStartEvent;
      Loaded += SetupWizard_Loaded;
    }

    private void SetupWizard_Loaded(object sender, RoutedEventArgs e)
    {
      Loaded -= SetupWizard_Loaded;

      sessionPropertiesGrid.ItemsSource = context.Session.GetProperties();

      if (context.IsMaintenance)
      {
        installButton.Visibility = Visibility.Collapsed;
        repairButton.Visibility = Visibility.Visible;
        uninstallButton.Visibility = Visibility.Visible;
      }
    }

    public MessageResult ProcessMessage(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton)
    {
      try
      {
        MessageResult result = context.HandleInstallMessage(
          messageType,
          messageRecord,
          buttons,
          icon,
          defaultButton);

        progressBar.Value = progressBar.Minimum +
          context.Progress * (progressBar.Maximum - progressBar.Minimum);
        progressLabel.Content = string.Concat((int)Math.Round(100 * context.Progress), "%");

        switch (messageType)
        {
          case InstallMessage.Error:
          case InstallMessage.Warning:
          case InstallMessage.Info:
            LogMessage(string.Concat(messageType, ": ", messageRecord));
            break;
        }

        return result;
      }
      catch (Exception ex)
      {
        LogMessage(ex.ToString());
        return MessageResult.OK;
      }
    }

    internal void EnableExit()
    {
      progressBar.Visibility = Visibility.Collapsed;
      progressLabel.Visibility = Visibility.Collapsed;
      cancelButton.Visibility = Visibility.Collapsed;
      exitButton.Visibility = Visibility.Visible;
    }

    private void LogMessage(string message)
    {
      messagesTextBox.Text += Environment.NewLine + message;
      messagesTextBox.ScrollToEnd();
    }

    private void installButton_Click(object sender, RoutedEventArgs e)
    {
      context.SelectedOperation = InstallOperation.Install;
      StartInstall();
    }

    private void repairButton_Click(object sender, RoutedEventArgs e)
    {
      context.SelectedOperation = InstallOperation.Repair;
      StartInstall();
    }

    private void uninstallButton_Click(object sender, RoutedEventArgs e)
    {
      context.SelectedOperation = InstallOperation.Uninstall;
      StartInstall();
    }

    private void StartInstall()
    {
      installButton.Visibility = Visibility.Collapsed;
      repairButton.Visibility = Visibility.Collapsed;
      uninstallButton.Visibility = Visibility.Collapsed;
      progressBar.Visibility = Visibility.Visible;
      progressLabel.Visibility = Visibility.Visible;
      installStarted = true;
      installStartEvent.Set();
    }

    private void exitButton_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void cancelButton_Click(object sender, RoutedEventArgs e)
    {
      if (!installStarted)
      {
        Close();
        return;
      }

      context.RequestCancel();
      cancelButton.IsEnabled = false;
    }
  }
}
