namespace Bring2mind.CodeGen.Cli.Razor
{
  using Bring2mind.CodeGen.Cli.Common;
  using Bring2mind.CodeGen.Cli.Data;
  using RazorLight;

  /// <summary>
  /// Base class for all templates giving access to the core objects (DnnDb, Settings, Engine) necessary for rendering
  /// </summary>
  public abstract class RazorTemplate : TemplatePage
  {
    /// <summary>
    /// Data model
    /// </summary>
    public Database DnnDb { get; set; } = Database.Instance;

    /// <summary>
    /// Settings for this project
    /// </summary>
    public Settings Settings { get; set; } = Settings.Instance;

    /// <summary>
    /// Razor processing engine used to render other files
    /// </summary>
    public RazorEngine Engine { get; set; } = RazorEngine.Instance;
  }

  /// <summary>
  /// Base class for all templates giving access to the core objects (DnnDb, Settings, Engine) necessary for rendering
  /// </summary>
  public abstract class RazorTemplate<T> : TemplatePage<T>
  {
    /// <summary>
    /// Data model
    /// </summary>
    public Database DnnDb { get; set; } = Database.Instance;

    /// <summary>
    /// Settings for this project
    /// </summary>
    public Settings Settings { get; set; } = Settings.Instance;

    /// <summary>
    /// Razor processing engine used to render other files
    /// </summary>
    public RazorEngine Engine { get; set; } = RazorEngine.Instance;
  }
}
