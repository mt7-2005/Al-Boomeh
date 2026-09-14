using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Al_BoomehServices
{
    public class ConflictException : AppException
    {
        public ConflictException(string message) : base(409, message) { }
    }
}

