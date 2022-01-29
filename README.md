# Code generation

This tool uses Razor templating to generate code from a (SQL Server) database model. This is useful for scaffolding out
your domain model, your services layer and/or other parts of your application that should always mirror your data model.

## DNN Platform

The [DNN Platform](https://github.com/dnnsoftware/Dnn.Platform) is a web application framework built on the .net framework.
If you know how to create modules, it is a very powerful tool to build advanced web applications. The default templates
supplied with this tool are intended to help module developers keep their code synchronized with their data model. The
default templates use the [PetaPoco](https://github.com/CollaboratingPlatypus/PetaPoco) patterns used by DNN (sometimes
referred to as DAL2).

## Opinionated

This project was borne out of frustration with limitations of similar tools and EF. And as a result it is highly opinionated.
Below is a list of assumptions and how these work in the tool.

### Module Qualifier

In the settings you supply a ```ModuleObjectQualifier```. This is a string which should end with an underscore and precedes
all DB items that you've created for your module. Tables, Views, Stored Procedures.

### Widget vs Widgets

The tool assumes your tables use the naming convention of plural for the table. So Widgets, NOT Widget. This will
automatically generate objects called "Widget" for your code.

### Tables and Views

The tool assumes that if there is a View that is called "vw_MyModule_Widgets" and a table called "MyModule_Widgets" that 
these two are related. In fact it will assume that the view columns are a superset of the table columns. In the default
templates this will generate a "WidgetBase" class tied to the table and a "Widget" class tied to the view. This encourages
using the view for data retrieval and the table for data addition and updating.

## Installation

Install this tool as a dotnet cli tool as follows:

```
dotnet tool install --global Bring2mind.CodeGen.Cli
```

You should then be able to run the tool in the directory of your project

```
codegen
```

## Configuration

Use the .codegen.json file generated by the tool to fill in the following:

| Parameter | Description |
| --------- | -------- |
| Template | A full path to the first template to run. Note that templates can call other templates, so you only need one as an entry point |
| OutputDirectory | Relative path to where you wish the generator to write files |
| OrgName | Organization name |
| ModuleName | Module name |
| RootNameSpace | Root namespace for your code |
| SiteConfig | Full path to the web.config of the site you're using to develop on. If specified the tool will use this to parse the connection string, db owner and object qualifier. If not specified use those parameters below |
| ConnectionString | Connectionstring of the SQL server on which you model your data |
| ObjectQualifier | ObjectQualifier used in that database if any. Leave blank if not used. |
| DatabaseOwner | Database owner of the schema. Normally dbo. |
| ModuleObjectQualifier | Module's object qualifier (see above) that helps the tool to parse out which objects to use for code generation |
| EnumValues | If some columns need to be mapped to Enums, you can specify that here. The code generator can insert the right type in code. |
| IncludeSqlScripts | Include SqlDataProvider scripts. These will be written to a "Scripts" folder under the OutputDirectory. This tool will create a script that generates the tables and keys (Install), a script with views, functions and sprocs (Upgrade), and finally an uninstall script. |

## Templates

You can find a collection of templates in a zip file with each release of this tool on the [Github page of this project](https://github.com/Bring2mind-net/codegen-cli).

## SQL Scripts

If selected the tool will also create install, upgrade and uninstall scripts for your project and write them to the "Scripts" folder. This is divided as follows:

| Script | Included items | Role |
| ------- | ------- | ------- |
| Install.SqlDataProvider | Tables, Primary and Foreign Keys and Triggers | Use this script to create an install script in DNN. |
| Upgrade.SqlDataProvider | Views, Functions and Stored Procedures | If you include this script as an upgrade script in your manifest, DNN will run this for every upgrade. The generated script will first delete and then install all items. |
| Uninstall.SqlDataProvider | All | This script will uninstall all items. |

Note this is not "templateable", but it's included in this tool as it is useful to generate alongside your DAL code.

## Building This Project

If you wish to fork this project and work on it, compile it using

```
dotnet pack
```

and install it on your dev machine using

```
dotnet tool install --global --add-source ./nupkg Bring2mind.CodeGen.Cli
```

## Uninstall

In the unlikely event you'll want to uninstall this tool, use the following:

```
dotnet tool uninstall -g Bring2mind.CodeGen.Cli
```

