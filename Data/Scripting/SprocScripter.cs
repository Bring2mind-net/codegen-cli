using Microsoft.SqlServer.Management.Smo;

namespace Bring2mind.CodeGen.Cli.Data.Scripting
{
  internal static class SprocScripter
  {
    internal static void ScriptSprocs(this StreamWriter input, Common.Settings settings, IEnumerable<StoredProcedure> procedures, bool drop, bool checkExists)
    {
      input.PrintComment("SPROCS");
      var opt = ScriptUtilities.GetScriptingoptions();
      opt.ScriptDrops = drop;
      opt.IncludeIfNotExists = checkExists;
      foreach (StoredProcedure p in procedures)
      {
        Console.WriteLine(string.Format("Adding {0}", p.Name));
        string sprocScript = ScriptUtilities.GetScript(p.Script(opt));
        input.PrintScript(sprocScript.ReplaceQualifiers(settings));
      }
    }
  }
}
