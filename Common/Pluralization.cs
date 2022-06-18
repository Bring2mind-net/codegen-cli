using Pluralize.NET.Core;
using System.Globalization;

namespace Bring2mind.CodeGen.Cli.Common
{
  public class Pluralization
  {
    public Dictionary<string, string> Plurals { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Get the plural version of the supplied string
    /// </summary>
    /// <param name="value">String to get plural for</param>
    /// <returns></returns>
    public string Pluralize(string value)
    {
      foreach (KeyValuePair<string, string> kv in Plurals)
      {
        if (value.EndsWith(kv.Value, false, new CultureInfo("en-US")))
        {
          return value.Substring(0, value.Length - kv.Value.Length) + kv.Key;
        }
      }
      return new Pluralizer().Pluralize(value);
    }

    /// <summary>
    /// Get the singular version of the supplied string
    /// </summary>
    /// <param name="value">String to get singular for</param>
    /// <returns></returns>
    public string Singularize(string value)
    {
      foreach (KeyValuePair<string, string> kv in Plurals)
      {
        if (value.EndsWith(kv.Key, false, new CultureInfo("en-US")))
        {
          return value.Substring(0, value.Length - kv.Key.Length) + kv.Value;
        }
      }
      return new Pluralizer().Singularize(value);
    }
  }

}