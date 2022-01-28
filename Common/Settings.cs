namespace Bring2mind.CodeGen.Cli.Common
{
  public class Settings
  {

    /// <summary>
    /// Template to use to generate code. Note this template can call other templates. This
    /// is just the entry point.
    /// </summary>
    public string Template { get; set; } = "";

    /// <summary>
    /// Directory to output the code to.
    /// </summary>
    public string OutputDirectory { get; set; } = "";

    /// <summary>
    /// For use in the templates
    /// </summary>
    public string OrgName { get; set; } = "";

    /// <summary>
    /// For use in the templates
    /// </summary>
    public string ModuleName { get; set; } = "";

    /// <summary>
    /// For use in the templates
    /// </summary>
    public string RootNameSpace { get; set; } = "";

    /// <summary>
    /// Full path to the web.config of the DNN Site you're using for development. Alternatively you
    /// can use the ConnectionString, ObjectQualifier, DatabaseOwner values here.
    /// </summary>
    public string SiteConfig { get; set; } = "";

    /// <summary>
    /// If the SiteConfig is not specified, use this.
    /// </summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>
    /// If the SiteConfig is not specified, use this. Note this is the object qualifier being used
    /// in this database.
    /// </summary>
    public string ObjectQualifier { get; set; } = "";

    /// <summary>
    /// If the SiteConfig is not specified, use this.
    /// </summary>
    public string DatabaseOwner { get; set; } = "dbo";

    /// <summary>
    /// Full qualifier to find the tables etc of your application. Do not include the module qualifier
    /// if you're using that.
    /// </summary>
    public string ModuleObjectQualifier { get; set; } = "";

    /// <summary>
    /// Any enums to use for database fields. The key is the table name. The values
    /// are pairs of Column and Enum strings. The generator can use this to map int columns
    /// to enums in your code.
    /// </summary>
    public Dictionary<string, List<EnumValue>> EnumValues { get; private set; }

    public class EnumValue
    {
      public string Column { get; set; }
      public string Enum { get; set; }
    }

    public static Settings Instance
    {
      get
      {
        var fileName = ".\\.codegen.json";
        var res = Globals.GetObject(fileName, new Settings(), true);
        return res;
      }
    }
  }
}