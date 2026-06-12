using System;
using System.Collections.Generic;

namespace WiXCraft
{
  public sealed class InstallerPropertyBag
  {
    public const string DefaultPrefix = "WIXCRAFT_";

    private readonly IInstallerSession session;
    private readonly string prefix;

    public InstallerPropertyBag(IInstallerSession session, string prefix = DefaultPrefix)
    {
      this.session = session ?? throw new ArgumentNullException(nameof(session));
      this.prefix = string.IsNullOrWhiteSpace(prefix) ? DefaultPrefix : prefix;
    }

    public string Prefix => prefix;

    public bool TrySet(string name, string value)
    {
      return session.TrySetProperty(NormalizeName(name), value ?? string.Empty);
    }

    public int TrySetMany(IEnumerable<KeyValuePair<string, string>> properties)
    {
      if (properties == null)
      {
        return 0;
      }

      int applied = 0;
      foreach (KeyValuePair<string, string> property in properties)
      {
        if (TrySet(property.Key, property.Value))
        {
          applied++;
        }
      }

      return applied;
    }

    public int TrySetMany(IDictionary<string, string> properties)
    {
      return TrySetMany((IEnumerable<KeyValuePair<string, string>>)properties);
    }

    public string NormalizeName(string name)
    {
      if (string.IsNullOrWhiteSpace(name))
      {
        throw new ArgumentException("Property name is required.", nameof(name));
      }

      if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
      {
        return name.ToUpperInvariant();
      }

      return string.Concat(prefix, name.ToUpperInvariant());
    }
  }
}
