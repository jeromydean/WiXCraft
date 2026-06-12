using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  public sealed class MessageDialogResult
  {
    public MessageDialogResult(MessageResult result)
    {
      Result = result;
    }

    public MessageResult Result { get; }
  }
}
