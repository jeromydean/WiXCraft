using System.Collections.Generic;

namespace WiXCraft
{
  public interface IInstallerSession
  {
    bool IsMaintenance { get; }

    bool EvaluateCondition(string condition);

    string this[string property] { get; set; }

    bool TrySetProperty(string property, string value);

    IReadOnlyList<InstallerSessionProperty> GetProperties();

    IReadOnlyList<InstallerFeatureInfo> GetFeatures();
  }
}