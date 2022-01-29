using Microsoft.SqlServer.Management.Smo;

namespace Bring2mind.CodeGen.Cli.Data.Scripting
{
  internal static class SprocScripter
  {
    internal static void ScriptSprocs(this StreamWriter input, Common.Settings settings, IEnumerable<StoredProcedure> procedures, bool drop)
    {
      input.PrintComment("SPROCS");
      var opt = ScriptUtilities.GetScriptingoptions();
      foreach (StoredProcedure p in procedures)
      {
        Console.WriteLine(string.Format("Adding {0}", p.Name));
        opt.ScriptDrops = true;
        opt.IncludeIfNotExists = true;
        string sprocScript = ScriptUtilities.GetScript(p.Script(opt));
        input.PrintScript(sprocScript.ReplaceQualifiers(settings));
        if (!drop)
        {
          opt.ScriptDrops = false;
          opt.IncludeIfNotExists = false;
          sprocScript = ScriptUtilities.GetScript(p.Script(opt));
          input.PrintScript(sprocScript.ReplaceQualifiers(settings));
        }
      }
    }
  }
}
