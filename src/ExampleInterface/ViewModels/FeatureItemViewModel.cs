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
      Title = feature.Title;
      Description = feature.Description;
      isSelected = feature.IsRequestedForInstall;
      isEnabled = feature.CanChangeSelection;
    }

    public string Title { get; }

    public string Description { get; }

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

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