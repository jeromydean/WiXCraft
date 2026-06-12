using System;
using System.Collections.Generic;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  public sealed class InstallerFeatureInfo
  {
    private readonly Session session;
    private readonly string title;
    private readonly string description;
    private readonly int defaultLevel;

    internal InstallerFeatureInfo(
      Session session,
      string id,
      string title,
      string description,
      int defaultLevel)
    {
      this.session = session ?? throw new ArgumentNullException(nameof(session));
      Id = id ?? throw new ArgumentNullException(nameof(id));
      this.title = title ?? id;
      this.description = description ?? string.Empty;
      this.defaultLevel = defaultLevel;
    }

    public string Id { get; }

    public string Title => title;

    public string Description => description;

    public FeatureInstallState CurrentState =>
      MapState(TryGetLiveInstallState(feature => feature.CurrentState, out InstallState state)
        ? state
        : InstallState.Unknown);

    public FeatureInstallState RequestState
    {
      get => MapState(TryGetLiveInstallState(feature => feature.RequestState, out InstallState state)
        ? state
        : InstallState.Unknown);
      set => SetFeatureState(feature => feature.RequestState = MapState(value));
    }

    public bool IsInstalled
    {
      get
      {
        if (TryGetLiveInstallState(feature => feature.CurrentState, out InstallState state))
        {
          return state == InstallState.Local ||
            state == InstallState.Source ||
            state == InstallState.Default;
        }

        return false;
      }
    }

    public bool IsRequestedForInstall
    {
      get
      {
        if (TryGetLiveInstallState(feature => feature.RequestState, out InstallState state))
        {
          return state == InstallState.Local ||
            state == InstallState.Source ||
            state == InstallState.Default ||
            state == InstallState.Advertised;
        }

        return IsSelectedByDefaultLevel();
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
        catch (Exception ex) when (IsUnavailableFeatureStateException(ex))
        {
          return true;
        }
      }
    }

    public void SetRequestedForInstall(bool requested)
    {
      try
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
      catch (Exception ex) when (IsUnavailableFeatureStateException(ex))
      {
        ApplyFeatureViaProperties(requested);
      }
    }

    private void ApplyFeatureViaProperties(bool requested)
    {
      if (requested)
      {
        session["ADDLOCAL"] = AppendFeatureProperty(session["ADDLOCAL"], Id);
        session["REMOVE"] = RemoveFeatureFromProperty(session["REMOVE"], Id);
      }
      else
      {
        session["REMOVE"] = AppendFeatureProperty(session["REMOVE"], Id);
        session["ADDLOCAL"] = RemoveFeatureFromProperty(session["ADDLOCAL"], Id);
      }
    }

    private static string AppendFeatureProperty(string existing, string featureId)
    {
      if (string.IsNullOrWhiteSpace(existing))
      {
        return featureId;
      }

      if (ContainsFeatureId(existing, featureId))
      {
        return existing;
      }

      return string.Concat(existing, ",", featureId);
    }

    private static string RemoveFeatureFromProperty(string existing, string featureId)
    {
      if (string.IsNullOrWhiteSpace(existing))
      {
        return string.Empty;
      }

      string[] parts = existing.Split(',');
      List<string> remaining = new List<string>();
      foreach (string part in parts)
      {
        string trimmed = part.Trim();
        if (trimmed.Length > 0 &&
          !string.Equals(trimmed, featureId, StringComparison.OrdinalIgnoreCase))
        {
          remaining.Add(trimmed);
        }
      }

      return string.Join(",", remaining);
    }

    private static bool ContainsFeatureId(string propertyValue, string featureId)
    {
      string[] parts = propertyValue.Split(',');
      foreach (string part in parts)
      {
        if (string.Equals(part.Trim(), featureId, StringComparison.OrdinalIgnoreCase))
        {
          return true;
        }
      }

      return false;
    }

    private bool IsSelectedByDefaultLevel()
    {
      if (defaultLevel <= 0)
      {
        return false;
      }

      if (!int.TryParse(session["INSTALLLEVEL"], out int installLevel))
      {
        installLevel = 1;
      }

      return defaultLevel <= installLevel;
    }

    private bool TryGetLiveInstallState(
      Func<FeatureInfo, InstallState> selector,
      out InstallState state)
    {
      try
      {
        state = selector(session.Features[Id]);
        return true;
      }
      catch (Exception ex) when (IsUnavailableFeatureStateException(ex))
      {
        state = InstallState.Unknown;
        return false;
      }
    }

    private void SetFeatureState(Action<FeatureInfo> setter)
    {
      setter(session.Features[Id]);
    }

    private static bool IsUnavailableFeatureStateException(Exception exception)
    {
      return exception is InvalidHandleException || exception is ArgumentException;
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
