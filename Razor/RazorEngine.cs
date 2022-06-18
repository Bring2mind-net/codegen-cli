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

    /// <summary>
    /// Loads and compiles all cshtml files found for this path
    /// </summary>
    /// <param name="templatesPath">Path to razor templates</param>
    public void LoadEngine(string templatesPath)
    {
      engine = new RazorLightEngineBuilder()
       .UseFileSystemProject(new DirectoryInfo(templatesPath).FullName)
       .UseMemoryCachingProvider()
       .Build();
    }

    /// <summary>
    /// Render a template to a file
    /// </summary>
    /// <param name="template">Name of the template to render</param>
    /// <param name="fileRelativePath">Relative path the file to write</param>
    /// <exception cref="Exception"></exception>
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

    /// <summary>
    /// Render a template to a file
    /// </summary>
    /// <param name="template">Name of the template to render</param>
    /// <param name="fileRelativePath">Relative path the file to write</param>
    /// <param name="obj">Object to use as model</param>
    /// <exception cref="Exception"></exception>
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

    /// <summary>
    /// Render a template to a file
    /// </summary>
    /// <param name="template">Name of the template to render</param>
    /// <param name="fileRelativePath">Relative path the file to write</param>
    /// <param name="obj">Model</param>
    /// <exception cref="Exception"></exception>
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

    /// <summary>
    /// Render template to current output
    /// </summary>
    /// <param name="template">Template to render</param>
    /// <param name="obj">Object to use as model</param>
    /// <returns></returns>
    public string RunCompile(string template, object obj)
    {
      var res = RunCompileAsync(template, obj).Result;
      return res;
    }

    async Task<string> RunCompileAsync(string template, object obj)
    {
      return await engine.CompileRenderAsync(template, obj);
    }

    /// <summary>
    /// Render template to current output
    /// </summary>
    /// <param name="template">Template to render</param>
    /// <param name="obj">Object to use as model</param>
    /// <param name="viewBag">Viewbag to use</param>
    /// <returns></returns>
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
