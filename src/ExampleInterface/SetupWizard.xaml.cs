using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using WiXCraft;
using WixToolset.Dtf.WindowsInstaller;

namespace ExampleInterface
{
  public partial class SetupWizard : Window
  {
    private readonly IInstallerUiContext context;
    private readonly ManualResetEvent installStartEvent;
    private readonly Dictionary<CheckBox, InstallerFeatureInfo> featureCheckBoxes =
      new Dictionary<CheckBox, InstallerFeatureInfo>();
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
      BuildFeaturePanel();
      ConfigureMaintenanceUi();
    }

    private void BuildFeaturePanel()
    {
      featurePanel.Children.Clear();
      featureCheckBoxes.Clear();

      foreach (InstallerFeatureInfo feature in context.Features)
      {
        CheckBox checkBox = new CheckBox
        {
          Content = string.IsNullOrWhiteSpace(feature.Description)
            ? feature.Title
            : string.Concat(feature.Title, " - ", feature.Description),
          IsChecked = feature.IsRequestedForInstall,
          IsEnabled = feature.CanChangeSelection,
          Margin = new Thickness(0, 0, 0, 6)
        };

        featureCheckBoxes[checkBox] = feature;
        featurePanel.Children.Add(checkBox);
      }
    }

    private void ConfigureMaintenanceUi()
    {
      if (context.MaintenanceLaunchAction == MaintenanceLaunchAction.Uninstall)
      {
        headerTextBlock.Text = string.Concat("Remove ", context.Session["ProductName"], "?");
        descriptionTextBlock.Text =
          "This will remove the application from your computer. You can also open Change to repair or modify the installation.";
        freshInstallActionsPanel.Visibility = Visibility.Collapsed;
        maintenanceActionsPanel.Visibility = Visibility.Collapsed;
        modifyActionsPanel.Visibility = Visibility.Collapsed;
        uninstallConfirmPanel.Visibility = Visibility.Visible;
        return;
      }

      if (context.IsMaintenance)
      {
        headerTextBlock.Text = string.Concat("Change ", context.Session["ProductName"]);
        descriptionTextBlock.Text =
          "Choose whether you want to repair, modify, or remove this installation.";
        freshInstallActionsPanel.Visibility = Visibility.Collapsed;
        maintenanceActionsPanel.Visibility = Visibility.Visible;
        return;
      }

      headerTextBlock.Text = string.Concat(
        "Install ",
        context.Session["ProductName"],
        " ",
        context.Session["ProductVersion"]);
      descriptionTextBlock.Text = "Click Install to begin setup.";
      maintenanceActionsPanel.Visibility = Visibility.Collapsed;
      modifyActionsPanel.Visibility = Visibility.Collapsed;
      uninstallConfirmPanel.Visibility = Visibility.Collapsed;
    }

    private void EnterModifyMode()
    {
      headerTextBlock.Text = string.Concat("Modify ", context.Session["ProductName"]);
      descriptionTextBlock.Text = "Select the features you want installed, then click Apply changes.";
      maintenanceActionsPanel.Visibility = Visibility.Collapsed;
      uninstallConfirmPanel.Visibility = Visibility.Collapsed;
      modifyActionsPanel.Visibility = Visibility.Visible;
    }

    private void ExitModifyMode()
    {
      ConfigureMaintenanceUi();
    }

    private void ApplyFeatureSelections()
    {
      foreach (KeyValuePair<CheckBox, InstallerFeatureInfo> entry in featureCheckBoxes)
      {
        entry.Value.SetRequestedForInstall(entry.Key.IsChecked == true);
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
      ApplyFeatureSelections();
      context.SelectedOperation = InstallOperation.Install;
      StartInstall();
    }

    private void repairButton_Click(object sender, RoutedEventArgs e)
    {
      context.SelectedOperation = InstallOperation.Repair;
      StartInstall();
    }

    private void modifyButton_Click(object sender, RoutedEventArgs e)
    {
      EnterModifyMode();
    }

    private void applyModifyButton_Click(object sender, RoutedEventArgs e)
    {
      ApplyFeatureSelections();
      context.SelectedOperation = InstallOperation.Modify;
      StartInstall();
    }

    private void removeButton_Click(object sender, RoutedEventArgs e)
    {
      context.SelectedOperation = InstallOperation.Uninstall;
      StartInstall();
    }

    private void backButton_Click(object sender, RoutedEventArgs e)
    {
      ExitModifyMode();
    }

    private void StartInstall()
    {
      freshInstallActionsPanel.Visibility = Visibility.Collapsed;
      maintenanceActionsPanel.Visibility = Visibility.Collapsed;
      modifyActionsPanel.Visibility = Visibility.Collapsed;
      uninstallConfirmPanel.Visibility = Visibility.Collapsed;
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