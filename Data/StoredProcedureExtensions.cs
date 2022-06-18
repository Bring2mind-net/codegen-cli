using Bring2mind.CodeGen.Cli.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Bring2mind.CodeGen.Cli.Data
{

  /// <summary>
  /// Extensions used to construct c# code from SQL stored procedures
  /// </summary>
  public static class StoredProcedureExtensions
  {

    /// <summary>
    /// Gets a list of parameters for the stored procedure in C#
    /// </summary>
    /// <param name="sproc">SQL stored procedure</param>
    /// <param name="includeType">Include the c# type definition</param>
    /// <param name="lowered">Whether to lower case the first letter of the parameter</param>
    /// <param name="parameterPrefix">String to stick in front of the parameter (e.g. "@")</param>
    /// <param name="separator">Seperator between parameters</param>
    /// <returns></returns>
    public static string ParameterList(this StoredProcedure sproc, bool includeType, bool lowered, string parameterPrefix, string separator)
    {
      if (sproc == null)
      {
        return "";
      }

      List<string> res = (from c in sproc.Parameters.Cast<StoredProcedureParameter>()
                          select c.ParameterParameter(includeType, lowered, parameterPrefix, true)).ToList();
      return string.Join(separator, res);
    }

    /// <summary>
    /// Returns parameter string for single sproc parameter
    /// </summary>
    /// <param name="param">SQL stored procedure parameter</param>
    /// <param name="includeType">Include the c# type definition</param>
    /// <param name="lowered">Whether to lower case the first letter of the parameter</param>
    /// <param name="parameterPrefix">String to stick in front of the parameter (e.g. "@")</param>
    /// <param name="removeLeadingAt">In SQL an "@" is found in front of a parameter, so setting this to true removes this</param>
    /// <returns></returns>
    public static string ParameterParameter(this StoredProcedureParameter param, bool includeType, bool lowered, string parameterPrefix, bool removeLeadingAt)
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
      else
      {
        res += paramName;
      }

      return res;
    }
  }
}
