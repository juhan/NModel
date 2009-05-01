using System;
using System.Collections.Generic;
using System.Text;

namespace PayrollImpl
{
  class XyzEmployee
  {
    int salary = 0;
    bool active = true;

    public void SetSalary(int x) { this.salary = x; }

    public void Delete() { this.active = false; }

    static public XyzEmployee Create() { return new XyzEmployee(); }
  }


}
