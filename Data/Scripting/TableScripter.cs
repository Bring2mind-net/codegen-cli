using Microsoft.SqlServer.Management.Smo;
using System.Text.RegularExpressions;

namespace Bring2mind.CodeGen.Cli.Data.Scripting
{
  internal static class TableScripter
  {
    internal static void ScriptTables(this StreamWriter input, Common.Settings settings, IEnumerable<Table> tables, bool drop)
    {
      input.PrintComment("TABLES");
      var opt = ScriptUtilities.GetScriptingoptions();
      opt.ScriptDrops = drop;
      opt.IncludeIfNotExists = true;
      foreach (Table t in tables)
      {
        Console.WriteLine(string.Format("Adding {0}", t.Name));
        string tableScript = ScriptUtilities.GetScript(t.Script(opt));
        foreach (Microsoft.SqlServer.Management.Smo.Index i in t.Indexes)
        {
          if (i.IndexKeyType == IndexKeyType.DriPrimaryKey)
          {
            tableScript = tableScript.Replace(i.Name, string.Format("PK_{0}", t.Name));
          }
        }
        // get default values for columns
        foreach (Column c in t.Columns)
        {
          if (c.DefaultConstraint is object)
          {
            Console.WriteLine(string.Format("Default value on {0}", c.Name));
            foreach (var line in c.DefaultConstraint.Script(opt))
            {
              var m = Regex.Match(line, @"\s+ADD\s+DEFAULT\s+(\(.+\))\s+FOR\s+\[?(\w+)\]?");
              if (m.Success)
              {
                string defaultValue = m.Groups[1].Value;
                string columnName = m.Groups[2].Value;
                Console.WriteLine(string.Format("Injecting default value {0} on column {1}", defaultValue, columnName));
                tableScript = Regex.Replace(tableScript, @"(\s+\[" + columnName + @"\].+),", "$1 DEFAULT " + defaultValue + ",");
                break;
              }
            }
          }
        }
        // fix missing ;
        tableScript = Regex.Replace(tableScript, @"\r\n\r\nIF NOT EXISTS", ";" + Environment.NewLine + Environment.NewLine + "IF NOT EXISTS");
        input.PrintScript(tableScript.ReplaceQualifiers(settings));
      }
    }

    internal static void ScriptTableStructure(this StreamWriter input, Common.Settings settings, IEnumerable<Table> tables, bool drop)
    {
      input.PrintComment("FOREIGN KEYS");
      var opt = ScriptUtilities.GetScriptingoptions();
      opt.ScriptDrops = drop;
      opt.IncludeIfNotExists = true;
      foreach (Table t in tables)
      {
        if (ScriptUtilities.IsPatternMatch(t.Name, settings.ObjectQualifier + settings.ModuleObjectQualifier + ".*"))
        {
          foreach (ForeignKey fk in t.ForeignKeys)
          {
            string fkScript = ScriptUtilities.GetScript(fk.Script(opt));
            if (string.IsNullOrEmpty(settings.ObjectQualifier))
            {
              fkScript = fkScript.Replace(fk.Name, string.Format("FK_{0}_{1}", t.Name, fk.ReferencedTable.Replace(settings.ModuleObjectQualifier, "")));
            }
            else
            {
              fkScript = fkScript.Replace(fk.Name, string.Format("FK_{0}_{1}", t.Name, fk.ReferencedTable.Replace(settings.ObjectQualifier, "").Replace(settings.ModuleObjectQualifier, "")));
            }

            fkScript = fkScript.ReplaceQualifiers(settings);
            fkScript = Regex.Replace(fkScript, @"REFERENCES \[(\w+)\]\s", "REFERENCES " + ScriptUtilities.dnnObjectQualifier + "$1 ");
            input.PrintScript(fkScript);
          }
        }
      }
    }

    internal static void ScriptTriggers(this StreamWriter input, Common.Settings settings, IEnumerable<Table> tables, bool drop)
    {
      input.PrintComment("TRIGGERS");
      var opt = ScriptUtilities.GetScriptingoptions();
      foreach (Table t in tables)
      {
        if (ScriptUtilities.IsPatternMatch(t.Name, settings.ObjectQualifier + settings.ModuleObjectQualifier + ".*"))
        {
          foreach (Trigger tr in t.Triggers)
          {
            opt.ScriptDrops = true;
            opt.IncludeIfNotExists = true;
            string triggerScript = ScriptUtilities.GetScript(tr.Script(opt));
            input.PrintScript(triggerScript.ReplaceQualifiers(settings));
            if (!drop)
            {
              opt.ScriptDrops = false;
              opt.IncludeIfNotExists = false;
              triggerScript = ScriptUtilities.GetScript(tr.Script(opt));
              input.PrintScript(triggerScript.ReplaceQualifiers(settings));
            }
          }
        }
      }
    }
  }
}
