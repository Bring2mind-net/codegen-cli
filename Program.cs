using Bring2mind.CodeGen.Cli.Data;
using Bring2mind.CodeGen.Cli.Razor;

Console.WriteLine("DNN Code Generation Starting");

var settings = Bring2mind.CodeGen.Cli.Common.Settings.Instance;

if (string.IsNullOrEmpty(settings.Template))
{
  Console.WriteLine("Please configure the code generator using the .codegen.json file first");
}
else
{
  var database = Database.Instance;
  database.Load(settings);
  var engine = RazorEngine.Instance;
  engine.LoadEngine(Path.GetDirectoryName(settings.Template));

  string res = await RazorEngine.Instance.engine.CompileRenderAsync(settings.Template, "");
  Console.WriteLine(res);
}

