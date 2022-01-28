using Bring2mind.CodeGen.Cli.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Text.RegularExpressions;

namespace Bring2mind.CodeGen.Cli.Data
{
  public class ObjectDefinition
  {
    public Table Table { get; set; }
    public List<Column> TableColumns { get; set; } = new List<Column>();

    public View View { get; set; }
    public List<Column> UniqueViewColumns { get; set; } = new List<Column>();
    public string ViewName { get; set; } = "";

    // Localization
    public bool IsLocalized { get; set; } = false;
    public ObjectDefinition Localization { get; set; }
    public List<string> LocalizationTableColumns { get; set; } = new List<string>();

    // Naming properties
    public string Name { get; set; }
    public string OriginalName { get; set; }
    public string Prefix { get; set; }
    public string ModuleQualifier { get; set; }
    public string SingularName { get; set; }
    public string SingularNameLowered { get; set; }
    public string PluralName { get; set; }
    public string PluralNameLowered { get; set; }
    public string Abbreviation { get; set; }

    public string Scope { get; set; } = "";
    public string PrimaryKey { get; set; } = "";
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

    public bool HasAuditFields { get; set; } = false;
    public bool IsLinkTableWithFields { get; set; } = false;
    public bool IsLinkTableWithoutFields { get; set; } = false;
    public bool HasNoPrimaryKey { get; set; } = false;
    public bool HasIdPrimaryKey { get; set; } = false;

    public bool HasScope
    {
      get
      {
        return !string.IsNullOrEmpty(Scope);
      }
    }
    public bool HasTable
    {
      get
      {
        return (Table != null);
      }
    }
    public bool HasView
    {
      get
      {
        return (View != null);
      }
    }
    public bool TableOnly
    {
      get
      {
        return (HasTable & !HasView);
      }
    }
    public bool ViewOnly
    {
      get
      {
        return (!HasTable & HasView);
      }
    }
    public bool TableAndView
    {
      get
      {
        return (HasTable & HasView);
      }
    }

    public ObjectDefinition(Table t, Match m)
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

    public ObjectDefinition(View v, Match m)
    {
      View = v;
      ParseName(v.Name, m);
      UniqueViewColumns = (from c in v.Columns.Cast<Column>()
                           select c).ToList();
      HasNoPrimaryKey = UniqueViewColumns.Where(c => c.InPrimaryKey).Count() == 0;
    }

    public void SetView(View v)
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

    public void ParseName(string name, Match m)
    {
      OriginalName = name;
      Prefix = m.Groups["prefix"].Value;
      ModuleQualifier = m.Groups["modqualifier"].Value;
      Name = m.Groups["name"].Value;
      var p = new Pluralization();
      PluralName = p.Pluralize(Name);
      PluralNameLowered = PluralName.Lowered();
      SingularName = p.Singularize(Name);
      SingularNameLowered = SingularName.Lowered();
      Abbreviation = GetAbbr(Name);
    }

    public string GetAbbr(string input)
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
                            select c.ColumnParameter(true, true, true, "")).ToList());
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
                            select c.ColumnParameter(false, false, false, "")).ToList());
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
                            select c.ColumnParameter(false, true, true, "")).ToList());
            return string.Join(", ", pList);
          }
      }
    }

    public Dictionary<string, ObjectDefinition> ForeignKeyObjects = new Dictionary<string, ObjectDefinition>();

    public void ProcessForeignKeys(Database db)
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

    public void ProcessChildObjects(Database db)
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

    public void SetLocalizationTable(ObjectDefinition table)
    {
      Localization = table;
      IsLocalized = true;
      LocalizationTableColumns = table.Table.Columns.Cast<Column>().Where(c => !c.InPrimaryKey).Select(c => c.Name).ToList();
    }
  }
}