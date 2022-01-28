namespace Bring2mind.CodeGen.Cli.Common
{
  internal class Help
  {
    internal static void PrintHelpText()
    {
      Console.WriteLine(@"
      This is a Command Line tool to help scaffold out code to build DNN modules. Run it once to generate
      a .codegen.json file which needs to be filled in for this to work. Note that you'll need some templates
      to begin generating code.

      You can specify the path to your project directory as an argument. If not specified the current directory
      will be used.
      ");
    }

  }
}
