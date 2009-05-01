using System;
using NModel.Attributes;
using NModel;
using NModel.Execution;

namespace Payroll
{
    class Employee : LabeledInstance<Employee>
    {
        static Set<Employee> allEmployees = Set<Employee>.EmptySet;
        int salary;

        [Action]
        static void CreateEmployee([Domain("new")] Employee emp)
        { allEmployees = allEmployees.Add(emp); }

        [Action]
        [Domain("allEmployees")]
        void DeleteEmployee()
        { allEmployees = allEmployees.Remove(this); }

        [Action]
        [Domain("allEmployees")]
        void SetSalary(int x)
        { this.salary = x; }

        public override void Initialize()
        { this.salary = 0; }
    }
}


