using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehDAL.Data
{
    public interface IAuditSuppressor
    {
        public bool IsSuppressed { get;  }
        IDisposable Suppress();
    }
}
