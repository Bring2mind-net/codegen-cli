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

      var installScriptFile = Path.Combine(dir.FullName, "Install.SqlDataProvider");
      var upgradeScriptFile = Path.Combine(dir.FullName, "Upgrade.SqlDataProvider");
      var uninstallScriptFile = Path.Combine(dir.FullName, "Uninstall.SqlDataProvider");

      var ourTables = db.GetOurTables(settings.FullDbPattern);
      var ourSprocs = db.GetOurStoredProcedures(settings.FullDbPattern);

      using (var tablesScript = new StreamWriter(installScriptFile, false))
      {
        tablesScript.ScriptTables(settings, ourTables, false, true);
        tablesScript.ScriptTableStructure(settings, ourTables, false, true);
      }

      using (var upgradeScript = new StreamWriter(upgradeScriptFile, false))
      {
        // first uninstall everything
        upgradeScript.ScriptSprocs(settings, ourSprocs, true, true);
        upgradeScript.ScriptViewsAndFunctions(settings, sqlServer, db, true, true);
        upgradeScript.ScriptTriggers(settings, ourTables, true, true);
        upgradeScript.ScriptTypes(settings, db, true, true);

        // now reinstall
        upgradeScript.ScriptTypes(settings, db, false, false);
        upgradeScript.ScriptTriggers(settings, ourTables, false, false);
        upgradeScript.ScriptViewsAndFunctions(settings, sqlServer, db, false, false);
        upgradeScript.ScriptSprocs(settings, ourSprocs, false, false);
      }

      using (var uninstallScript = new StreamWriter(uninstallScriptFile, false))
      {
        uninstallScript.ScriptSprocs(settings, ourSprocs, true, true);
        uninstallScript.ScriptViewsAndFunctions(settings, sqlServer, db, true, true);
        uninstallScript.ScriptTriggers(settings, ourTables, true, true);
        uninstallScript.ScriptTypes(settings, db, true, true);
        uninstallScript.ScriptTableStructure(settings, ourTables, true, true);
        uninstallScript.ScriptTables(settings, ourTables, true, true);
      }

      Console.WriteLine("Finished Generating SQL scripts");
    }
  }
}
