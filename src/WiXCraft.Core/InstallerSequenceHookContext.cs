namespace WiXCraft
{
  public sealed class InstallerSequenceHookContext
  {
    public InstallerSequenceHookContext(
      IInstallerUiContext context,
      string hookId,
      string payload)
    {
      Context = context;
      HookId = hookId;
      Payload = payload ?? string.Empty;
    }

    public IInstallerUiContext Context { get; }

    public string HookId { get; }

    public string Payload { get; }

    public InstallOperation Operation => Context.SelectedOperation;

    public IInstallerSession Session => Context.Session;

    public bool Cancel { get; set; }
  }
}
