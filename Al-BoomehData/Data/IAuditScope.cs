using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAuditScope
{
    bool IsEnabled { get; }
    IDisposable Enable();
}
