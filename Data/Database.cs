﻿using Microsoft.SqlServer.Management.Smo;
using System.Linq;
using System.Xml;

namespace Bring2mind.CodeGen.Cli.Data
{
  public class Database
  {
    public Microsoft.SqlServer.Management.Common.ServerConnection Connection { get; set; }
    public Server Server { get; set; }
    public Microsoft.SqlServer.Management.Smo.Database Db { get; set; }

    /// <summary>
    /// Qualifier used for this project in SQL naming
    /// </summary>
    public string ModuleQualifier { get; set; } = "";

    /// <summary>
    /// Connection string to development database
    /// </summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>
    /// If being used, DNN's objectQualifier for this database
    /// </summary>
    public string ObjectQualifier { get; set; } = "";

    /// <summary>
    /// Db owner (normally dbo)
    /// </summary>
    public string DatabaseOwner { get; set; } = "";

    /// <summary>
    /// Full regex pattern to parse out object names from the SQL object names
    /// Can be left empty for default parsing mechanism
    /// Regex pattern will need to include groups "prefix", "modqualifier" and "name"
    /// </summary>
    public string FullPattern { get; set; } = "";

    /// <summary>
    /// Regex to parse/detect other relevant tables in the database that might be
    /// used in foreign keys (think Users, Portals, Modules tables)
    /// Can be left empty for default parsing mechanism
    /// </summary>
    public string OtherTablesPattern { get; set; } = "";

    public Dictionary<string, ObjectDefinition> Objects { get; set; } = new Dictionary<string, ObjectDefinition>();
    public Dictionary<string, ObjectDefinition> LocalizationObjects { get; set; } = new Dictionary<string, ObjectDefinition>();
    public Dictionary<string, ObjectDefinition> ForeignObjects { get; set; } = new Dictionary<string, ObjectDefinition>();
    public Dictionary<string, SprocDefinition> StoredProcedures { get; set; } = new Dictionary<string, SprocDefinition>();

    public void Load(Common.Settings settings)
    {
      ConnectionString = settings.ConnectionString;
      ModuleQualifier = settings.ModuleObjectQualifier;
      ObjectQualifier = settings.ObjectQualifier;
      DatabaseOwner = settings.DatabaseOwner;
      FullPattern = settings.FullDbPattern;
      if (ConnectionString == "")
      {
        if (string.IsNullOrEmpty(settings.SiteConfig) || !File.Exists(settings.SiteConfig))
        {
          Console.WriteLine("Could not find a Web.Config");
          return;
        }
        XmlDocument webConfig = new XmlDocument();
        try
        {
          webConfig.Load(settings.SiteConfig);
          Console.WriteLine(string.Format(@"Loaded {0}", settings.SiteConfig));
          if (ConnectionString == "")
          {
            ConnectionString = webConfig.SelectSingleNode("/configuration/connectionStrings/add[@name='SiteSqlServer']").Attributes["connectionString"].InnerText;
          }

          if (ObjectQualifier == "")
          {
            ObjectQualifier = webConfig.SelectSingleNode("/configuration/dotnetnuke/data/providers/add[@name=../../@defaultProvider]").Attributes["objectQualifier"].InnerText;
          }

          if (DatabaseOwner == "")
          {
            DatabaseOwner = webConfig.SelectSingleNode("/configuration/dotnetnuke/data/providers/add[@name=../../@defaultProvider]").Attributes["databaseOwner"].InnerText;
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(string.Format("Error: {0}" + Environment.NewLine + "{1}", ex.Message, ex.StackTrace));
          return;
        }
      }

      if (!ModuleQualifier.EndsWith('_') && ModuleQualifier != "")
      {
        ModuleQualifier += "_";
      }

      if (!ObjectQualifier.EndsWith('_') && ObjectQualifier != "")
      {
        ObjectQualifier += "_";
      }

      if (DatabaseOwner == "")
      {
        DatabaseOwner = "dbo";
      }

      Console.WriteLine(string.Format("ConnectionString: {0}", ConnectionString));
      Console.WriteLine(string.Format("ObjectQualifier : {0}", ObjectQualifier));
      Console.WriteLine(string.Format("DatabaseOwner   : {0}", DatabaseOwner));

      if (ObjectQualifier == "")
      {
        OtherTablesPattern = @"(?<owner>\[?" + DatabaseOwner + @"\]?\.)?\[?(?<name>\w+)\]?|(?<owner>\[?" + DatabaseOwner + @"\]?\.)\[?(?<name>\w+)\]?|(?<=\sJOIN\s+)(?<name>\w+)";
      }
      else
      {
        OtherTablesPattern = @"(?<owner>\[?" + DatabaseOwner + @"\]?\.)?\[?(?<dnnqualifier>" + ObjectQualifier + @")(?<name>\w+)\]?|(?<owner>\[?" + DatabaseOwner + @"\]?\.)\[?(?<dnnqualifier>" + ObjectQualifier + @")?(?<name>\w+)\]?|\[?(?<dnnqualifier>" + ObjectQualifier + @")(?<name>\w+)\]?";
      }
      Console.WriteLine(string.Format("FullPattern     : {0}", FullPattern));
      Console.WriteLine();

      var conn = new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);
      Connection = new Microsoft.SqlServer.Management.Common.ServerConnection(conn);
      Server = new Server(Connection);
      Server.SetDefaultInitFields(true);
      Db = Server.Databases[Connection.DatabaseName];

      Console.WriteLine("Checking Tables");
      foreach (Table t in Db.Tables.Cast<Table>().Where(o => !o.IsSystemObject))
      {
        var m = System.Text.RegularExpressions.Regex.Match(t.Name, FullPattern);
        if (m.Success)
        {
          Console.WriteLine("Reading Table: {0}", t.Name);
          var sqlo = new ObjectDefinition(t, m);
          if (t.Name.EndsWith("Localizations"))
          {
            LocalizationObjects.Add(sqlo.Name.ToLower(), sqlo);
          }
          else
          {
            Objects.Add(sqlo.Name.ToLower(), sqlo);
          }
        }
        else
        {
          var fom = System.Text.RegularExpressions.Regex.Match(t.Name, OtherTablesPattern);
          if (fom.Success)
          {
            Console.WriteLine($"Reading Table: {t.Name} (not included in output)");
            var sqlo = new ObjectDefinition(t, fom);
            ForeignObjects.Add(sqlo.Name, sqlo);
          }
        }
      }

      Console.WriteLine("Checking Foreign Keys");
      foreach (var o in Objects.Values)
      {
        Console.WriteLine($"Processing Foreign Keys for {o.Name}");
        o.ProcessForeignKeys(this);
      }

      Console.WriteLine("Checking Child Objects");
      foreach (var o in Objects.Values)
      {
        Console.WriteLine($"Processing Child Objects for {o.Name}");
        o.ProcessChildObjects(this);
      }

      Console.WriteLine("Checking Views");
      foreach (View v in Db.Views.Cast<View>().Where(o => !o.IsSystemObject))
      {
        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(v.Name, FullPattern);
        if (m.Success)
        {
          Console.WriteLine($"Processing View: {v.Name}");
          var sqlo = new ObjectDefinition(v, m);
          if (Objects.ContainsKey(sqlo.Name.ToLower()))
          {
            Objects[sqlo.Name.ToLower()].SetView(v);
            Objects[sqlo.Name.ToLower()].Prefix = sqlo.Prefix;
          }
          else
          {
            Objects.Add(sqlo.Name.ToLower(), new ObjectDefinition(v, m));
          }
        }
      }

      Console.WriteLine("Checking Localization");
      foreach (var lo in LocalizationObjects)
      {
        Console.WriteLine("Analyzing: {0}", lo.Value.OriginalName);
        var name = lo.Value.SingularName.Replace("Localization", "");
        foreach (var o in Objects)
        {
          if (o.Value.SingularName == name)
          {
            Console.WriteLine("Found localization for: {0}", o.Value.SingularName);
            o.Value.SetLocalizationTable(lo.Value);
          }
        }
      }

      Console.WriteLine("Checking SPROCs");
      foreach (StoredProcedure s in Db.StoredProcedures.Cast<StoredProcedure>().Where(o => !o.IsSystemObject))
      {
        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(s.Name, FullPattern);
        if (m.Success)
        {
          Console.WriteLine($"Processing SPROC: {s.Name}");
          SprocDefinition sp = new SprocDefinition(s, m);
          if (sp.ReturnObject != "")
          {
            foreach (ObjectDefinition o in Objects.Values)
            {
              if (o.Table != null && o.Table.Name.ToLower() == sp.ReturnObject.ToLower())
              {
                sp.ReturnObject = o.SingularName;
              }
              else if (o.View != null && o.View.Name.ToLower() == sp.ReturnObject.ToLower())
              {
                sp.ReturnObject = o.SingularName;
              }
            }
          }
          StoredProcedures[sp.Name] = sp;
        }
      }
    }

    private static readonly Lazy<Database>
        lazy =
        new Lazy<Database>
            (() => new Database());

    public static Database Instance { get { return lazy.Value; } }

    private Database()
    {
    }

  }
}