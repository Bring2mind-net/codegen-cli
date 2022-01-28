using Bring2mind.CodeGen.Cli.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Bring2mind.CodeGen.Cli.Data
{

  public static class StoredProcedureExtensions
  {
    public static string ParameterList(this StoredProcedure sproc, bool includeType, bool lowered, bool camelCase, string parameterPrefix, string separator)
    {
      if (sproc == null)
      {
        return "";
      }

      List<string> res = (from c in sproc.Parameters.Cast<StoredProcedureParameter>()
                          select c.ParameterParameter(includeType, lowered, camelCase, parameterPrefix, true)).ToList();
      return string.Join(separator, res);
    }

    public static string ParameterParameter(this StoredProcedureParameter param, bool includeType, bool lowered, bool camelCase, string parameterPrefix, bool removeLeadingAt)
    {
      if (param == null)
      {
        return "";
      }

      string paramName = param.Name;
      if (removeLeadingAt)
      {
        paramName = paramName.TrimStart('@');
      }

      string res = "";
      if (includeType)
      {
        res += param.DataType.DataTypeToCs() + " ";
      }

      res += parameterPrefix;
      if (lowered)
      {
        res += paramName.Lowered();
      }
      else if (camelCase)
      {
        res += paramName.Substring(0, 1).ToLower() + paramName.Substring(1);
      }
      else
      {
        res += paramName;
      }

      return res;
    }
  }
}
