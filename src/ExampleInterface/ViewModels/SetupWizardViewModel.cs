using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExampleInterface.Windows;
using MahApps.Metro.IconPacks;
using WiXCraft;
using WixToolset.Dtf.WindowsInstaller;

namespace ExampleInterface.ViewModels
{
  public partial class SetupWizardViewModel : ObservableObject
  {
    private readonly IInstallerUiContext context;
    private readonly ManualResetEvent installStartEvent;
    private bool installStarted;
    private bool installFailed;

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
      DatabaseConnection = new DatabaseConnectionViewModel();
      WebsiteConfiguration = new WebsiteConfigurationViewModel(ProductName);
      NavigationItems = new ObservableCollection<NavigationItemViewModel>
      {
        new NavigationItemViewModel(InstallerNavigationPage.License, "License", PackIconMaterialKind.FileDocumentOutline),
        new NavigationItemViewModel(InstallerNavigationPage.Features, "Features", PackIconMaterialKind.ViewList),
        new NavigationItemViewModel(InstallerNavigationPage.Database, "Database", PackIconMaterialKind.Database),
        new NavigationItemViewModel(InstallerNavigationPage.Website, "Website", PackIconMaterialKind.Web),
        new NavigationItemViewModel(InstallerNavigationPage.Diagnostics, "Diagnostics", PackIconMaterialKind.InformationOutline),
      };

      SelectedNavigationItem = NavigationItems[0];
      SelectedPage = InstallerNavigationPage.License;

      UpdateElevationState();
      ConfigureUi();
    }

    public Action CloseAction { get; set; }

    public bool ShowElevateButton => !IsElevated && ShowCancelButton;

    public string ProductName { get; }

    public string ProductVersion { get; }

    public string Manufacturer { get; }

    public string WindowTitle { get; }

    public bool ShowPrimaryContent => !ShowProgress && !ShowFinishState;

    public bool ShowConfigurationNavigation => ShowPrimaryContent && ShowFreshInstallActions;

    public bool ShowModifyContent => ShowPrimaryContent && ShowModifyActions;

    public bool ShowMaintenanceContent =>
      ShowPrimaryContent && (ShowMaintenanceActions || ShowUninstallConfirm);

    public DatabaseConnectionViewModel DatabaseConnection { get; }

    public WebsiteConfigurationViewModel WebsiteConfiguration { get; }

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

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
    private string currentActionText = "Preparing installation...";

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
    [NotifyPropertyChangedFor(nameof(ShowPrimaryContent))]
    private bool showFinishState;

    [ObservableProperty]
    private bool installSucceeded = true;

    [ObservableProperty]
    private string finishTitle = string.Empty;

    [ObservableProperty]
    private string finishMessage = string.Empty;

    [ObservableProperty]
    private bool showExitButton;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowElevateButton))]
    private bool showCancelButton = true;

    [ObservableProperty]
    private bool isCancelEnabled = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowElevateButton))]
    [NotifyCanExecuteChangedFor(nameof(ElevateCommand))]
    private bool isElevated;

    [ObservableProperty]
    private string elevationStatusText;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowConfigurationNavigation))]
    [NotifyPropertyChangedFor(nameof(ShowModifyContent))]
    [NotifyPropertyChangedFor(nameof(ShowMaintenanceContent))]
    private InstallerNavigationPage selectedPage;

    [ObservableProperty]
    private NavigationItemViewModel selectedNavigationItem;

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
        UpdateCurrentAction(messageType, messageRecord);

        switch (messageType)
        {
          case InstallMessage.FatalExit:
            installFailed = true;
            break;

          case InstallMessage.Error:
            installFailed = true;
            AppendMessage(string.Concat(messageType, ": ", messageRecord));
            break;

          case InstallMessage.Warning:
          case InstallMessage.Info:
            AppendMessage(string.Concat(messageType, ": ", messageRecord));
            break;
        }

        return result;
      }
      catch (Exception ex)
      {
        installFailed = true;
        AppendMessage(ex.ToString());
        return MessageResult.OK;
      }
    }

    public void EnableExit()
    {
      ShowProgress = false;
      ShowFinishState = true;
      InstallSucceeded = !installFailed;
      FinishTitle = InstallSucceeded ? "Setup completed successfully" : "Setup did not complete";
      FinishMessage = BuildFinishMessage();
      HeaderText = FinishTitle;
      DescriptionText = FinishMessage;
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

    partial void OnSelectedNavigationItemChanged(NavigationItemViewModel value)
    {
      if (value != null && value.Page != SelectedPage)
      {
        SelectedPage = value.Page;
      }
    }

    partial void OnSelectedPageChanged(InstallerNavigationPage value)
    {
      SyncNavigationSelection();
      UpdatePageDescription();
    }

    partial void OnShowFreshInstallActionsChanged(bool value)
    {
      OnPropertyChanged(nameof(ShowConfigurationNavigation));
    }

    partial void OnShowModifyActionsChanged(bool value)
    {
      OnPropertyChanged(nameof(ShowModifyContent));
    }

    partial void OnShowMaintenanceActionsChanged(bool value)
    {
      OnPropertyChanged(nameof(ShowMaintenanceContent));
    }

    partial void OnShowUninstallConfirmChanged(bool value)
    {
      OnPropertyChanged(nameof(ShowMaintenanceContent));
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

    [RelayCommand(CanExecute = nameof(CanElevate))]
    private void Elevate()
    {
      string msiPath = context.Session["OriginalDatabase"];
      if (!InstallerElevationHelper.TryRestartElevated(msiPath, out string errorMessage))
      {
        AppendMessage(errorMessage ?? "Could not restart the installer with elevation.");
        return;
      }

      CloseAction?.Invoke();
    }

    private bool CanElevate()
    {
      return !IsElevated && !installStarted;
    }

    private void UpdateElevationState()
    {
      IsElevated = InstallerElevationHelper.IsElevated(context.Session);
      ElevationStatusText = IsElevated
        ? "Running as administrator"
        : "Restart required for administrator privileges";
    }

    private void ConfigureUi()
    {
      ShowModifyActions = false;
      ShowFinishState = false;

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
      DescriptionText = "Review the license and configuration pages, then click Install to begin setup.";
      ShowFreshInstallActions = true;
      ShowMaintenanceActions = false;
      ShowUninstallConfirm = false;
      SelectedPage = InstallerNavigationPage.License;
      SyncNavigationSelection();
      UpdatePageDescription();
    }

    private void SyncNavigationSelection()
    {
      foreach (NavigationItemViewModel item in NavigationItems)
      {
        item.IsSelected = item.Page == SelectedPage;
      }

      NavigationItemViewModel selected = NavigationItems.FirstOrDefault(item => item.Page == SelectedPage);
      if (selected != null && SelectedNavigationItem?.Page != SelectedPage)
      {
        SelectedNavigationItem = selected;
      }
    }

    private void UpdatePageDescription()
    {
      if (!ShowFreshInstallActions)
      {
        return;
      }

      switch (SelectedPage)
      {
        case InstallerNavigationPage.License:
          DescriptionText = "Please read the following license agreement.";
          break;

        case InstallerNavigationPage.Database:
          DescriptionText = "Configure the SQL Server database connection for the application.";
          break;

        case InstallerNavigationPage.Website:
          DescriptionText = "Configure IIS site bindings, SSL, and authentication for the web application.";
          break;

        case InstallerNavigationPage.Diagnostics:
          DescriptionText = "Review MSI session properties and installer messages.";
          break;

        default:
          DescriptionText = "Choose the components you want to install.";
          break;
      }
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
      ShowFinishState = false;
      installFailed = false;
      CurrentActionText = "Starting installation...";
      ProgressValue = 0;
      ProgressText = "0%";
      ShowProgress = true;
      installStarted = true;
      installStartEvent.Set();
    }

    private void UpdateCurrentAction(InstallMessage messageType, Record messageRecord)
    {
      if (messageRecord == null)
      {
        return;
      }

      switch (messageType)
      {
        case InstallMessage.ActionStart:
          if (messageRecord.FieldCount >= 2)
          {
            string description = messageRecord.GetString(2);
            if (!string.IsNullOrWhiteSpace(description))
            {
              CurrentActionText = description;
              return;
            }
          }

          if (messageRecord.FieldCount >= 1)
          {
            string actionName = messageRecord.GetString(1);
            if (!string.IsNullOrWhiteSpace(actionName))
            {
              CurrentActionText = actionName;
            }
          }

          break;

        case InstallMessage.ActionData:
          if (messageRecord.FieldCount >= 1)
          {
            string actionData = messageRecord.GetString(1);
            if (!string.IsNullOrWhiteSpace(actionData))
            {
              CurrentActionText = actionData;
            }
          }

          break;
      }
    }

    private string BuildFinishMessage()
    {
      if (!InstallSucceeded)
      {
        return "An error occurred during setup. Review the details log for more information.";
      }

      switch (context.SelectedOperation)
      {
        case InstallOperation.Repair:
          return string.Concat(ProductName, " has been repaired and is ready to use.");

        case InstallOperation.Modify:
          return string.Concat(ProductName, " has been updated with your selected changes.");

        case InstallOperation.Uninstall:
          return string.Concat(ProductName, " has been removed from this computer.");

        default:
          return string.Concat(ProductName, " is now installed and ready to use.");
      }
    }

    private void AppendMessage(string message)
    {
      MessagesText += Environment.NewLine + message;
    }
  }
}
