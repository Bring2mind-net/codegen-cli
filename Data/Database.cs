using Microsoft.SqlServer.Management.Smo;
using System.Xml;

namespace Bring2mind.CodeGen.Cli.Data
{
  public class Database
  {
    public Microsoft.SqlServer.Management.Common.ServerConnection Connection { get; set; }
    public Server Server { get; set; }
    public Microsoft.SqlServer.Management.Smo.Database Db { get; set; }

    public string ModuleQualifier { get; set; } = "";
    public string ConnectionString { get; set; } = "";
    public string ObjectQualifier { get; set; } = "";
    public string DatabaseOwner { get; set; } = "";
    public string FullPattern { get; set; } = "";
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
        if (ModuleQualifier == "")
        {
          FullPattern = @"(?<owner>\[?" + DatabaseOwner + @"\]?\.)?\[?(?<name>\w+)\]?|(?<owner>\[?" + DatabaseOwner + @"\]?\.)\[?(?<name>\w+)\]?|(?<=\sJOIN\s+)(?<name>\w+)";
        }
        else
        {
          FullPattern = @"(?<owner>\[?" + DatabaseOwner + @"\]?\.)?\[?(?<prefix>\w*)(?<modqualifier>" + ModuleQualifier + @")(?<name>\w+)\]?|(?<owner>\[?" + DatabaseOwner + @"\]?\.)\[?(?<name>\w+)\]?|(?<=\sJOIN\s+)(?<name>\w+)";
        }

        OtherTablesPattern = @"(?<owner>\[?" + DatabaseOwner + @"\]?\.)?\[?(?<name>\w+)\]?|(?<owner>\[?" + DatabaseOwner + @"\]?\.)\[?(?<name>\w+)\]?|(?<=\sJOIN\s+)(?<name>\w+)";
      }
      else
      {
        if (ModuleQualifier == "")
        {
          FullPattern = @"(?<owner>\[?" + DatabaseOwner + @"\]?\.)?\[?(?<dnnqualifier>" + ObjectQualifier + @")(?<name>\w+)\]?|(?<owner>\[?" + DatabaseOwner + @"\]?\.)\[?(?<dnnqualifier>" + ObjectQualifier + @")?(?<name>\w+)\]?|\[?(?<dnnqualifier>" + ObjectQualifier + @")(?<name>\w+)\]?";
        }
        else
        {
          FullPattern = @"(?<owner>\[?" + DatabaseOwner + @"\]?\.)?\[?(?<dnnqualifier>" + ObjectQualifier + @")(?<prefix>\w*)(?<modqualifier>" + ModuleQualifier + @")(?<name>\w+)\]?|(?<owner>\[?" + DatabaseOwner + @"\]?\.)\[?(?<dnnqualifier>" + ObjectQualifier + @")(?<prefix>\w*)(?<modqualifier>" + ModuleQualifier + @")(?<name>\w+)\]?";
        }

        OtherTablesPattern = @"(?<owner>\[?" + DatabaseOwner + @"\]?\.)?\[?(?<dnnqualifier>" + ObjectQualifier + @")(?<name>\w+)\]?|(?<owner>\[?" + DatabaseOwner + @"\]?\.)\[?(?<dnnqualifier>" + ObjectQualifier + @")?(?<name>\w+)\]?|\[?(?<dnnqualifier>" + ObjectQualifier + @")(?<name>\w+)\]?";
      }
      Console.WriteLine(string.Format("FullPattern     : {0}", FullPattern));
      Console.WriteLine();

      var conn = new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);
      Connection = new Microsoft.SqlServer.Management.Common.ServerConnection(conn);
      Server = new Server(Connection);
      Db = Server.Databases[Connection.DatabaseName];

      foreach (Table t in Db.Tables)
      {
        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(t.Name, FullPattern);
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
          System.Text.RegularExpressions.Match fom = System.Text.RegularExpressions.Regex.Match(t.Name, OtherTablesPattern);
          if (fom.Success)
          {
            Console.WriteLine($"Reading Table: {t.Name} (not included in output)");
            var sqlo = new ObjectDefinition(t, fom);
            ForeignObjects.Add(sqlo.Name, sqlo);
          }
        }
      }

      foreach (var o in Objects.Values)
      {
        Console.WriteLine($"Processing Foreign Keys for {o.Name}");
        o.ProcessForeignKeys(this);
      }
      foreach (var o in Objects.Values)
      {
        Console.WriteLine($"Processing Child Objects for {o.Name}");
        o.ProcessChildObjects(this);
      }

      foreach (View v in Db.Views)
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

      foreach (StoredProcedure s in Db.StoredProcedures)
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
          StoredProcedures.Add(sp.Name, sp);
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