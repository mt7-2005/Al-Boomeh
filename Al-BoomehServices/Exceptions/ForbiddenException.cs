using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Al_BoomehServices
{
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message) : base(403, message) { }
    }
}

