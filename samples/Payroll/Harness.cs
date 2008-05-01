using System;
using System.Collections.Generic;
using NModel.Conformance;
using NModel.Terms;
using PayrollImpl;

namespace PayrollImpl
{
  class Stepper : IStepper
  {
    static Dictionary<Term, XyzEmployee> employees =
               new Dictionary<Term, XyzEmployee>();

    public CompoundTerm DoAction(CompoundTerm action)
    {
      string name = action.FunctionSymbol.ToString();

      switch (name)
      {
        case "CreateEmployee":
          XyzEmployee emp = XyzEmployee.Create();
          employees.Add(action.Arguments[0], emp);
          return null;

        case "DeleteEmployee":
          XyzEmployee emp2 = employees[action.Arguments[0]];
          emp2.Delete();
          return null;

        case "SetSalary":
          XyzEmployee emp3 = employees[action.Arguments[0]];
          int value = (int)((Literal)action.Arguments[1]).Value;
          emp3.SetSalary(value);
          return null;

        default:
          throw new Exception("Unknown action");
      }
    }

    public void Reset() { employees.Clear(); }
  }
}
