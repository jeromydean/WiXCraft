using System;
using System.Windows;
using MahApps.Metro.Controls;
using WiXCraft;
using WixToolset.Dtf.WindowsInstaller;
namespace ExampleInterface.Dialogs
{
  public sealed class WpfInstallerMessageDialogHandler : IInstallerMessageDialogHandler
  {
    private readonly Func<Window> getOwnerWindow;

    public WpfInstallerMessageDialogHandler(Func<Window> getOwnerWindow)
    {
      this.getOwnerWindow = getOwnerWindow ??
        throw new ArgumentNullException(nameof(getOwnerWindow));
    }

    public MessageDialogResult ShowDialog(InstallMessageEventArgs message)
    {
      Window owner = getOwnerWindow();
      if (owner is MetroWindow metroWindow)
      {
        return new MessageDialogResult(MahAppsInstallerMessageBox.Show(metroWindow, message));
      }

      IntPtr ownerHandle = IntPtr.Zero;
      if (owner != null)
      {
        if (!owner.IsLoaded)
        {
          owner.Show();
        }

        ownerHandle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;
      }

      return new MessageDialogResult(
        NativeInstallerMessageBox.Show(ownerHandle, message));
    }
  }
}
