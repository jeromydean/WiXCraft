using System.Collections.Generic;

namespace WiXCraft
{
  public interface IInstallerSession
  {
    bool IsMaintenance { get; }

    bool EvaluateCondition(string condition);

    string this[string property] { get; set; }

    IReadOnlyList<InstallerSessionProperty> GetProperties();
  }
}
