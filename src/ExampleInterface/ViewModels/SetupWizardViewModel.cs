using System;
using System.Collections.ObjectModel;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WiXCraft;
using WixToolset.Dtf.WindowsInstaller;

namespace ExampleInterface.ViewModels
{
  public partial class SetupWizardViewModel : ObservableObject
  {
    private readonly IInstallerUiContext context;
    private readonly ManualResetEvent installStartEvent;
    private bool installStarted;

    public SetupWizardViewModel(IInstallerUiContext context, ManualResetEvent installStartEvent)
    {
      this.context = context ?? throw new ArgumentNullException(nameof(context));
      this.installStartEvent = installStartEvent ?? throw new ArgumentNullException(nameof(installStartEvent));

      SessionProperties = new ObservableCollection<InstallerSessionProperty>(context.Session.GetProperties());

      Features = new ObservableCollection<FeatureItemViewModel>();
      foreach (InstallerFeatureInfo feature in context.Features)
      {
        Features.Add(new FeatureItemViewModel(feature));
      }

      ProductName = context.Session["ProductName"] ?? "Setup";
      ProductVersion = context.Session["ProductVersion"] ?? string.Empty;
      Manufacturer = context.Session["Manufacturer"] ?? string.Empty;
      WindowTitle = string.Concat(ProductName, " Setup");

      ConfigureUi();
    }

    public Action CloseAction { get; set; }

    public string ProductName { get; }

    public string ProductVersion { get; }

    public string Manufacturer { get; }

    public string WindowTitle { get; }

    public bool ShowPrimaryContent => !ShowProgress;

    public ObservableCollection<InstallerSessionProperty> SessionProperties { get; }

    public ObservableCollection<FeatureItemViewModel> Features { get; }

    [ObservableProperty]
    private string headerText;

    [ObservableProperty]
    private string descriptionText;

    [ObservableProperty]
    private string messagesText = string.Empty;

    [ObservableProperty]
    private double progressValue;

    [ObservableProperty]
    private string progressText = "0%";

    [ObservableProperty]
    private bool showFreshInstallActions;

    [ObservableProperty]
    private bool showMaintenanceActions;

    [ObservableProperty]
    private bool showModifyActions;

    [ObservableProperty]
    private bool showUninstallConfirm;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowPrimaryContent))]
    private bool showProgress;

    [ObservableProperty]
    private bool showExitButton;

    [ObservableProperty]
    private bool showCancelButton = true;

    [ObservableProperty]
    private bool isCancelEnabled = true;

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

        ProgressValue = context.Progress * 100;
        ProgressText = string.Concat((int)Math.Round(ProgressValue), "%");

        switch (messageType)
        {
          case InstallMessage.Error:
          case InstallMessage.Warning:
          case InstallMessage.Info:
            AppendMessage(string.Concat(messageType, ": ", messageRecord));
            break;
        }

        return result;
      }
      catch (Exception ex)
      {
        AppendMessage(ex.ToString());
        return MessageResult.OK;
      }
    }

    public void EnableExit()
    {
      ShowProgress = false;
      ShowCancelButton = false;
      ShowExitButton = true;
    }

    [RelayCommand]
    private void Install()
    {
      ApplyFeatureSelections();
      context.SelectedOperation = InstallOperation.Install;
      StartInstall();
    }

    [RelayCommand]
    private void Repair()
    {
      context.SelectedOperation = InstallOperation.Repair;
      StartInstall();
    }

    [RelayCommand]
    private void EnterModify()
    {
      HeaderText = string.Concat("Modify ", context.Session["ProductName"]);
      DescriptionText = "Select the features you want installed, then click Apply changes.";
      ShowMaintenanceActions = false;
      ShowUninstallConfirm = false;
      ShowModifyActions = true;
    }

    [RelayCommand]
    private void ApplyModify()
    {
      ApplyFeatureSelections();
      context.SelectedOperation = InstallOperation.Modify;
      StartInstall();
    }

    [RelayCommand]
    private void Remove()
    {
      context.SelectedOperation = InstallOperation.Uninstall;
      StartInstall();
    }

    [RelayCommand]
    private void Back()
    {
      ConfigureUi();
    }

    [RelayCommand]
    private void Exit()
    {
      CloseAction?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
      if (!installStarted)
      {
        CloseAction?.Invoke();
        return;
      }

      context.RequestCancel();
      IsCancelEnabled = false;
    }

    private void ConfigureUi()
    {
      ShowModifyActions = false;

      if (context.MaintenanceLaunchAction == MaintenanceLaunchAction.Uninstall)
      {
        HeaderText = string.Concat("Remove ", context.Session["ProductName"], "?");
        DescriptionText =
          "This will remove the application from your computer. You can also open Change to repair or modify the installation.";
        ShowFreshInstallActions = false;
        ShowMaintenanceActions = false;
        ShowUninstallConfirm = true;
        return;
      }

      if (context.IsMaintenance)
      {
        HeaderText = string.Concat("Change ", context.Session["ProductName"]);
        DescriptionText =
          "Choose whether you want to repair, modify, or remove this installation.";
        ShowFreshInstallActions = false;
        ShowMaintenanceActions = true;
        ShowUninstallConfirm = false;
        return;
      }

      HeaderText = string.Concat(
        "Install ",
        context.Session["ProductName"],
        " ",
        context.Session["ProductVersion"]);
      DescriptionText = "Click Install to begin setup.";
      ShowFreshInstallActions = true;
      ShowMaintenanceActions = false;
      ShowUninstallConfirm = false;
    }

    private void ApplyFeatureSelections()
    {
      foreach (FeatureItemViewModel feature in Features)
      {
        feature.ApplyToSession();
      }
    }

    private void StartInstall()
    {
      ShowFreshInstallActions = false;
      ShowMaintenanceActions = false;
      ShowModifyActions = false;
      ShowUninstallConfirm = false;
      ShowProgress = true;
      installStarted = true;
      installStartEvent.Set();
    }

    private void AppendMessage(string message)
    {
      MessagesText += Environment.NewLine + message;
    }
  }
}