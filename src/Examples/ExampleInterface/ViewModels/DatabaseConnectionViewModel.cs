using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ExampleInterface.ViewModels
{
  public partial class DatabaseConnectionViewModel : ObservableObject
  {
    public DatabaseConnectionViewModel()
    {
      Databases = new ObservableCollection<string>
      {
        "master",
        "model",
        "msdb",
        "tempdb",
        "ExampleDb",
      };

      SelectedDatabase = Databases[0];
    }

    public ObservableCollection<string> Databases { get; }

    [ObservableProperty]
    private string serverName = "(local)";

    [ObservableProperty]
    private bool useWindowsAuthentication = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSqlAuthentication))]
    private bool useSqlAuthentication;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string selectedDatabase;

    [ObservableProperty]
    private bool encryptConnection = true;

    [ObservableProperty]
    private bool trustServerCertificate;

    [ObservableProperty]
    private string statusMessage = "Specify connection settings for the application database.";

    public bool IsSqlAuthentication => UseSqlAuthentication;

    partial void OnUseWindowsAuthenticationChanged(bool value)
    {
      if (value)
      {
        UseSqlAuthentication = false;
      }
    }

    partial void OnUseSqlAuthenticationChanged(bool value)
    {
      if (value)
      {
        UseWindowsAuthentication = false;
      }
    }

    [RelayCommand]
    private void TestConnection()
    {
      StatusMessage = "Demo mode: connection test is not implemented.";
    }

    [RelayCommand]
    private void BrowseServers()
    {
      StatusMessage = "Demo mode: server browse is not implemented.";
    }

    [RelayCommand]
    private void RefreshDatabases()
    {
      StatusMessage = "Demo mode: database list refresh is not implemented.";
    }
  }
}
