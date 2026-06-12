using CommunityToolkit.Mvvm.ComponentModel;
using WiXCraft;

namespace ExampleInterface.ViewModels
{
  public partial class FeatureItemViewModel : ObservableObject
  {
    private readonly InstallerFeatureInfo feature;

    public FeatureItemViewModel(InstallerFeatureInfo feature)
    {
      this.feature = feature;
      DisplayName = string.IsNullOrWhiteSpace(feature.Description)
        ? feature.Title
        : string.Concat(feature.Title, " - ", feature.Description);
      isSelected = feature.IsRequestedForInstall;
      isEnabled = feature.CanChangeSelection;
    }

    public string DisplayName { get; }

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isEnabled;

    public void ApplyToSession()
    {
      feature.SetRequestedForInstall(IsSelected);
    }
  }
}