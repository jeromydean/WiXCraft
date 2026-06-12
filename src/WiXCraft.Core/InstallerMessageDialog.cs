using System.Text;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  public static class InstallerMessageDialog
  {
    public static bool IsInteractive(InstallMessage messageType)
    {
      switch (messageType)
      {
        case InstallMessage.Error:
        case InstallMessage.Warning:
        case InstallMessage.User:
        case InstallMessage.FatalExit:
        case InstallMessage.OutOfDiskSpace:
        case InstallMessage.FilesInUse:
          return true;

        default:
          return false;
      }
    }

    public static string FormatMessage(InstallMessage messageType, Record messageRecord)
    {
      if (messageRecord == null || messageRecord.FieldCount == 0)
      {
        return messageType.ToString();
      }

      StringBuilder builder = new StringBuilder();
      for (int fieldIndex = 1; fieldIndex <= messageRecord.FieldCount; fieldIndex++)
      {
        string value = messageRecord.GetString(fieldIndex);
        if (string.IsNullOrWhiteSpace(value))
        {
          continue;
        }

        if (builder.Length > 0)
        {
          builder.AppendLine();
        }

        builder.Append(value);
      }

      return builder.Length > 0 ? builder.ToString() : messageType.ToString();
    }

    public static string GetDialogTitle(InstallMessage messageType)
    {
      switch (messageType)
      {
        case InstallMessage.Error:
        case InstallMessage.FatalExit:
          return "Setup Error";

        case InstallMessage.Warning:
          return "Setup Warning";

        case InstallMessage.OutOfDiskSpace:
          return "Disk Space";

        case InstallMessage.FilesInUse:
          return "Files In Use";

        default:
          return "Setup";
      }
    }

    public static MessageResult GetDefaultResult(
      MessageButtons buttons,
      MessageDefaultButton defaultButton)
    {
      switch (buttons)
      {
        case MessageButtons.OK:
          return MessageResult.OK;

        case MessageButtons.OKCancel:
          return defaultButton == MessageDefaultButton.Button2
            ? MessageResult.Cancel
            : MessageResult.OK;

        case MessageButtons.AbortRetryIgnore:
          switch (defaultButton)
          {
            case MessageDefaultButton.Button2:
              return MessageResult.Retry;
            case MessageDefaultButton.Button3:
              return MessageResult.Ignore;
            default:
              return MessageResult.Abort;
          }

        case MessageButtons.YesNoCancel:
          switch (defaultButton)
          {
            case MessageDefaultButton.Button2:
              return MessageResult.No;
            case MessageDefaultButton.Button3:
              return MessageResult.Cancel;
            default:
              return MessageResult.Yes;
          }

        case MessageButtons.YesNo:
          return defaultButton == MessageDefaultButton.Button2
            ? MessageResult.No
            : MessageResult.Yes;

        case MessageButtons.RetryCancel:
          return defaultButton == MessageDefaultButton.Button2
            ? MessageResult.Cancel
            : MessageResult.Retry;

        default:
          return MessageResult.OK;
      }
    }
  }
}
