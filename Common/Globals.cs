using Microsoft.SqlServer.Management.Smo;

namespace Bring2mind.CodeGen.Cli.Common
{

  public static class Globals
  {
    public enum ColumnGroup
    {
      All = 0,
      PrimaryKey = 1,
      ForeignKeys = 2,
      NonePrimaryKey = 3
    }

    public static string ReadFile(string filePath)
    {
      if (!File.Exists(filePath)) return "";
      using (var sr = new StreamReader(filePath))
      {
        return sr.ReadToEnd();
      }
    }

    public static void WriteFile(string dir, string relPath, string textToWrite)
    {
      string targetFile = Path.Combine(dir, relPath);
      if (!Directory.Exists(Path.GetDirectoryName(targetFile)))
      {
        Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
      }

      using (StreamWriter f = new StreamWriter(targetFile))
      {
        f.WriteLine(textToWrite);
      }
    }

    public static string GetSqlParameterNumbers(int nrParameters, int startIndex, string separator)
    {
      List<string> res = new List<string>();
      for (int i = 0; i <= nrParameters - 1; i++)
      {
        res.Add("@" + (i + startIndex).ToString());
      }
      return string.Join(separator, res);
    }

    public static string PrimaryKeyParameterList(this Table table)
    {
      if (table == null)
      {
        return "";
      }
      var res = table.Columns.Cast<Column>().Where(c => c.InPrimaryKey).Select(c => c.ColumnParameter());
      return string.Join(", ", res);
    }

    public static string PrimaryKeyParameters(this Table table)
    {
      if (table == null)
      {
        return "";
      }
      var res = table.Columns.Cast<Column>().Where(c => c.InPrimaryKey).Select(c => c.Name);
      return string.Join(", ", res);
    }

    public static string ParameterList(this Table table, ColumnGroup group, bool includeType, bool lowered)
    {
      if (table == null)
      {
        return "";
      }
      return table.ParameterList(group, includeType, lowered, "", ", ");
    }

    public static string ParameterList(this Table table, ColumnGroup group, bool includeType, bool lowered, string parameterPrefix, string separator)
    {
      if (table == null)
      {
        return "";
      }
      var res = table.GetColumns(group).Select(c => c.ColumnParameter(includeType, lowered, parameterPrefix));
      return string.Join(separator, res);
    }

    public static string Parameter(this Table table, string parameterName, bool includeType, bool lowered, string parameterPrefix)
    {
      if (table == null)
      {
        return "";
      }
      return table.Columns.Cast<Column>().Where(c => c.Name == parameterName).Select(c => c.ColumnParameter(includeType, lowered, parameterPrefix)).FirstOrDefault();
    }

    public static string SqlParameterList(this Table table, ColumnGroup group, bool useOrdinals, int startOrdinal, string separator)
    {
      if (table == null)
      {
        return "";
      }

      List<string> res = new List<string>();
      if (useOrdinals)
      {
        int i = startOrdinal;
        foreach (Column c in table.GetColumns(group))
        {
          res.Add(string.Format("{0}=@{1}", c.Name, i));
          i += 1;
        }
      }
      else
      {
        res = table.GetColumns(group).Select(c => string.Format("{0}=@{0}", c.Name)).ToList();
      }
      return string.Join(separator, res);
    }

    public static string ColumnParameter(this Column col, bool includeType, bool lowered, bool camelCase, string parameterPrefix)
    {
      if (col == null)
      {
        return "";
      }

      string res = "";
      if (includeType)
      {
        res += col.DataType.DataTypeToCs() + " ";
      }

      res += parameterPrefix;
      if (lowered)
      {
        res += col.Name.Lowered();
      }
      else if (camelCase)
      {
        res += col.Name.Substring(0, 1).ToLower() + col.Name.Substring(1);
      }
      else
      {
        res += col.Name;
      }

      return res;
    }

    public static string ColumnParameter(this Column col, bool includeType, bool lowered, string parameterPrefix)
    {
      if (col == null)
      {
        return "";
      }

      return col.ColumnParameter(includeType, lowered, false, parameterPrefix);
    }

    public static string ColumnParameter(this Column col)
    {
      if (col == null)
      {
        return "";
      }

      return string.Format("{0} {1}", col.DataType.DataTypeToCs(), col.Name.Lowered());
    }

    public static Column FirstPrimaryKeyParameter(this Table table)
    {
      if (table == null)
      {
        return null/* TODO Change to default(_) if this is not a reference type */;
      }
      return table.Columns.Cast<Column>().Where(c => c.InPrimaryKey).FirstOrDefault();
    }

    public static string NullSuffix(this Column col)
    {
      if (col == null)
      {
        return "";
      }

      if (col.Nullable)
      {
        switch (col.DataType.SqlDataType)
        {
          case SqlDataType.NChar:
          case SqlDataType.NText:
          case SqlDataType.NVarChar:
          case SqlDataType.NVarCharMax:
          case SqlDataType.Text:
          case SqlDataType.Char:
          case SqlDataType.VarChar:
          case SqlDataType.VarCharMax:
            {
              return "";
            }

          default:
            {
              return "?";
            }
        }
      }
      return "";
    }

    public static List<Column> GetColumns(this Table table, ColumnGroup group)
    {
      if (table == null)
      {
        return new List<Column>();
      }

      switch (group)
      {
        case ColumnGroup.PrimaryKey:
          {
            return table.Columns.Cast<Column>().Where(c => c.InPrimaryKey).ToList();
          }

        case ColumnGroup.ForeignKeys:
          {
            return table.ForeignKeys.Cast<ForeignKey>().Select(c => table.Columns[c.Columns[0].Name]).ToList();
          }

        case ColumnGroup.NonePrimaryKey:
          {
            return (from c in table.Columns.Cast<Column>()
                    where !c.InPrimaryKey
                    select c).ToList();
          }

        default:
          {
            return (from c in table.Columns.Cast<Column>()
                    select c).ToList();
          }
      }
    }

    public static string DataTypeToCs(this DataType d)
    {
      switch (d.SqlDataType)
      {
        case SqlDataType.BigInt:
          {
            return "long";
          }

        case SqlDataType.Bit:
          {
            return "bool";
          }

        case SqlDataType.Date:
        case SqlDataType.DateTime:
        case SqlDataType.Time:
        case SqlDataType.SmallDateTime:
          {
            return "DateTime";
          }

        case SqlDataType.Decimal:
        case SqlDataType.Money:
        case SqlDataType.Numeric:
        case SqlDataType.SmallMoney:
          {
            return "decimal";
          }

        case SqlDataType.Int:
          {
            return "int";
          }

        case SqlDataType.Real:
          {
            return "float";
          }
        case SqlDataType.Float:
          {
            return "double";
          }

        case SqlDataType.TinyInt:
          {
            return "byte";
          }

        case SqlDataType.SmallInt:
          {
            return "short";
          }

        case SqlDataType.UniqueIdentifier:
          {
            return "Guid";
          }

        case SqlDataType.Binary:
        case SqlDataType.VarBinary:
        case SqlDataType.Image:
        case SqlDataType.Timestamp:
          {
            return "object";
          }
      }
      return "string";
    }

    public static string DataTypeToJs(this DataType d)
    {
      switch (d.SqlDataType)
      {
        case SqlDataType.BigInt:
        case SqlDataType.Decimal:
        case SqlDataType.Money:
        case SqlDataType.SmallMoney:
        case SqlDataType.Int:
        case SqlDataType.Real:
        case SqlDataType.Numeric:
        case SqlDataType.Float:
        case SqlDataType.Timestamp:
        case SqlDataType.Binary:
        case SqlDataType.TinyInt:
        case SqlDataType.SmallInt:
          {
            return "number";
          }
        case SqlDataType.Bit:
          {
            return "boolean";
          }
        case SqlDataType.Char:
        case SqlDataType.VarChar:
        case SqlDataType.VarCharMax:
        case SqlDataType.NChar:
        case SqlDataType.NText:
        case SqlDataType.NVarChar:
        case SqlDataType.NVarCharMax:
        case SqlDataType.Text:
          {
            return "string";
          }
        case SqlDataType.Date:
        case SqlDataType.DateTime:
        case SqlDataType.SmallDateTime:
        case SqlDataType.Time:
          {
            return "Date";
          }
      }
      return "any";
    }


    public static bool IsTableWithIdColumn(this Table table)
    {
      if (table == null)
      {
        return false;
      }

      List<Column> primaryKey = (from c in table.Columns.Cast<Column>()
                                 where c.InPrimaryKey
                                 select c).ToList();
      if (primaryKey.Count == 1 && primaryKey[0].Identity)
      {
        return true;
      }

      return false;
    }

    public static string Lowered(this string input)
    {
      if (string.IsNullOrEmpty(input))
      {
        return "";
      }

      return input.Substring(0, 1).ToLower() + input.Substring(1);
    }

    public static string EnumName(this Column column)
    {
      try
      {
        var settings = Settings.Instance;
        var tableName = ((Table)column.Parent).Name.Replace(settings.ModuleObjectQualifier + "_", "");
        if (settings.EnumValues.ContainsKey(tableName))
        {
          var c = settings.EnumValues[tableName].FirstOrDefault(ev => ev.Column == column.Name);
          if (c != null)
          {
            return c.Enum;
          }
        }
        return null;
      }
      catch (System.Exception ex)
      {
        return null;
      }
    }

    public static string DataTypeToCsOrEnum(this Column column)
    {
      return column.EnumName() ?? column.DataType.DataTypeToCs();
    }

    public static T GetObject<T>(string filename, T defaultObject, bool createIfNotPresent)
    {
      T res = defaultObject;
      if (File.Exists(filename))
      {
        using (var sr = new StreamReader(filename))
        {
          var list = sr.ReadToEnd();
          res = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(list);
        }
      }
      else if (createIfNotPresent)
      {
        SaveObject(filename, defaultObject);
      }
      return res;
    }

    public static void SaveObject(string filename, object objectToSave)
    {
      using (var sw = new StreamWriter(filename))
      {
        sw.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(objectToSave, Newtonsoft.Json.Formatting.Indented));
      }
    }

    public static string NonEmptyEnsureEndsWith(this string input, string endsWith)
    {
      if (string.IsNullOrEmpty(input)) return input;
      if (!input.EndsWith(endsWith))
      {
        return input + endsWith;
      }
      return input;
    }
  }
}
