using System;
using System.Collections.Generic;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  public sealed class InstallerSession : IInstallerSession
  {
    private readonly Session session;

    public InstallerSession(Session session)
    {
      this.session = session;
    }

    public bool IsMaintenance =>
      session != null && session.EvaluateCondition("Installed");

    public bool EvaluateCondition(string condition)
    {
      return session != null && session.EvaluateCondition(condition);
    }

    public string this[string property]
    {
      get => session[property];
      set => session[property] = value;
    }

    public IReadOnlyList<InstallerSessionProperty> GetProperties()
    {
      var properties = new List<InstallerSessionProperty>();
      if (session?.Database == null)
      {
        return properties;
      }

      using (View view = session.Database.OpenView("SELECT Property FROM Property"))
      {
        view.Execute();
        foreach (Record record in view)
        {
          string name = record.GetString(1);
          if (string.IsNullOrEmpty(name))
          {
            continue;
          }

          properties.Add(new InstallerSessionProperty(name, session[name] ?? string.Empty));
        }
      }

      properties.Sort((left, right) =>
        string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

      return properties;
    }
  }
}
