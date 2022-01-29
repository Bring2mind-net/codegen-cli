using Microsoft.SqlServer.Management.Smo;
using System.Text.RegularExpressions;

namespace Bring2mind.CodeGen.Cli.Data.Scripting
{
  internal static class ViewsAndFunctionsScripter
  {
    internal static void ScriptViewsAndFunctions(this StreamWriter input, Common.Settings settings, Server sqlServer, Microsoft.SqlServer.Management.Smo.Database db, bool drop)
    {
      input.PrintComment("VIEWS AND FUNCTIONS");
      var opt = ScriptUtilities.GetScriptingoptions();

      // Establish Dependencies
      var ViewDependencies = new Dictionary<string, List<string>>();
      var dw = new DependencyWalker(sqlServer);
      foreach (var v in db.GetOurViews(settings.FullDbPattern))
      {
        Console.WriteLine("Checking {0}", v.Name);
        var deps = new List<string>();
        DependencyTree dt = dw.DiscoverDependencies(new SqlSmoObject[] { v }, true);
        DependencyCollection dc = dw.WalkDependencies(dt);
        foreach (DependencyCollectionNode d in dc)
        {
          var m = Regex.Match(d.Urn.Value, @"/View\[@Name='([^']+)'");
          if (m.Success)
          {
            string ViewName = m.Groups[1].Value;
            if (ViewName != v.Name & ScriptUtilities.IsPatternMatch(ViewName, settings.ObjectQualifier + "vw_" + settings.ModuleObjectQualifier + ".*"))
            {
              if (!deps.Contains(ViewName))
              {
                deps.Add(ViewName);
              }
            }
          }

          m = Regex.Match(d.Urn.Value, @"/UserDefinedFunction\[@Name='([^']+)'");
          if (m.Success)
          {
            string FunctionName = m.Groups[1].Value;
            if (ScriptUtilities.IsPatternMatch(FunctionName, settings.ObjectQualifier + settings.ModuleObjectQualifier + ".*"))
            {
              if (!deps.Contains(FunctionName))
              {
                deps.Add(FunctionName);
              }
            }
          }
        }

        ViewDependencies[v.Name] = deps;
      }

      foreach (UserDefinedFunction f in db.GetOurFunctions(settings.FullDbPattern))
      {
        Console.WriteLine(string.Format("Checking {0}", f.Name));
        var deps = new List<string>();
        DependencyTree dt = dw.DiscoverDependencies(new SqlSmoObject[] { f }, true);
        DependencyCollection dc = dw.WalkDependencies(dt);
        foreach (DependencyCollectionNode d in dc)
        {
          var m = Regex.Match(d.Urn.Value, @"/UserDefinedFunction\[@Name='([^']+)'");
          if (m.Success)
          {
            string FunctionName = m.Groups[1].Value;
            if (FunctionName != f.Name & ScriptUtilities.IsPatternMatch(FunctionName, settings.ObjectQualifier + settings.ModuleObjectQualifier + ".*"))
            {
              if (!deps.Contains(FunctionName))
              {
                deps.Add(FunctionName);
              }
            }
          }

          m = Regex.Match(d.Urn.Value, @"/View\[@Name='([^']+)'");
          if (m.Success)
          {
            string ViewName = m.Groups[1].Value;
            if (ScriptUtilities.IsPatternMatch(ViewName, settings.ObjectQualifier + "vw_" + settings.ModuleObjectQualifier + ".*"))
            {
              if (!deps.Contains(ViewName))
              {
                deps.Add(ViewName);
              }
            }
          }
        }

        ViewDependencies[f.Name] = deps;
      }

      Console.WriteLine("Parsed dependencies for views and functions");
      foreach (string key in ViewDependencies.Keys)
        Console.WriteLine("{0}: {1}", key, string.Join(", ", ViewDependencies[key]));

      // Determine order
      var DepOrder = new List<string>();
      foreach (string key in ViewDependencies.Keys)
      {
        if (ViewDependencies[key].Count == 0)
        {
          DepOrder.Add(key);
        }
      }

      while (DepOrder.Count < ViewDependencies.Count)
      {
        bool HasAdded = false;
        foreach (string key in ViewDependencies.Keys)
        {
          if (!DepOrder.Contains(key))
          {
            var deps = ViewDependencies[key];
            bool IsOK = true;
            foreach (string dep in deps)
            {
              if (!DepOrder.Contains(dep))
              {
                IsOK = false;
              }
            }

            if (IsOK)
            {
              HasAdded = true;
              DepOrder.Add(key);
            }
          }
        }

        if (!HasAdded)
        {
          break;
        }
      }

      Console.WriteLine("Run order for views");
      foreach (string key in DepOrder)
      {
        if (db.Views.Contains(key))
        {
          View v = db.Views[key];
          Console.WriteLine(string.Format("Adding {0}", v.Name));
          opt.ScriptDrops = true;
          opt.IncludeIfNotExists = true;
          string viewScript = ScriptUtilities.GetScript(v.Script(opt));
          input.PrintScript(viewScript.ReplaceQualifiers(settings));
          if (!drop)
          {
            opt.ScriptDrops = false;
            opt.IncludeIfNotExists = false;
            viewScript = ScriptUtilities.GetScript(v.Script(opt));
            input.PrintScript(viewScript.ReplaceQualifiers(settings));
          }
        }

        if (db.UserDefinedFunctions.Contains(key))
        {
          UserDefinedFunction f = db.UserDefinedFunctions[key];
          Console.WriteLine(string.Format("Adding {0}", f.Name));
          opt.ScriptDrops = true;
          opt.IncludeIfNotExists = true;
          string functionScript = ScriptUtilities.GetScript(f.Script(opt));
          input.PrintScript(functionScript.ReplaceQualifiers(settings));
          if (!drop)
          {
            opt.ScriptDrops = false;
            opt.IncludeIfNotExists = false;
            functionScript = ScriptUtilities.GetScript(f.Script(opt));
            input.PrintScript(functionScript.ReplaceQualifiers(settings));
          }
        }
      }
    }
  }
}
