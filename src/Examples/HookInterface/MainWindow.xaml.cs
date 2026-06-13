using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WiXCraft;

namespace HookInterface
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
      StatusText.Text =
        "Click Install. An async hook runs before file copy; " +
        "a second hook runs before the deferred elevated custom action.";
      context.SequenceHooks.HookInvoked += OnHookInvoked;
    }

    internal void ReportHookStatus(string message)
    {
      StatusText.Text = message;
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

    private void OnHookInvoked(object sender, InstallerSequenceHookContext hookContext)
    {
      string kind = string.Equals(hookContext.HookId, "BeforeInstallFiles", StringComparison.OrdinalIgnoreCase)
        ? "[async] "
        : "[before deferred] ";

      string line = string.Concat(
        kind,
        hookContext.HookId,
        ": ",
        hookContext.Payload,
        hookContext.Cancel ? " (cancelled)" : string.Empty);

      Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => AppendHookLog(line)));
    }

    private void AppendHookLog(string line)
    {
      HookLogText.AppendText(line + Environment.NewLine);
      HookLogText.ScrollToEnd();
    }

    private void InstallButton_Click(object sender, RoutedEventArgs e)
    {
      context.SelectedOperation = InstallOperation.Install;
      HookLogText.Clear();
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
