using Microsoft.SqlServer.Management.Smo;

namespace Bring2mind.CodeGen.Cli.Data.Scripting
{
  internal static class Extensions
  {
    internal static bool MyContains(this ViewCollection viewCollection, string viewName)
    {
      foreach (View view in viewCollection)
      {
        if (view.Name == viewName)
        {
          return true;
        }
      }
      return false;
    }

    internal static bool MyContains(this UserDefinedFunctionCollection functionCollection, string functionName)
    {
      foreach (UserDefinedFunction function in functionCollection)
      {
        if (function.Name == functionName)
        {
          return true;
        }
      }
      return false;
    }

    internal static View View(this ViewCollection viewCollection, string viewName)
    {
      foreach (View view in viewCollection)
      {
        if (view.Name == viewName)
        {
          return view;
        }
      }
      return null;
    }

    internal static UserDefinedFunction UserDefinedFunction(this UserDefinedFunctionCollection functionCollection, string functionName)
    {
      foreach (UserDefinedFunction function in functionCollection)
      {
        if (function.Name == functionName)
        {
          return function;
        }
      }
      return null;
    }
  }
}
