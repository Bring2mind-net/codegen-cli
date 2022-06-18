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

    /// <summary>
    /// Reads the contents of a text file and returns it as a string
    /// </summary>
    /// <param name="filePath">Full path to file</param>
    /// <returns></returns>
    public static string ReadFile(string filePath)
    {
      if (!File.Exists(filePath)) return "";
      using (var sr = new StreamReader(filePath))
      {
        return sr.ReadToEnd();
      }
    }

    /// <summary>
    /// Creates a file and writes a string to it
    /// </summary>
    /// <param name="dir">Base directory</param>
    /// <param name="relPath">Relative path to file including filename</param>
    /// <param name="textToWrite">String to write to the file</param>
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

    /// <summary>
    /// Get a string of @n parameters
    /// </summary>
    /// <param name="nrParameters">Number to get</param>
    /// <param name="startIndex">Number to start from (normally 0)</param>
    /// <param name="separator">Separator between parameters (normally a comma)</param>
    /// <returns></returns>
    public static string GetSqlParameterNumbers(int nrParameters, int startIndex, string separator)
    {
      List<string> res = new List<string>();
      for (int i = 0; i <= nrParameters - 1; i++)
      {
        res.Add("@" + (i + startIndex).ToString());
      }
      return string.Join(separator, res);
    }

    /// <summary>
    /// Gets a comma separated list with the primary key parameters as a list of c# parameters for a function
    /// (i.e. with the type declaration)
    /// </summary>
    /// <param name="table">Table from which to get the primary key</param>
    /// <returns></returns>
    public static string PrimaryKeyParameterList(this Table table)
    {
      if (table == null)
      {
        return "";
      }
      var res = table.Columns.Cast<Column>().Where(c => c.InPrimaryKey).Select(c => c.ColumnParameter());
      return string.Join(", ", res);
    }

    /// <summary>
    /// Gets a comma separated list of primary key parameters to use in calling a function (without type declaration)
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public static string PrimaryKeyParameters(this Table table)
    {
      if (table == null)
      {
        return "";
      }
      var res = table.Columns.Cast<Column>().Where(c => c.InPrimaryKey).Select(c => c.Name);
      return string.Join(", ", res);
    }

    /// <summary>
    /// Gets a comma separated list of parameters from a group of columns
    /// </summary>
    /// <param name="table">Table from which to get the columns</param>
    /// <param name="group">The kind of group (e.g. PrimaryKey)</param>
    /// <param name="includeType">Whether to include the type declaration</param>
    /// <param name="lowered">Whether to lowercase each column name's first letter</param>
    /// <returns></returns>
    public static string ParameterList(this Table table, ColumnGroup group, bool includeType, bool lowered)
    {
      if (table == null)
      {
        return "";
      }
      return table.ParameterList(group, includeType, lowered, "", ", ");
    }

    /// <summary>
    /// Gets a comma separated list of parameters from a group of columns
    /// </summary>
    /// <param name="table">Table from which to get the columns</param>
    /// <param name="group">The kind of group (e.g. PrimaryKey)</param>
    /// <param name="includeType">Whether to include the type declaration</param>
    /// <param name="lowered">Whether to lowercase each column name's first letter</param>
    /// <param name="parameterPrefix">Prefix to put before each parameter</param>
    /// <param name="separator">Separator to use for the list</param>
    /// <returns></returns>
    public static string ParameterList(this Table table, ColumnGroup group, bool includeType, bool lowered, string parameterPrefix, string separator)
    {
      if (table == null)
      {
        return "";
      }
      var res = table.GetColumns(group).Select(c => c.ColumnParameter(includeType, lowered, parameterPrefix));
      return string.Join(separator, res);
    }

    /// <summary>
    /// Gets a single column as a string parameter
    /// </summary>
    /// <param name="table">Table from which to get the column</param>
    /// <param name="parameterName">Name of the column</param>
    /// <param name="includeType">Whether to include the type declaration</param>
    /// <param name="lowered">Whether to lowercase the columnname's first letter</param>
    /// <param name="parameterPrefix">Text to add after the parameter</param>
    /// <returns></returns>
    public static string Parameter(this Table table, string parameterName, bool includeType, bool lowered, string parameterPrefix)
    {
      if (table == null)
      {
        return "";
      }
      return table.Columns.Cast<Column>().Where(c => c.Name == parameterName).Select(c => c.ColumnParameter(includeType, lowered, parameterPrefix)).FirstOrDefault();
    }

    /// <summary>
    /// List of colname=@colname parameters for SQL
    /// </summary>
    /// <param name="table">Table from which to get the column</param>
    /// <param name="group">The kind of group (e.g. PrimaryKey)</param>
    /// <param name="useOrdinals">Whether to use numbers for the parameters like colname=@0</param>
    /// <param name="startOrdinal">Starting ordnial number in case of using ordinals</param>
    /// <param name="separator">Separator between parameters</param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets a single column parameter statement
    /// </summary>
    /// <param name="col">Column to get the statement for</param>
    /// <param name="includeType">Whether to include the type declaration</param>
    /// <param name="lowered">Whether to lowercase the columnname's first letter</param>
    /// <param name="parameterPrefix">Prexif for parameter like @ symbol</param>
    /// <returns></returns>
    public static string ColumnParameter(this Column col, bool includeType, bool lowered, string parameterPrefix)
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
      else
      {
        res += col.Name;
      }

      return res;
    }

    /// <summary>
    /// Get a simple C# "type colName" for a column
    /// </summary>
    /// <param name="col">Column to get the parameter for</param>
    /// <returns></returns>
    public static string ColumnParameter(this Column col)
    {
      if (col == null)
      {
        return "";
      }

      return string.Format("{0} {1}", col.DataType.DataTypeToCs(), col.Name.Lowered());
    }

    /// <summary>
    /// Gets first primary key column from a table
    /// </summary>
    /// <param name="table">Table for which to get the column</param>
    /// <returns></returns>
    public static Column FirstPrimaryKeyParameter(this Table table)
    {
      if (table == null)
      {
        return null/* TODO Change to default(_) if this is not a reference type */;
      }
      return table.Columns.Cast<Column>().Where(c => c.InPrimaryKey).FirstOrDefault();
    }

    /// <summary>
    /// Gets the suffix for the C# data type in case it's nullable. Mostly "?" but sometimes it's an empty string.
    /// </summary>
    /// <param name="col">Column for which to get the suffix</param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets a list of columns for a group
    /// </summary>
    /// <param name="table">Table from which to get the columns</param>
    /// <param name="group">Which group of columns to get</param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets the C# data type for an SQL data type
    /// </summary>
    /// <param name="d">SQL data type</param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets a Typescript data type for an SQL data type
    /// </summary>
    /// <param name="d">SQL data type</param>
    /// <returns></returns>
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


    /// <summary>
    /// Determines if the table has a primary key of one column that is auto incrementing
    /// </summary>
    /// <param name="table">Table to test</param>
    /// <returns></returns>
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

    /// <summary>
    /// Lower cases the first letter of a string
    /// </summary>
    /// <param name="input">String to lower case the first letter</param>
    /// <returns></returns>
    public static string Lowered(this string input)
    {
      if (string.IsNullOrEmpty(input))
      {
        return "";
      }

      return input.Substring(0, 1).ToLower() + input.Substring(1);
    }

    /// <summary>
    /// Checks the list of known enums to see if the column should be an enum
    /// </summary>
    /// <param name="column">Column to check</param>
    /// <returns></returns>
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

    /// <summary>
    /// Get data type of column or enum if it is one
    /// </summary>
    /// <param name="column">Column to get type for</param>
    /// <returns></returns>
    public static string DataTypeToCsOrEnum(this Column column)
    {
      return column.EnumName() ?? column.DataType.DataTypeToCs();
    }

    /// <summary>
    /// Reads the contents of a json file and converts this to the desired object
    /// </summary>
    /// <typeparam name="T">Type to cast file contents to</typeparam>
    /// <param name="filename">Full path to file</param>
    /// <param name="defaultObject">Default object in case the file is not present</param>
    /// <param name="createIfNotPresent">If true then writes the default object out to the file if the file is not present</param>
    /// <returns></returns>
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

    /// <summary>
    /// Save an object as json to a file
    /// </summary>
    /// <param name="filename">Full path to file</param>
    /// <param name="objectToSave">Object to serialize as json and save</param>
    public static void SaveObject(string filename, object objectToSave)
    {
      using (var sw = new StreamWriter(filename))
      {
        sw.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(objectToSave, Newtonsoft.Json.Formatting.Indented));
      }
    }

    /// <summary>
    /// Check to see if input is not empty then if it ends with string
    /// </summary>
    /// <param name="input">String to check</param>
    /// <param name="endsWith">Character(s) the string should end with</param>
    /// <returns></returns>
    public static string NonEmptyEnsureEndsWith(this string input, string endsWith)
    {
      if (string.IsNullOrEmpty(input)) return input;
      if (!input.EndsWith(endsWith))
      {
        return input + endsWith;
      }
      return input;
    }

    /// <summary>
    /// Check to see if string is empty and if not ensure the string doesn't end with string
    /// </summary>
    /// <param name="input">String to check</param>
    /// <param name="endsWith">Character(s) to cut off from end</param>
    /// <returns></returns>
    public static string NonEmptyEnsureDoesNotEndWith(this string input, string endsWith)
    {
      if (string.IsNullOrEmpty(input)) return input;
      if (input.EndsWith(endsWith))
      {
        return input.Substring(0, input.Length - endsWith.Length);
      }
      return input;
    }
  }
}
