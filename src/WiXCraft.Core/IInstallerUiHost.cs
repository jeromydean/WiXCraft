using System;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  public interface IInstallerUiHost
  {
    void Run(IInstallerUiContext context, System.Threading.ManualResetEvent installStartEvent);

    MessageResult ProcessMessage(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton);

    void EnableExit();
  }
}
