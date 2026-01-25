using System;
using System.Collections.Generic;

namespace POS.UI.Core.Exceptions
{
    public class ApiValidationException : Exception
    {
        public ApiValidationError Error { get; }

        public ApiValidationException(ApiValidationError error)
            : base("Validation error from API")
        {
            Error = error;
        }
    }

    public class ApiValidationError
    {
        public Dictionary<string, string[]> Errors { get; set; }
    }
}