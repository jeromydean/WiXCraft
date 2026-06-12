using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  public sealed class InstallerUiModeOptions
  {
    public InstallUIOptions RequiredInitializeLevel { get; set; } = InstallUIOptions.Full;

    public InstallUIOptions PostInitializeLevel { get; set; } =
      InstallUIOptions.NoChange | InstallUIOptions.SourceResolutionOnly;

    public InstallerOperationModes SupportedOperations { get; set; } = InstallerOperationModes.All;

    public string RepairReinstallMode { get; set; } = "ecmus";

    public bool HandleEngineDialogs { get; set; } = true;

    public static InstallerUiModeOptions CreateDefault()
    {
      return new InstallerUiModeOptions();
    }

    public bool SupportsOperation(InstallOperation operation)
    {
      InstallerOperationModes requiredFlag = MapOperationToFlag(operation);
      if (requiredFlag == InstallerOperationModes.None)
      {
        return false;
      }

      return (SupportedOperations & requiredFlag) == requiredFlag;
    }

    public bool SupportsAnyMaintenance()
    {
      return (SupportedOperations & (InstallerOperationModes.Repair |
        InstallerOperationModes.Modify |
        InstallerOperationModes.Uninstall)) != InstallerOperationModes.None;
    }

    private static InstallerOperationModes MapOperationToFlag(InstallOperation operation)
    {
      switch (operation)
      {
        case InstallOperation.Install:
          return InstallerOperationModes.FreshInstall;

        case InstallOperation.Repair:
          return InstallerOperationModes.Repair;

        case InstallOperation.Modify:
          return InstallerOperationModes.Modify;

        case InstallOperation.Uninstall:
          return InstallerOperationModes.Uninstall;

        default:
          return InstallerOperationModes.None;
      }
    }
  }
}
