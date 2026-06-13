using System;
using System.Runtime.InteropServices;
using WiXCraft;
using WixToolset.Dtf.WindowsInstaller;

namespace ExampleInterface.Dialogs
{
  internal static class NativeInstallerMessageBox
  {
    private const uint MbOk = 0x00000000;
    private const uint MbOkCancel = 0x00000001;
    private const uint MbAbortRetryIgnore = 0x00000002;
    private const uint MbYesNoCancel = 0x00000003;
    private const uint MbYesNo = 0x00000004;
    private const uint MbRetryCancel = 0x00000005;

    private const uint MbIconError = 0x00000010;
    private const uint MbIconQuestion = 0x00000020;
    private const uint MbIconWarning = 0x00000030;
    private const uint MbIconInformation = 0x00000040;

    private const uint MbDefButton1 = 0x00000000;
    private const uint MbDefButton2 = 0x00000100;
    private const uint MbDefButton3 = 0x00000200;

    private const int IdOk = 1;
    private const int IdCancel = 2;
    private const int IdAbort = 3;
    private const int IdRetry = 4;
    private const int IdIgnore = 5;
    private const int IdYes = 6;
    private const int IdNo = 7;

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public static MessageResult Show(
      IntPtr owner,
      InstallMessageEventArgs message)
    {
      uint flags = MapButtons(message.Buttons) |
        MapIcon(message.Icon) |
        MapDefaultButton(message.DefaultButton);

      int selectedButton = MessageBox(
        owner,
        message.FormattedMessage,
        message.DialogTitle,
        flags);

      return MapResult(selectedButton, message.Buttons);
    }

    private static uint MapButtons(MessageButtons buttons)
    {
      switch (buttons)
      {
        case MessageButtons.OKCancel:
          return MbOkCancel;

        case MessageButtons.AbortRetryIgnore:
          return MbAbortRetryIgnore;

        case MessageButtons.YesNoCancel:
          return MbYesNoCancel;

        case MessageButtons.YesNo:
          return MbYesNo;

        case MessageButtons.RetryCancel:
          return MbRetryCancel;

        default:
          return MbOk;
      }
    }

    private static uint MapIcon(MessageIcon icon)
    {
      switch (icon)
      {
        case MessageIcon.Error:
          return MbIconError;

        case MessageIcon.Question:
          return MbIconQuestion;

        case MessageIcon.Warning:
          return MbIconWarning;

        case MessageIcon.Information:
          return MbIconInformation;

        default:
          return 0;
      }
    }

    private static uint MapDefaultButton(MessageDefaultButton defaultButton)
    {
      switch (defaultButton)
      {
        case MessageDefaultButton.Button2:
          return MbDefButton2;

        case MessageDefaultButton.Button3:
          return MbDefButton3;

        default:
          return MbDefButton1;
      }
    }

    private static MessageResult MapResult(int selectedButton, MessageButtons buttons)
    {
      switch (buttons)
      {
        case MessageButtons.OKCancel:
          return selectedButton == IdCancel ? MessageResult.Cancel : MessageResult.OK;

        case MessageButtons.AbortRetryIgnore:
          switch (selectedButton)
          {
            case IdRetry:
              return MessageResult.Retry;
            case IdIgnore:
              return MessageResult.Ignore;
            default:
              return MessageResult.Abort;
          }

        case MessageButtons.YesNoCancel:
          switch (selectedButton)
          {
            case IdNo:
              return MessageResult.No;
            case IdCancel:
              return MessageResult.Cancel;
            default:
              return MessageResult.Yes;
          }

        case MessageButtons.YesNo:
          return selectedButton == IdNo ? MessageResult.No : MessageResult.Yes;

        case MessageButtons.RetryCancel:
          return selectedButton == IdCancel ? MessageResult.Cancel : MessageResult.Retry;

        default:
          return MessageResult.OK;
      }
    }
  }
}
