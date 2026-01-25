using System.Collections.Generic;

namespace POS.UI.Core.Models
{
    public class ApiValidationError
    {
        public Dictionary<string, string[]> Errors { get; set; }
    }
}
