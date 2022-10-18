using Bring2mind.CodeGen.Cli.Data;
using Bring2mind.CodeGen.Cli.Data.Scripting;
using Bring2mind.CodeGen.Cli.Razor;

var arg = args.AsQueryable().FirstOrDefault();
if (arg != null)
{
  switch (arg.ToLower())
  {
    case "-h":
    case "--help":
      Bring2mind.CodeGen.Cli.Common.Help.PrintHelpText();
      return;
    case "-v":
    case "--version":
      var thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
      Console.Write("v");
      Console.WriteLine(System.Diagnostics.FileVersionInfo.GetVersionInfo(thisAssembly.Location).FileVersion);
      return;
    default:
      if (Directory.Exists(arg))
      {
        Environment.CurrentDirectory = arg;
      }
      break;
  }
}

var settings = Bring2mind.CodeGen.Cli.Common.Settings.Instance;

if (string.IsNullOrEmpty(settings.Template))
{
  Console.WriteLine("Please configure the code generator using the .codegen.json file first");
}
else
{
  Console.WriteLine("DNN Code Generation Starting");
  Console.WriteLine("Loading Database Objects");
  var database = Database.Instance;
  database.Load(settings);
  Console.WriteLine("Loading Template Engine");
  var entryTemplate = new FileInfo(settings.Template);
  if (entryTemplate != null && entryTemplate.Exists)
  {
    var engine = RazorEngine.Instance;
    engine.LoadEngine(entryTemplate.Directory.FullName);
    Console.WriteLine("Start Generating");
    string res = await RazorEngine.Instance.engine.CompileRenderAsync(entryTemplate.FullName, "");
    Console.WriteLine(res);
  }
  else
  {
    Console.WriteLine($"Can't find template {settings.Template}");
  }

  if (settings.IncludeSqlScripts)
  {
    ScriptGenerator.GenerateScripts(database.Server, database.Db, settings);
  }
}

