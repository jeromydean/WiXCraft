namespace WiXCraft
{
  public interface IInstallerMessageDialogHandler
  {
    MessageDialogResult ShowDialog(InstallMessageEventArgs message);
  }
}
