using Microsoft.SqlServer.Management.Smo;
using System.Text;
using System.Text.RegularExpressions;

namespace Bring2mind.CodeGen.Cli.Data.Scripting
{
  internal static class ScriptUtilities
  {
    internal const string dnnObjectQualifier = "{objectQualifier}";
    internal const string dnnDatabaseOwner = "{databaseOwner}";

    internal static ScriptingOptions GetScriptingoptions()
    {
      var opt = new ScriptingOptions();
      opt.DriAll = false;
      opt.DriAllKeys = false;
      opt.Indexes = true;
      opt.NoCollation = true;
      opt.TargetDatabaseEngineType = Microsoft.SqlServer.Management.Common.DatabaseEngineType.SqlAzureDatabase;
      return opt;
    }

    internal static bool IsPatternMatch(string input, string pattern)
    {
      var m = Regex.Match(input, pattern);
      return m.Success;
    }

    internal static void PrintComment(this StreamWriter input, string comment)
    {
      input.WriteLine(string.Format("/******* {0} *******/", comment));
      input.Flush();
    }

    internal static void PrintScript(this StreamWriter input, string script)
    {
      input.WriteLine(script);
      input.Flush();
    }

    internal static string GetScript(System.Collections.Specialized.StringCollection script)
    {
      var res = new StringBuilder();
      foreach (var l in script)
      {
        res.AppendLine(l);
      }
      res.AppendLine("GO");
      res.AppendLine("");
      return res.ToString().TrimStart(Environment.NewLine.ToCharArray());
    }

    internal static string ReplaceQualifiers(this string script, Common.Settings settings)
    {
      script = Regex.Replace(script, @"(\r?\n){2,}", Environment.NewLine); // remove double linefeeds
      script = Regex.Replace(script, @"\t", " "); // tabs to spaces
      script = Regex.Replace(script, settings.FullDbPattern, (m) =>
      {
        string res = "";
        if (m.Groups["owner"].Success)
        {
          res += dnnDatabaseOwner;
        }

        if (m.Groups["name"].Value.ToLower().StartsWith("sp_"))
        {
          return "dbo." + m.Groups["name"].Value;
        }

        if (m.Groups["prefix"].Success && m.Groups["prefix"].Value != "vw_")
        {
          res += m.Groups["prefix"].Value;
        }

        res += dnnObjectQualifier;
        if (m.Groups["prefix"].Success && m.Groups["prefix"].Value == "vw_")
        {
          res += m.Groups["prefix"].Value;
        }

        if (m.Groups["modqualifier"].Success)
        {
          res += settings.ModuleObjectQualifier;
        }

        res += m.Groups["name"].Value;
        return res;
      });
      script = Regex.Replace(script, @"(?i)SET (\w+) ON\s+(?=SET|CREATE)(?-i)", "SET $1 ON" + Environment.NewLine + "GO" + Environment.NewLine);
      return script;
    }

  }
}
