using Al_BoomehDAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehDAL.Data
{
    public class AuditScope : IAuditScope
    {
        public bool IsEnabled { get; private set; }

        public IDisposable Enable()
        {
            IsEnabled = true;
            return new ActionOnDispose(() => IsEnabled = false);
        }

        private class ActionOnDispose : IDisposable
        {
            private readonly Action _action;
            public ActionOnDispose(Action action) => _action = action;
            public void Dispose() => _action();
        }
    }

}
