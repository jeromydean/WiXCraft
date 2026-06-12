using System.Collections.Generic;
using WixToolset.Dtf.WindowsInstaller;

namespace ExampleInterface.Dialogs
{
  internal sealed class InstallerMessageButton
  {
    public InstallerMessageButton(string text, MessageResult result, bool isDefault)
    {
      Text = text;
      Result = result;
      IsDefault = isDefault;
    }

    public string Text { get; }

    public MessageResult Result { get; }

    public bool IsDefault { get; }

    public static IReadOnlyList<InstallerMessageButton> CreateSet(
      MessageButtons buttons,
      MessageDefaultButton defaultButton)
    {
      switch (buttons)
      {
        case MessageButtons.OKCancel:
          return new[]
          {
            Create("OK", MessageResult.OK, defaultButton, MessageDefaultButton.Button1),
            Create("Cancel", MessageResult.Cancel, defaultButton, MessageDefaultButton.Button2),
          };

        case MessageButtons.AbortRetryIgnore:
          return new[]
          {
            Create("Abort", MessageResult.Abort, defaultButton, MessageDefaultButton.Button1),
            Create("Retry", MessageResult.Retry, defaultButton, MessageDefaultButton.Button2),
            Create("Ignore", MessageResult.Ignore, defaultButton, MessageDefaultButton.Button3),
          };

        case MessageButtons.YesNoCancel:
          return new[]
          {
            Create("Yes", MessageResult.Yes, defaultButton, MessageDefaultButton.Button1),
            Create("No", MessageResult.No, defaultButton, MessageDefaultButton.Button2),
            Create("Cancel", MessageResult.Cancel, defaultButton, MessageDefaultButton.Button3),
          };

        case MessageButtons.YesNo:
          return new[]
          {
            Create("Yes", MessageResult.Yes, defaultButton, MessageDefaultButton.Button1),
            Create("No", MessageResult.No, defaultButton, MessageDefaultButton.Button2),
          };

        case MessageButtons.RetryCancel:
          return new[]
          {
            Create("Retry", MessageResult.Retry, defaultButton, MessageDefaultButton.Button1),
            Create("Cancel", MessageResult.Cancel, defaultButton, MessageDefaultButton.Button2),
          };

        default:
          return new[]
          {
            Create("OK", MessageResult.OK, defaultButton, MessageDefaultButton.Button1),
          };
      }
    }

    private static InstallerMessageButton Create(
      string text,
      MessageResult result,
      MessageDefaultButton defaultButton,
      MessageDefaultButton buttonIndex)
    {
      return new InstallerMessageButton(text, result, defaultButton == buttonIndex);
    }
  }
}
