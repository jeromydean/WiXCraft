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
  }
}
