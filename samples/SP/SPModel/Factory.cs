using System.Collections.Generic;
using NModel;
using NModel.Execution;
using NModel.Attributes;

namespace SP
{
  public class Factory
  {
    public static ModelProgram CreateCredits()
    {
      return new LibraryModelProgram(typeof(SP.Credits).Assembly,
        "SP", new Set<string>("Credits"));
    }

    public static ModelProgram CreateCreditsWithParams()
    {
      return new LibraryModelProgram(typeof(SP.Credits).Assembly,
        "SP", new Set<string>("Credits","MessageParameters"));
    }

    public static ModelProgram CreateCommands()
    {
      return new LibraryModelProgram(typeof(SP.Credits).Assembly,
        "SP", new Set<string>("Commands"));
    }

    public static ModelProgram CreateSetup()
    {
      return new LibraryModelProgram(typeof(SP.Credits).Assembly,
        "SP", new Set<string>("SetupModel"));
    }

    public static ModelProgram CreateCancellation()
    {
      return new LibraryModelProgram(typeof(SP.Credits).Assembly,
        "SP", new Set<string>("Cancellation"));
    }

  }
}
