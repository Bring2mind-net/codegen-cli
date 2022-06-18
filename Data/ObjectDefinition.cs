using Bring2mind.CodeGen.Cli.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Text.RegularExpressions;

namespace Bring2mind.CodeGen.Cli.Data
{
  /// <summary>
  /// Class containing all aspects of an object in this project
  /// </summary>
  public class ObjectDefinition
  {
    /// <summary>
    /// SQL table associated with this object, can be null if just a view
    /// </summary>
    public Table Table { get; set; }

    /// <summary>
    /// List of table's columns. Excludes audit columns CreatedByUserID, CreatedOnDate, LastModifiedByUserID and LastModifiedOnDate.
    /// </summary>
    public List<Column> TableColumns { get; set; } = new List<Column>();

    /// <summary>
    /// SQL View associated with this object, can be null if only a table
    /// </summary>
    public View View { get; set; }

    /// <summary>
    /// List of columns in the view not present in the table
    /// </summary>
    public List<Column> UniqueViewColumns { get; set; } = new List<Column>();

    /// <summary>
    /// Full name of the view
    /// </summary>
    public string ViewName { get; set; } = "";

    // Localization
    public bool IsLocalized { get; set; } = false;
    public ObjectDefinition Localization { get; set; }
    public List<string> LocalizationTableColumns { get; set; } = new List<string>();

    // Naming properties

    /// <summary>
    /// Object name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Full name of the object
    /// </summary>
    public string OriginalName { get; set; }

    /// <summary>
    /// Detected prefix if present (e.g. "vw_")
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// The qualifier used for this project without trailing underscore
    /// </summary>
    public string ModuleQualifier { get; set; }

    /// <summary>
    /// Name => Singular
    /// </summary>
    public string SingularName { get; set; }

    /// <summary>
    /// Singular name with the first letter lowercased
    /// </summary>
    public string SingularNameLowered { get; set; }

    /// <summary>
    /// Name => Plural
    /// </summary>
    public string PluralName { get; set; }

    /// <summary>
    /// Plural name with the first letter lowercased
    /// </summary>
    public string PluralNameLowered { get; set; }

    /// <summary>
    /// Name => abbreviated using upper case letters in the name
    /// </summary>
    public string Abbreviation { get; set; }

    /// <summary>
    /// Detected scope (ModuleId or PortalId)
    /// </summary>
    public string Scope { get; set; } = "";

    /// <summary>
    /// Name of the primary key
    /// </summary>
    public string PrimaryKey { get; set; } = "";

    /// <summary>
    /// Returns Singular name and then "Base" behind it if a view has been detected
    /// </summary>
    public string TableObjectName
    {
      get
      {
        string res = SingularName;
        if (HasView)
        {
          res += "Base";
        }

        return res;
      }
    }

    /// <summary>
    /// True if CreatedByUserID, CreatedOnDate, LastModifiedByUserID and LastModifiedOnDate have been detected
    /// </summary>
    public bool HasAuditFields { get; set; } = false;

    /// <summary>
    /// True if table has two columns pointing to primary keys of two other tables and extra columns
    /// </summary>
    public bool IsLinkTableWithFields { get; set; } = false;

    /// <summary>
    /// True if table just has two columns pointing to primary keys of two other tables
    /// </summary>
    public bool IsLinkTableWithoutFields { get; set; } = false;

    /// <summary>
    /// True if table has no primary key
    /// </summary>
    public bool HasNoPrimaryKey { get; set; } = false;

    /// <summary>
    /// True if the primary key is a single column which auto increments
    /// </summary>
    public bool HasIdPrimaryKey { get; set; } = false;

    /// <summary>
    /// True if a scope value has been found
    /// </summary>
    public bool HasScope
    {
      get
      {
        return !string.IsNullOrEmpty(Scope);
      }
    }

    /// <summary>
    /// True if a table underlies this object
    /// </summary>
    public bool HasTable
    {
      get
      {
        return (Table != null);
      }
    }

    /// <summary>
    /// True if a view underlies this object
    /// </summary>
    public bool HasView
    {
      get
      {
        return (View != null);
      }
    }

    /// <summary>
    /// True if only a table underlies this object
    /// </summary>
    public bool TableOnly
    {
      get
      {
        return (HasTable & !HasView);
      }
    }

    /// <summary>
    /// True if only a view underlies this object
    /// </summary>
    public bool ViewOnly
    {
      get
      {
        return (!HasTable & HasView);
      }
    }

    /// <summary>
    /// True if both a table and view are used for this object
    /// </summary>
    public bool TableAndView
    {
      get
      {
        return (HasTable & HasView);
      }
    }

    internal ObjectDefinition(Table t, Match m)
    {
      Table = t;
      ParseName(t.Name, m);
      HasAuditFields = Table.Columns.Contains("CreatedByUserID") & Table.Columns.Contains("CreatedOnDate") & Table.Columns.Contains("LastModifiedByUserID") & Table.Columns.Contains("LastModifiedOnDate");
      if (HasAuditFields)
      {
        TableColumns = (from c in Table.Columns.Cast<Column>()
                        where c.Name != "CreatedByUserID" & c.Name != "CreatedOnDate" & c.Name != "LastModifiedByUserID" & c.Name != "LastModifiedOnDate"
                        select c).ToList();
      }
      else
      {
        TableColumns = (from c in Table.Columns.Cast<Column>()
                        select c).ToList();
      }

      string LocalizationTableName = SingularName + "Localizations";


      IsLinkTableWithFields = TableColumns.Where(c => c.InPrimaryKey).Count() > 1 & TableColumns.Where(c => !c.InPrimaryKey).Count() > 0;
      IsLinkTableWithoutFields = Enumerable.Where(TableColumns, c => c.InPrimaryKey).Count() > 1 & Enumerable.Where(TableColumns, c => !c.InPrimaryKey).Count() == 0;
      HasNoPrimaryKey = Enumerable.Where(TableColumns, c => c.InPrimaryKey).Count() == 0;
      if (Enumerable.Where(TableColumns, c => c.InPrimaryKey).Count() == 1)
      {
        Column key = TableColumns.FirstOrDefault(c => c.InPrimaryKey);
        HasIdPrimaryKey = key.Identity;
        PrimaryKey = key.Name;
      }

      if ((from c in Table.Columns.Cast<Column>()
           where c.Name.ToLower() == "moduleid"
           select c).Count() == 1)
      {
        Scope = "ModuleId";
      }
      else if ((from c in Table.Columns.Cast<Column>()
                where c.Name.ToLower() == "portalid"
                select c).Count() == 1)
      {
        Scope = "PortalId";
      }
    }

    internal ObjectDefinition(View v, Match m)
    {
      View = v;
      ParseName(v.Name, m);
      UniqueViewColumns = (from c in v.Columns.Cast<Column>()
                           select c).ToList();
      HasNoPrimaryKey = UniqueViewColumns.Where(c => c.InPrimaryKey).Count() == 0;
    }

    internal void SetView(View v)
    {
      View = v;
      ViewName = v.Name;
      UniqueViewColumns = (from c in v.Columns.Cast<Column>()
                           where Table.Columns[c.Name] == null
                           select c).ToList();
      if ((from c in Table.Columns.Cast<Column>()
           where c.Name.ToLower() == "moduleid"
           select c).Count() == 1)
      {
        Scope = "ModuleId";
      }
      else if ((from c in Table.Columns.Cast<Column>()
                where c.Name.ToLower() == "portalid"
                select c).Count() == 1)
      {
        Scope = "PortalId";
      }
    }

    internal void ParseName(string name, Match m)
    {
      OriginalName = name;
      Prefix = m.Groups["prefix"].Value;
      ModuleQualifier = m.Groups["modqualifier"].Value;
      Name = m.Groups["name"].Value.FirstCharToUpper();
      var p = new Pluralization();
      PluralName = p.Pluralize(Name);
      PluralNameLowered = PluralName.Lowered();
      SingularName = p.Singularize(Name);
      SingularNameLowered = SingularName.Lowered();
      Abbreviation = GetAbbr(Name);
    }

    private string GetAbbr(string input)
    {
      string res = input.Substring(0, 1);
      foreach (char c in (input.Substring(2)).ToCharArray())
      {
        if (char.IsUpper(c))
        {
          res += c;
        }
      }
      if (res.Length == input.Length)
      {
        res = input.Substring(1);
      }

      res = res.ToLower();
      return res;
    }

    public string GetScopeDeclaration(bool camelCase, bool includeType, bool leadingComma, bool trailingComma)
    {
      string sc = Scope;
      if (sc == "")
      {
        return "";
      }

      if (camelCase)
      {
        sc = sc.Substring(0, 1).ToLower() + sc.Substring(1);
      }

      string res = "";
      if (leadingComma)
      {
        res = ", ";
      }

      if (includeType)
      {
        res += "int ";
      }

      res += sc;
      if (trailingComma)
      {
        res += ", ";
      }

      return res;
    }

    private void AddScopeDeclaration(ref List<string> list, bool camelCase, bool includeType, bool leadingComma, bool trailingComma)
    {
      string d = GetScopeDeclaration(camelCase, includeType, leadingComma, trailingComma);
      if (d != "")
      {
        list.Add(d);
      }
    }

    public enum ParameterListType
    {
      Plain,
      FunctionDeclaration,
      SqlWhereClause
    }

    public string GetParameterList(bool includeScope, bool onlyPrimaryKey, ParameterListType listType)
    {
      List<string> pList = new List<string>();
      switch (listType)
      {
        case ParameterListType.FunctionDeclaration:
          {
            if (includeScope)
            {
              AddScopeDeclaration(ref pList, true, true, false, false);
            }

            pList.AddRange((from c in Table.Columns.Cast<Column>()
                            where c.InPrimaryKey | !onlyPrimaryKey
                            select c.ColumnParameter(true, true, "")).ToList());
            return string.Join(", ", pList);
          }

        case ParameterListType.SqlWhereClause:
          {
            if (includeScope)
            {
              AddScopeDeclaration(ref pList, false, false, false, false);
            }

            pList.AddRange((from c in Table.Columns.Cast<Column>()
                            where c.InPrimaryKey | !onlyPrimaryKey
                            select c.ColumnParameter(false, false, "")).ToList());
            int i = 0;
            List<string> outList = new List<string>();
            foreach (string el in pList)
            {
              outList.Add(el + " = @" + i.ToString());
              i += 1;
            }
            return "WHERE " + string.Join(" AND ", outList);
          }

        default:
          {
            if (includeScope)
            {
              AddScopeDeclaration(ref pList, true, false, false, false);
            }

            pList.AddRange((from c in Table.Columns.Cast<Column>()
                            where c.InPrimaryKey | !onlyPrimaryKey
                            select c.ColumnParameter(false, true, "")).ToList());
            return string.Join(", ", pList);
          }
      }
    }

    public Dictionary<string, ObjectDefinition> ForeignKeyObjects = new Dictionary<string, ObjectDefinition>();

    internal void ProcessForeignKeys(Database db)
    {
      if (!HasTable)
      {
        return;
      }

      foreach (ForeignKey fk in Table.ForeignKeys)
      {
        string refTable = fk.ReferencedTable;
        if (db.Objects.Values.Where(o => o.OriginalName == refTable).Count() > 0)
        {
          ForeignKeyObjects.Add(fk.Columns[0].Name, db.Objects.Values.FirstOrDefault(o => o.OriginalName == refTable));
        }
        else if (db.ForeignObjects.Values.Where(o => o.OriginalName == refTable).Count() > 0)
        {
          ForeignKeyObjects.Add(fk.Columns[0].Name, db.ForeignObjects.Values.FirstOrDefault(o => o.OriginalName == refTable));
        }
      }
    }

    public Dictionary<string, ObjectDefinition> ChildObjects = new Dictionary<string, ObjectDefinition>();

    internal void ProcessChildObjects(Database db)
    {
      if (!HasTable)
      {
        return;
      }

      foreach (var t in db.Objects.Keys)
      {
        if (t != this.Name)
        {
          foreach (var fko in db.Objects[t].ForeignKeyObjects.Keys)
          {
            if (db.Objects[t].ForeignKeyObjects[fko].Name == this.Name)
            {
              ChildObjects[db.Objects[t].Name] = db.Objects[t];
            }
          }
        }
      }
    }

    internal void SetLocalizationTable(ObjectDefinition table)
    {
      Localization = table;
      IsLocalized = true;
      LocalizationTableColumns = table.Table.Columns.Cast<Column>().Where(c => !c.InPrimaryKey).Select(c => c.Name).ToList();
    }
  }
}