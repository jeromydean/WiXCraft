using System;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  public sealed class InstallMessageEventArgs : EventArgs
  {
    public InstallMessageEventArgs(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton)
    {
      MessageType = messageType;
      MessageRecord = messageRecord;
      Buttons = buttons;
      Icon = icon;
      DefaultButton = defaultButton;
    }

    public InstallMessage MessageType { get; }

    public Record MessageRecord { get; }

    public MessageButtons Buttons { get; }

    public MessageIcon Icon { get; }

    public MessageDefaultButton DefaultButton { get; }

    public MessageResult Result { get; set; } = MessageResult.OK;
  }
}
