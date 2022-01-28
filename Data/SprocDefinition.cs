using Microsoft.SqlServer.Management.Smo;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace Bring2mind.CodeGen.Cli.Data
{
  public class SprocDefinition
  {
    public string Name { get; set; } = "";
    public bool ReturnsData { get; set; } = false;
    public string ReturnObject { get; set; } = "";
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
