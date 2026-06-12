using System;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  public sealed class InstallerFeatureInfo
  {
    private readonly Session session;
    private readonly string title;
    private readonly string description;

    internal InstallerFeatureInfo(
      Session session,
      string id,
      string title,
      string description)
    {
      this.session = session ?? throw new ArgumentNullException(nameof(session));
      Id = id ?? throw new ArgumentNullException(nameof(id));
      this.title = title ?? id;
      this.description = description ?? string.Empty;
    }

    public string Id { get; }

    public string Title => title;

    public string Description => description;

    public FeatureInstallState CurrentState => MapState(GetFeatureState(feature => feature.CurrentState));

    public FeatureInstallState RequestState
    {
      get => MapState(GetFeatureState(feature => feature.RequestState));
      set => SetFeatureState(feature => feature.RequestState = MapState(value));
    }

    public bool IsInstalled
    {
      get
      {
        InstallState state = GetFeatureState(feature => feature.CurrentState);
        return state == InstallState.Local ||
          state == InstallState.Source ||
          state == InstallState.Default;
      }
    }

    public bool IsRequestedForInstall
    {
      get
      {
        InstallState state = GetFeatureState(feature => feature.RequestState);
        return state == InstallState.Local ||
          state == InstallState.Source ||
          state == InstallState.Default ||
          state == InstallState.Advertised;
      }
    }

    public bool CanChangeSelection
    {
      get
      {
        try
        {
          FeatureInfo feature = session.Features[Id];
          return feature.ValidStates.Contains(InstallState.Local) ||
            feature.ValidStates.Contains(InstallState.Absent);
        }
        catch (InvalidHandleException)
        {
          return false;
        }
      }
    }

    public void SetRequestedForInstall(bool requested)
    {
      if (!CanChangeSelection)
      {
        return;
      }

      FeatureInfo feature = session.Features[Id];

      if (requested)
      {
        if (feature.ValidStates.Contains(InstallState.Local))
        {
          feature.RequestState = InstallState.Local;
        }
        else if (feature.ValidStates.Contains(InstallState.Advertised))
        {
          feature.RequestState = InstallState.Advertised;
        }

        return;
      }

      if (feature.ValidStates.Contains(InstallState.Absent))
      {
        feature.RequestState = InstallState.Absent;
      }
    }

    private T GetFeatureState<T>(Func<FeatureInfo, T> selector)
    {
      try
      {
        return selector(session.Features[Id]);
      }
      catch (InvalidHandleException)
      {
        if (typeof(T) == typeof(InstallState))
        {
          return (T)(object)InstallState.Unknown;
        }

        throw;
      }
    }

    private void SetFeatureState(Action<FeatureInfo> setter)
    {
      setter(session.Features[Id]);
    }

    private static FeatureInstallState MapState(InstallState state)
    {
      switch (state)
      {
        case InstallState.Absent:
          return FeatureInstallState.Absent;

        case InstallState.Advertised:
          return FeatureInstallState.Advertised;

        case InstallState.Local:
          return FeatureInstallState.Local;

        case InstallState.Source:
          return FeatureInstallState.Source;

        case InstallState.Default:
          return FeatureInstallState.Default;

        default:
          return FeatureInstallState.Unknown;
      }
    }

    private static InstallState MapState(FeatureInstallState state)
    {
      switch (state)
      {
        case FeatureInstallState.Absent:
          return InstallState.Absent;

        case FeatureInstallState.Advertised:
          return InstallState.Advertised;

        case FeatureInstallState.Local:
          return InstallState.Local;

        case FeatureInstallState.Source:
          return InstallState.Source;

        case FeatureInstallState.Default:
          return InstallState.Default;

        default:
          return InstallState.Unknown;
      }
    }
  }
}