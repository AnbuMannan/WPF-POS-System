using System;
using static POS.UI.Modules.Admin.Products.ProductFormView;

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
}
