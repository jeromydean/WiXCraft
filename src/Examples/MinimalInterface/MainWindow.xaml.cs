using System.Threading;
using System.Windows;
using WiXCraft;

namespace MinimalInterface
{
  public partial class MainWindow : Window
  {
    private readonly IInstallerUiContext context;
    private readonly ManualResetEvent installStartEvent;
    private bool installStarted;

    public MainWindow(IInstallerUiContext context, ManualResetEvent installStartEvent)
    {
      this.context = context;
      this.installStartEvent = installStartEvent;
      InitializeComponent();
      Title = string.Concat(context.Session["ProductName"] ?? "Setup", " Setup");
      StatusText.Text = "Click Install to begin.";
    }

    internal void UpdateProgress(double progress)
    {
      ProgressBar.Value = progress * 100;
    }

    internal void ShowComplete()
    {
      installStarted = false;
      ProgressBar.Visibility = Visibility.Collapsed;
      InstallButton.Visibility = Visibility.Collapsed;
      CloseButton.Content = "Close";
      CloseButton.IsEnabled = true;
      StatusText.Text = "Setup finished.";
    }

    private void InstallButton_Click(object sender, RoutedEventArgs e)
    {
      context.SelectedOperation = InstallOperation.Install;
      InstallButton.IsEnabled = false;
      ProgressBar.Visibility = Visibility.Visible;
      StatusText.Text = "Installing...";
      installStarted = true;
      installStartEvent.Set();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      if (installStarted)
      {
        context.RequestCancel();
        CloseButton.IsEnabled = false;
        return;
      }

      Close();
    }
  }
}
