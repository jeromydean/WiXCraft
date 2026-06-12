using CommunityToolkit.Mvvm.ComponentModel;
using MahApps.Metro.IconPacks;

namespace ExampleInterface.ViewModels
{
  public partial class NavigationItemViewModel : ObservableObject
  {
    public NavigationItemViewModel(InstallerNavigationPage page, string title, PackIconMaterialKind iconKind)
    {
      Page = page;
      Title = title;
      IconKind = iconKind;
    }

    public InstallerNavigationPage Page { get; }

    public string Title { get; }

    public PackIconMaterialKind IconKind { get; }

    [ObservableProperty]
    private bool isSelected;
  }
}
