using System;

namespace POS.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public string Field { get; }

        public ValidationException(string field, string message) : base(message)
        {
            Field = field;
        }
    }
}
