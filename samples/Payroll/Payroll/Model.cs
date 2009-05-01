using System;
using NModel.Attributes;
using NModel;
using NModel.Execution;

namespace Payroll
{
  [Abstract]
  class Employee : CompoundValue
  {
    public readonly int id;
    public Employee(int id) { this.id = id; }
  }

  static class Contract
  {
    // [ExcludeFromState]
    static int nextId = 1;

    static Set<Employee> allEmployees = Set<Employee>.EmptySet;
    static Map<Employee, int> salary = Map<Employee, int>.EmptyMap;

    static Set<Employee> NextEmployee() { return new Set<Employee>(new Employee(nextId)); }

    [Action]
    static void CreateEmployee([Domain("NextEmployee")] Employee emp)
    {
      nextId = nextId + 1;
      allEmployees = allEmployees.Add(emp);
      salary = salary.Add(emp, 0);   // default salary
    }

    [Action]
    static void DeleteEmployee([Domain("allEmployees")] Employee emp)
    {
      allEmployees = allEmployees.Remove(emp);
      salary = salary.RemoveKey(emp);
    }

    [Action]
    static void SetSalary([Domain("allEmployees")] Employee emp, int x)
    {
      salary = salary.Override(emp, x);
    }
  }

    static class Program
    {
        static public ModelProgram Create()
        {
            return LibraryModelProgram.Create(typeof(Program));
        }
    }
}


