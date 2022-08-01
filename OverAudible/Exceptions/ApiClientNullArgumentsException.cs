using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Exceptions
{
    public class ApiClientNullArgumentsException : Exception
    {
        public ApiClientNullArgumentsException() : base()
        {
        }

        public ApiClientNullArgumentsException(string? message) : base(message)
        {
        }

        public ApiClientNullArgumentsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
