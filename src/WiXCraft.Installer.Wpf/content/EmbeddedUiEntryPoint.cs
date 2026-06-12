using System;
using System.Configuration;
using System.Reflection;
using WiXCraft;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft.Installer.Wpf
{
  public sealed class EmbeddedUiEntryPoint : IEmbeddedUI
  {
    private readonly EmbeddedUiEngine engine;

    public EmbeddedUiEntryPoint()
    {
      string factorySpec = ConfigurationManager.AppSettings["EmbeddedUiHostFactory"];
      if (string.IsNullOrWhiteSpace(factorySpec))
      {
        throw new InvalidOperationException(
          "EmbeddedUI.config must define appSettings key EmbeddedUiHostFactory.");
      }

      IInstallerUiHostFactory factory = EmbeddedUiHostFactoryLoader.Create(factorySpec);
      engine = new EmbeddedUiEngine(factory);
    }

    public bool Initialize(Session session, string resourcePath, ref InstallUIOptions internalUILevel)
    {
      return engine.Initialize(session, resourcePath, ref internalUILevel);
    }

    public MessageResult ProcessMessage(
      InstallMessage messageType,
      Record messageRecord,
      MessageButtons buttons,
      MessageIcon icon,
      MessageDefaultButton defaultButton)
    {
      return engine.ProcessMessage(messageType, messageRecord, buttons, icon, defaultButton);
    }

    public void Shutdown()
    {
      engine.Shutdown();
      engine.Dispose();
    }
  }

  internal static class EmbeddedUiHostFactoryLoader
  {
    public static IInstallerUiHostFactory Create(string factorySpec)
    {
      int splitIndex = factorySpec.IndexOf('!');
      if (splitIndex <= 0 || splitIndex >= factorySpec.Length - 1)
      {
        throw new InvalidOperationException(
          "EmbeddedUiHostFactory must use the format AssemblyName!Namespace.TypeName.");
      }

      string assemblyName = factorySpec.Substring(0, splitIndex);
      string typeName = factorySpec.Substring(splitIndex + 1);
      Assembly assembly = Assembly.Load(assemblyName);
      Type factoryType = assembly.GetType(typeName, true);
      object instance = Activator.CreateInstance(factoryType);
      if (instance is IInstallerUiHostFactory factory)
      {
        return factory;
      }

      throw new InvalidOperationException(
        string.Concat(typeName, " must implement IInstallerUiHostFactory."));
    }
  }
}