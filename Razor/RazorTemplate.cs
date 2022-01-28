namespace Bring2mind.CodeGen.Cli.Razor
{
  using Bring2mind.CodeGen.Cli.Common;
  using Bring2mind.CodeGen.Cli.Data;
  using RazorLight;

  public abstract class RazorTemplate : TemplatePage
  {
    public Database DnnDb { get; set; } = Database.Instance;
    public Settings Settings { get; set; } = Settings.Instance;
    public RazorEngine Engine { get; set; } = RazorEngine.Instance;
  }

  public abstract class RazorTemplate<T> : TemplatePage<T>
  {
    public Database DnnDb { get; set; } = Database.Instance;
    public Settings Settings { get; set; } = Settings.Instance;
    public RazorEngine Engine { get; set; } = RazorEngine.Instance;
  }
}
