using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ExampleInterface.ViewModels
{
  public partial class WebsiteConfigurationViewModel : ObservableObject
  {
    public WebsiteConfigurationViewModel(string productName)
    {
      string safeName = string.IsNullOrWhiteSpace(productName) ? "ExampleApp" : productName.Replace(" ", string.Empty);

      SiteName = safeName;
      ApplicationPoolName = string.Concat(safeName, "Pool");
      PhysicalPath = string.Concat(@"C:\inetpub\wwwroot\", safeName);

      IpAddresses = new ObservableCollection<string>
      {
        "* (All Unassigned)",
        "127.0.0.1",
        "::1",
      };

      SslCertificates = new ObservableCollection<string>
      {
        "(No certificate)",
        "localhost",
        "My Development Certificate",
      };

      PipelineModes = new ObservableCollection<string>
      {
        "Integrated",
        "Classic",
      };

      ClrVersions = new ObservableCollection<string>
      {
        "No Managed Code",
        ".NET CLR Version v4.0.30319",
      };

      SelectedIpAddress = IpAddresses[0];
      SelectedSslCertificate = SslCertificates[0];
      SelectedPipelineMode = PipelineModes[0];
      SelectedClrVersion = ClrVersions[1];
    }

    public ObservableCollection<string> IpAddresses { get; }

    public ObservableCollection<string> SslCertificates { get; }

    public ObservableCollection<string> PipelineModes { get; }

    public ObservableCollection<string> ClrVersions { get; }

    [ObservableProperty]
    private string siteName;

    [ObservableProperty]
    private string physicalPath;

    [ObservableProperty]
    private string selectedIpAddress;

    [ObservableProperty]
    private string port = "80";

    [ObservableProperty]
    private string hostHeader = string.Empty;

    [ObservableProperty]
    private bool enableHttps;

    [ObservableProperty]
    private bool requireHttps;

    [ObservableProperty]
    private bool redirectHttpToHttps;

    [ObservableProperty]
    private string selectedSslCertificate;

    [ObservableProperty]
    private bool enableAnonymousAuthentication = true;

    [ObservableProperty]
    private bool enableWindowsAuthentication;

    [ObservableProperty]
    private string applicationPoolName;

    [ObservableProperty]
    private string selectedPipelineMode;

    [ObservableProperty]
    private string selectedClrVersion;

    [ObservableProperty]
    private string statusMessage = "Configure the IIS site, bindings, and authentication for the web application.";

    partial void OnEnableHttpsChanged(bool value)
    {
      if (value && string.Equals(Port, "80", System.StringComparison.Ordinal))
      {
        Port = "443";
      }
      else if (!value && string.Equals(Port, "443", System.StringComparison.Ordinal))
      {
        Port = "80";
      }
    }

    [RelayCommand]
    private void BrowsePhysicalPath()
    {
      StatusMessage = "Demo mode: folder browse is not implemented.";
    }

    [RelayCommand]
    private void TestBinding()
    {
      StatusMessage = "Demo mode: binding validation is not implemented.";
    }
  }
}
