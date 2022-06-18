using Microsoft.SqlServer.Management.Smo;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace Bring2mind.CodeGen.Cli.Data
{
  /// <summary>
  /// Stored procedure used in this project
  /// </summary>
  public class SprocDefinition
  {

    /// <summary>
    /// Parsed name of the procedure
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Whether the stored procedure returns data or not
    /// </summary>
    public bool ReturnsData { get; set; } = false;

    /// <summary>
    /// If "select * from dbo.Foo" is being used this is detected and
    /// Foo will be returned here
    /// </summary>
    public string ReturnObject { get; set; } = "";

    /// <summary>
    /// The actual SQL stored procedure
    /// </summary>
    public StoredProcedure Sproc { get; set; }

    public Dictionary<string, StoredProcedureParameter> Parameters { get; set; } = new Dictionary<string, StoredProcedureParameter>();

    public SprocDefinition(StoredProcedure sp, Match m)
    {
      Name = m.Groups["name"].Value;
      if (Name.ToLower().StartsWith("get"))
      {
        string sprocText = sp.TextBody.Replace(Constants.vbCrLf, " ").Trim();
        Match m1 = Regex.Match(sprocText, @"(?i)\s*select\s+(\w+)\.\*.*?from\s+(\w+)\.(?<tablename>[^\s]+)\s+\1\s+(?-i)");
        if (m1.Success)
        {
          ReturnObject = m1.Groups["tablename"].Value;
        }

        ReturnsData = true;
      }
      Sproc = sp;
      foreach (StoredProcedureParameter p in sp.Parameters)
      {
        Parameters.Add(p.Name.TrimStart('@'), p);
      }
    }
  }
}
