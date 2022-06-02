using Microsoft.SqlServer.Management.Smo;

namespace Bring2mind.CodeGen.Cli.Data
{
  internal static class Utilities
  {
    internal static IEnumerable<Table> GetOurTables(this Microsoft.SqlServer.Management.Smo.Database db, string pattern)
    {
      foreach (Table t in db.Tables)
      {
        var m = System.Text.RegularExpressions.Regex.Match(t.Name, pattern);
        if (m.Success)
        {
          yield return t;
        }
      }
    }

    internal static IEnumerable<View> GetOurViews(this Microsoft.SqlServer.Management.Smo.Database db, string pattern)
    {
      foreach (View v in db.Views)
      {
        var m = System.Text.RegularExpressions.Regex.Match(v.Name, pattern);
        if (m.Success)
        {
          yield return v;
        }
      }
    }

    internal static IEnumerable<StoredProcedure> GetOurStoredProcedures(this Microsoft.SqlServer.Management.Smo.Database db, string pattern)
    {
      foreach (StoredProcedure s in db.StoredProcedures)
      {
        var m = System.Text.RegularExpressions.Regex.Match(s.Name, pattern);
        if (m.Success)
        {
          yield return s;
        }
      }
    }

    internal static IEnumerable<UserDefinedFunction> GetOurFunctions(this Microsoft.SqlServer.Management.Smo.Database db, string pattern)
    {
      foreach (UserDefinedFunction u in db.UserDefinedFunctions)
      {
        var m = System.Text.RegularExpressions.Regex.Match(u.Name, pattern);
        if (m.Success)
        {
          yield return u;
        }
      }
    }

    internal static string FirstCharToUpper(this string input)
    {
      if (string.IsNullOrEmpty(input)) return "";
      return string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1));
    }
  }
}
