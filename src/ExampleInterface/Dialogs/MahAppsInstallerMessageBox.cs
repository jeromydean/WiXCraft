using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using WiXCraft;
using WixToolset.Dtf.WindowsInstaller;
using MessageResult = WixToolset.Dtf.WindowsInstaller.MessageResult;

namespace ExampleInterface.Dialogs
{
  internal static class MahAppsInstallerMessageBox
  {
    public static MessageResult Show(MetroWindow owner, InstallMessageEventArgs message)
    {
      if (owner == null)
      {
        throw new ArgumentNullException(nameof(owner));
      }

      if (!owner.Dispatcher.CheckAccess())
      {
        return owner.Dispatcher.Invoke(() => Show(owner, message));
      }

      if (!owner.IsLoaded)
      {
        owner.Show();
      }

      MessageResult selectedResult = InstallerMessageDialog.GetDefaultResult(
        message.Buttons,
        message.DefaultButton);

      DispatcherFrame frame = new DispatcherFrame();
      RunDialogAsync(owner, message, result =>
      {
        selectedResult = result;
        frame.Continue = false;
      });

      Dispatcher.PushFrame(frame);
      return selectedResult;
    }

    private static async void RunDialogAsync(
      MetroWindow owner,
      InstallMessageEventArgs message,
      Action<MessageResult> completed)
    {
      CustomDialog dialog = null;

      try
      {
        MetroDialogSettings settings = CreateDialogSettings();
        dialog = new CustomDialog
        {
          Title = message.DialogTitle,
        };

        dialog.Content = BuildContent(
          owner,
          dialog,
          message,
          settings,
          completed);

        await owner.ShowMetroDialogAsync(dialog, settings);
      }
      catch (Exception)
      {
        if (dialog != null)
        {
          try
          {
            await owner.HideMetroDialogAsync(dialog);
          }
          catch (InvalidOperationException)
          {
          }
        }

        completed(InstallerMessageDialog.GetDefaultResult(message.Buttons, message.DefaultButton));
      }
    }

    private static UIElement BuildContent(
      MetroWindow owner,
      CustomDialog dialog,
      InstallMessageEventArgs message,
      MetroDialogSettings settings,
      Action<MessageResult> completed)
    {
      Grid root = new Grid
      {
        MinWidth = 360,
        MaxWidth = 520,
        Margin = new Thickness(8, 4, 8, 0),
      };
      root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
      root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

      StackPanel messageRow = new StackPanel
      {
        Orientation = Orientation.Horizontal,
        Margin = new Thickness(0, 0, 0, 20),
      };
      messageRow.Children.Add(CreateIcon(message.Icon, settings));

      ScrollViewer messageHost = new ScrollViewer
      {
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        MaxHeight = 220,
        Margin = new Thickness(12, 0, 0, 0),
      };
      messageHost.Content = new TextBlock
      {
        Text = message.FormattedMessage,
        FontSize = settings.DialogMessageFontSize,
        TextWrapping = TextWrapping.Wrap,
        LineHeight = 20,
        Foreground = ResolveBrush(settings, "Installer.MutedTextBrush"),
      };
      messageRow.Children.Add(messageHost);
      Grid.SetRow(messageRow, 0);
      root.Children.Add(messageRow);

      StackPanel buttonPanel = new StackPanel
      {
        Orientation = Orientation.Horizontal,
        HorizontalAlignment = HorizontalAlignment.Right,
      };

      IReadOnlyList<InstallerMessageButton> buttons = InstallerMessageButton.CreateSet(
        message.Buttons,
        message.DefaultButton);

      foreach (InstallerMessageButton button in buttons)
      {
        Button dialogButton = CreateDialogButton(button, settings);
        dialogButton.Click += async (_, __) =>
        {
          dialogButton.IsEnabled = false;
          await CloseDialogAsync(owner, dialog, button.Result, completed);
        };
        buttonPanel.Children.Add(dialogButton);
      }

      Grid.SetRow(buttonPanel, 1);
      root.Children.Add(buttonPanel);
      return root;
    }

    private static async Task CloseDialogAsync(
      MetroWindow owner,
      CustomDialog dialog,
      MessageResult result,
      Action<MessageResult> completed)
    {
      try
      {
        await owner.HideMetroDialogAsync(dialog);
      }
      catch (InvalidOperationException)
      {
      }

      completed(result);
    }

    private static Button CreateDialogButton(
      InstallerMessageButton button,
      MetroDialogSettings settings)
    {
      Button dialogButton = new Button
      {
        Content = button.Text,
        FontSize = settings.DialogButtonFontSize,
        MinWidth = 96,
        Height = 34,
        Margin = new Thickness(8, 0, 0, 0),
        IsDefault = button.IsDefault,
        Style = button.IsDefault
          ? ResolveStyle(settings, "Installer.PrimaryButton")
          : ResolveStyle(settings, "Installer.SecondaryButton"),
      };

      return dialogButton;
    }

    private static FrameworkElement CreateIcon(MessageIcon icon, MetroDialogSettings settings)
    {
      PackIconMaterialKind iconKind;
      string brushKey;

      switch (icon)
      {
        case MessageIcon.Error:
          iconKind = PackIconMaterialKind.AlertCircle;
          brushKey = "Installer.ErrorBrush";
          break;

        case MessageIcon.Warning:
          iconKind = PackIconMaterialKind.Alert;
          brushKey = "AccentColorBrush";
          break;

        case MessageIcon.Question:
          iconKind = PackIconMaterialKind.HelpCircle;
          brushKey = "AccentColorBrush";
          break;

        case MessageIcon.Information:
          iconKind = PackIconMaterialKind.InformationOutline;
          brushKey = "AccentColorBrush";
          break;

        default:
          iconKind = PackIconMaterialKind.MessageTextOutline;
          brushKey = "AccentColorBrush";
          break;
      }

      return new PackIconMaterial
      {
        Kind = iconKind,
        Width = 28,
        Height = 28,
        VerticalAlignment = VerticalAlignment.Center,
        Foreground = ResolveBrush(settings, brushKey),
      };
    }

    private static Style ResolveStyle(MetroDialogSettings settings, string key)
    {
      return settings.CustomResourceDictionary[key] as Style;
    }

    private static System.Windows.Media.Brush ResolveBrush(MetroDialogSettings settings, string key)
    {
      return settings.CustomResourceDictionary[key] as System.Windows.Media.Brush;
    }

    private static MetroDialogSettings CreateDialogSettings()
    {
      ResourceDictionary resources = new ResourceDictionary();
      resources.MergedDictionaries.Add(new ResourceDictionary
      {
        Source = new Uri(
          "pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml",
          UriKind.Absolute),
      });
      resources.MergedDictionaries.Add(new ResourceDictionary
      {
        Source = new Uri(
          "pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml",
          UriKind.Absolute),
      });
      resources.MergedDictionaries.Add(new ResourceDictionary
      {
        Source = new Uri(
          "pack://application:,,,/MahApps.Metro;component/Styles/Themes/Dark.Red.xaml",
          UriKind.Absolute),
      });
      resources.MergedDictionaries.Add(new ResourceDictionary
      {
        Source = new Uri(
          "pack://application:,,,/ExampleInterface;component/Themes/InstallerResources.xaml",
          UriKind.Absolute),
      });

      return new MetroDialogSettings
      {
        ColorScheme = MetroDialogColorScheme.Accented,
        AnimateShow = true,
        AnimateHide = true,
        OwnerCanCloseWithDialog = false,
        DialogTitleFontSize = 18,
        DialogMessageFontSize = 14,
        DialogButtonFontSize = 13,
        CustomResourceDictionary = resources,
      };
    }
  }
}
