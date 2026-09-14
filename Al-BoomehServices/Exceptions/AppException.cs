using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Al_BoomehServices
{
    public abstract class AppException : Exception
    {
        public int StatusCode { get; }

        protected AppException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}

