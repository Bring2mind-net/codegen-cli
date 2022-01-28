namespace Bring2mind.CodeGen.Cli.Razor
{
  using Bring2mind.CodeGen.Cli.Common;
  using Bring2mind.CodeGen.Cli.Data;
  using RazorLight;

  public class RazorEngine
  {
    private static readonly Lazy<RazorEngine>
        lazy =
        new Lazy<RazorEngine>
            (() => new RazorEngine());

    public static RazorEngine Instance { get { return lazy.Value; } }

    private RazorEngine()
    {
    }

    public RazorLightEngine engine { get; set; }

    public void LoadEngine(string templatesPath)
    {
      engine = new RazorLightEngineBuilder()
       .UseFileSystemProject(new DirectoryInfo(templatesPath).FullName)
       .UseMemoryCachingProvider()
       .Build();
    }

    public void RenderTemplate(string template, string fileRelativePath)
    {
      try
      {
        RenderTemplateAsync(template, fileRelativePath).Wait();
      }
      catch (Exception ex)
      {
        throw new Exception(string.Format("Error in template {0}", template), ex);
      }
    }
    async Task RenderTemplateAsync(string template, string fileRelativePath)
    {
      string res = await engine.CompileRenderAsync(template, "");
      Globals.WriteFile(Settings.Instance.OutputDirectory, fileRelativePath, res);
    }

    public void RenderTemplate(string template, string fileRelativePath, ObjectDefinition obj)
    {
      try
      {
        RenderTemplateAsync(template, fileRelativePath, obj).Wait();
      }
      catch (Exception ex)
      {
        throw new Exception(string.Format("Error in template {0} for object {1}", template, obj.Name), ex);
      }
    }
    async Task RenderTemplateAsync(string template, string fileRelativePath, ObjectDefinition obj)
    {
      string res = await engine.CompileRenderAsync(template, obj);
      Globals.WriteFile(Settings.Instance.OutputDirectory, fileRelativePath, res);
    }

    public void RenderTemplate(string template, string fileRelativePath, Dictionary<string, SprocDefinition> obj)
    {
      try
      {
        RenderTemplateAsync(template, fileRelativePath, obj).Wait();
      }
      catch (Exception ex)
      {
        throw new Exception(string.Format("Error in template {0}", template), ex);
      }
    }
    async Task RenderTemplateAsync(string template, string fileRelativePath, Dictionary<string, SprocDefinition> obj)
    {
      string res = await engine.CompileRenderAsync(template, obj);
      Globals.WriteFile(Settings.Instance.OutputDirectory, fileRelativePath, res);
    }

    public string RunCompile(string template, object obj)
    {
      var res = RunCompileAsync(template, obj).Result;
      return res;
    }

    async Task<string> RunCompileAsync(string template, object obj)
    {
      return await engine.CompileRenderAsync(template, obj);
    }

    public string RunCompile(string template, object obj, dynamic viewBag)
    {
      var res = RunCompileAsync(template, obj, viewBag).Result;
      return res;
    }
    async Task<string> RunCompileAsync(string template, object obj, dynamic viewBag)
    {
      return await engine.CompileRenderAsync(template, obj, viewBag);
    }
  }
}
