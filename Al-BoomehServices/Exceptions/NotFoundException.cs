using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Al_BoomehServices
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(404, message) { }
    }
}

