using Microsoft.SqlServer.Management.Smo;

namespace Bring2mind.CodeGen.Cli.Data.Scripting
{
  internal class ScriptGenerator
  {
    internal static void GenerateScripts(Server sqlServer, Microsoft.SqlServer.Management.Smo.Database db, Common.Settings settings)
    {

      Console.WriteLine("Generating SQL scripts");

      var dir = new DirectoryInfo(Path.Combine(settings.OutputDirectory, "Scripts"));
      if (!dir.Exists)
      {
        dir.Create();
      }

      var tablesScriptFile = Path.Combine(dir.FullName, "Install.SqlDataProvider");
      var upgradeScriptFile = Path.Combine(dir.FullName, "Upgrade.SqlDataProvider");
      var uninstallScriptFile = Path.Combine(dir.FullName, "Uninstall.SqlDataProvider");

      var ourTables = db.GetOurTables(settings.FullDbPattern);
      var ourSprocs = db.GetOurStoredProcedures(settings.FullDbPattern);

      using (var tablesScript = new StreamWriter(tablesScriptFile, false))
      {
        tablesScript.ScriptTables(settings, ourTables, false);
        tablesScript.ScriptTableStructure(settings, ourTables, false);
        tablesScript.ScriptTriggers(settings, ourTables, false);
      }

      using (var upgradeScript = new StreamWriter(upgradeScriptFile, false))
      {
        upgradeScript.ScriptViewsAndFunctions(settings, sqlServer, db, false);
        upgradeScript.ScriptSprocs(settings, ourSprocs, false);
      }

      using (var uninstallScript = new StreamWriter(uninstallScriptFile, false))
      {
        uninstallScript.ScriptSprocs(settings, ourSprocs, true);
        uninstallScript.ScriptViewsAndFunctions(settings, sqlServer, db, false);
        uninstallScript.ScriptTriggers(settings, ourTables, false);
        uninstallScript.ScriptTableStructure(settings, ourTables, false);
        uninstallScript.ScriptTables(settings, ourTables, false);
      }

      Console.WriteLine("Finished Generating SQL scripts");
    }
  }
}
