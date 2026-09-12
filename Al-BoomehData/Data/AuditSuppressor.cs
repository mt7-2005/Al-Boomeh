using Al_BoomehDAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehDAL.Data
{
    public class AuditSuppressor : IAuditSuppressor
    {
        public bool IsSuppressed { get; private set; }

        public IDisposable Suppress()
        {
            IsSuppressed = true;

            return new ActionOnDispose(() =>
            {
                IsSuppressed = false;
            });
        }

       
        private class ActionOnDispose : IDisposable
        {
            private readonly Action _action;

            public ActionOnDispose(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }
    }

}
