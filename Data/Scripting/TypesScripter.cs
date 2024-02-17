using Microsoft.SqlServer.Management.Smo;

namespace Bring2mind.CodeGen.Cli.Data.Scripting
{
  internal static class TypesScripter
  {
    internal static void ScriptTypes(this StreamWriter input, Common.Settings settings, Microsoft.SqlServer.Management.Smo.Database db, bool drop)
    {
      input.PrintComment("TYPES");
      var opt = ScriptUtilities.GetScriptingoptions();

      foreach (var dt in db.GetOurDataTypes(settings.FullDbPattern))
      {
        Console.WriteLine(string.Format("Adding {0}", dt.Name));
        opt.ScriptDrops = true;
        opt.IncludeIfNotExists = true;
        string sprocScript = ScriptUtilities.GetScript(dt.Script(opt));
        input.PrintScript(sprocScript.ReplaceQualifiers(settings));
        if (!drop)
        {
          opt.ScriptDrops = false;
          opt.IncludeIfNotExists = false;
          sprocScript = ScriptUtilities.GetScript(dt.Script(opt));
          input.PrintScript(sprocScript.ReplaceQualifiers(settings));
        }
      }

      foreach (var dt in db.GetOurTableTypes(settings.FullDbPattern))
      {
        Console.WriteLine(string.Format("Adding {0}", dt.Name));
        opt.ScriptDrops = true;
        opt.IncludeIfNotExists = true;
        string sprocScript = ScriptUtilities.GetScript(dt.Script(opt));
        input.PrintScript(sprocScript.ReplaceQualifiers(settings));
        if (!drop)
        {
          opt.ScriptDrops = false;
          opt.IncludeIfNotExists = false;
          sprocScript = ScriptUtilities.GetScript(dt.Script(opt));
          input.PrintScript(sprocScript.ReplaceQualifiers(settings));
        }
      }
    }
  }
}
